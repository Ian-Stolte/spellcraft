using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WalkingTurret : Enemy
{   
    
    [Header("Movement")]
    [SerializeField] private float defSpeed;
    [SerializeField] private float targetMin;
    [SerializeField] private float targetMax;
    private Vector3 target;
    [SerializeField] private bool lineOfSight;
    [SerializeField] private LayerMask terrainLayer;

    [Header("Attack")]
    [SerializeField] private float atkDelay;
    private float atkTimer;
    [SerializeField] private int dmg;
    [SerializeField] private int numProj;
    [SerializeField] private float spread;
    [SerializeField] private GameObject projPrefab;

    [Header("Stomp")]
    [SerializeField] private float meleeRange;
    [SerializeField] private int stompDmg;
    [SerializeField] private float stompForce;
    [SerializeField] private float stompDelay;
    private float stompTimer;
    [SerializeField] private GameObject stompIndicator;

    [Header("Enemy Spawn")]
    [SerializeField] private float spawnInterval;
    [SerializeField] private GameObject spawnIndicator;
    private List<GameObject> indicators = new List<GameObject>();
    [SerializeField] private int enemiesToSpawn;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private LayerMask spawnLayer;
    
    [Header("Defense")]
    [SerializeField] private GameObject shield;
    [SerializeField] private float shieldTime;

    [Header("Barriers")]
    [SerializeField] private GameObject startBarrier;
    [SerializeField] private GameObject endBarrier;

    public bool finalForm;


    void Start()
    {
        base.Start();
        //AudioManager.Instance.Play("Area 1");
        //StartCoroutine(AudioManager.Instance.StartFade("Area 1", 1, 0.2f));
    }

    void Update()
    {
        base.Update();

        float dist = Vector3.Distance(player.transform.position, transform.position);
        if (dist < aggroRange && !aggro)
        {
            aggro = true;
            StartCoroutine(StartAggro());
        } 

        if (!GameManager.Instance.pauseGame && aggro && stunTimer <= 0)
        {
            //increment timers
            if (dist > meleeRange)
            {
                atkTimer = Mathf.Max(0, atkTimer - Time.deltaTime);
                if (finalForm)
                    stompTimer = Mathf.Max(0, stompTimer - Time.deltaTime);
                else if (stompTimer > 1)
                    stompTimer = Mathf.Max(1, stompTimer - Time.deltaTime);
                else
                    stompTimer = Mathf.Max(0.5f, atkTimer);
            }
            else
            {   
                stompTimer = Mathf.Max(0, stompTimer - Time.deltaTime);
                if (finalForm)
                    atkTimer = Mathf.Max(0, atkTimer - Time.deltaTime);
                else if (atkTimer > 1)
                    atkTimer = Mathf.Max(1, atkTimer - Time.deltaTime);
                else
                    atkTimer = Mathf.Max(0.5f, atkTimer);
            }

            //move randomly
            float speed = (slowTimer > 0) ? defSpeed*0.3f : defSpeed;
            rb.MovePosition(rb.position + (target-rb.position).normalized * speed * Time.deltaTime);
            if (Vector3.Distance(rb.position, target) < 1f)
                ChooseTarget();
            
            if (atkTimer <= 0 && (dist > meleeRange) && !finalForm) //ranged attack
            {
                Vector3 dir = Vector3.Scale(player.transform.position - transform.position, new Vector3(1, 0, 1)).normalized;
                StartCoroutine(FireProjectiles(dir));
            }
            else if (stompTimer <= 0 && (dist < meleeRange) && !finalForm) //melee attack
            {
                stompTimer = stompDelay;
                StartCoroutine(Stomp());
            }
        }

        //spawn enemies @ HP thresholds
        if (health/(maxHealth*1.0f) < spawnInterval*indicators.Count)
        {
            health = (int)Mathf.Round(spawnInterval*indicators.Count * maxHealth);
            StartCoroutine(SpawnEnemies(enemiesToSpawn - indicators.Count));
            StartCoroutine(Shield());
            Destroy(indicators[indicators.Count-1]);
            indicators.RemoveAt(indicators.Count-1);
            if (indicators.Count == 0) //if last tick
            {
                atkDelay = 2.5f;
                stompDelay = 1.5f;
                finalForm = true;
                StartCoroutine(Stomp());
            }
        }
    }


    private IEnumerator StartAggro()
    {
        atkTimer = atkDelay * 0.5f;
        base.Start();

        startBarrier.SetActive(true);
        AudioManager a = AudioManager.Instance;
        foreach (Sound s in a.currentSongs)
            StartCoroutine(a.StartFade(s.name, 1, 0));
        a.Play("Boss 1");
        StartCoroutine(a.StartFade("Boss 1", 1, 0.2f));

        yield return DialogueManager.Instance.GardenerDialogue();

        GameManager.Instance.bossUI.SetActive(true);
        healthBar = GameObject.Find("Boss Fill").GetComponent<Image>();
        for (float i = spawnInterval; i < 1; i+=spawnInterval)
        {
            GameObject indicator = Instantiate(spawnIndicator, Vector2.zero, Quaternion.identity, healthBar.transform.parent);
            indicator.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(-343, 343, i), 0);
            indicators.Add(indicator);
        }
        ChooseTarget();
    }


    private void ChooseTarget()
    {
        do
        {
            target = transform.position + Quaternion.Euler(0, Random.Range(0, 360), 0) * new Vector3(1, 0, 1) * Random.Range(targetMin, targetMax);
            lineOfSight = !Physics.Raycast(transform.position, target-transform.position, Vector3.Distance(target, transform.position), terrainLayer);
        } while (Physics.OverlapSphere(target, 1, terrainLayer).Length > 0 || !lineOfSight);
    }


    private IEnumerator FireProjectiles(Vector3 dir)
    {
        atkTimer = atkDelay;
        anim.Play("Gardener_Open");
        yield return new WaitForSeconds(0.3f);
        AudioManager.Instance.Play("Walking Turret Fire");
        for (int i = 0; i < numProj; i++)
        {
            GameObject proj = Instantiate(projPrefab, transform.position + dir * 0.5f + new Vector3(0, 1, 0), Quaternion.LookRotation(dir));
            proj.GetComponent<Missile>().dmg = dmg;
            proj.GetComponent<Missile>().dir = dir * 0.5f + new Vector3(0, 2.5f+(0.1f*i), 0);
            proj.GetComponent<Missile>().target = new Vector3(player.transform.position.x, 0, player.transform.position.z) + player.GetComponent<PlayerMovement>().moveDir*3 + Quaternion.Euler(0, Random.Range(0, 360), 0) * new Vector3(Random.Range(0f, spread), 0, 0);
        }
        yield return new WaitForSeconds(1);
        if (finalForm)
            StartCoroutine(Stomp());
    }


    private IEnumerator Stomp()
    {
        anim.Play("Gardener_Stomp");
        stompIndicator.SetActive(true);
        float elapsed = 0f;
        while (elapsed < 0.75f)
        {
            stompIndicator.transform.GetChild(0).localScale = new Vector3(1, 1, 1) * Mathf.Lerp(0, 1, elapsed / 0.75f);
            elapsed += Time.deltaTime;
            yield return null;
            if (stunTimer > 0 && !finalForm)
            {
                stompIndicator.SetActive(false);
                yield break;
            }
        }
        AudioManager.Instance.Play("Stomp Impact");
        if (Vector3.Distance(player.transform.position, transform.position) < meleeRange - 1.5f)
        {
            player.GetComponent<PlayerMovement>().TakeDamage(stompDmg);
            Vector3 dir = (player.transform.position - transform.position).normalized + new Vector3(0, 0.5f, 0);
            player.GetComponent<Rigidbody>().AddForce(dir * stompForce, ForceMode.Impulse);
        }
        stompIndicator.SetActive(false);
        if (finalForm)
        {
            Vector3 dir = Vector3.Scale(player.transform.position - transform.position, new Vector3(1, 0, 1)).normalized;
            StartCoroutine(FireProjectiles(dir));
        }
    }


    private IEnumerator SpawnEnemies(int n)
    {
        for (int i = 0; i < n; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)).normalized * Random.Range(3, 15) + new Vector3(0, 1, 0);
            int attempts = 0;
            while (Physics.OverlapSphere(transform.position + offset, 0.5f, spawnLayer).Length > 0 || (transform.position + offset).x < 12 || (transform.position + offset).x > 46)
            {
                offset = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)).normalized * Random.Range(3, 15) + new Vector3(0, 1, 0);
                attempts++;
                if (attempts == 10) //fail to find open spot
                {
                    Debug.Log("NO OPEN SPOT :(");
                    break;
                }
            }
            if (attempts < 10)
            {
                GameObject enemy = Instantiate(enemyPrefab, transform.position + offset + new Vector3(0, 15, 0), Quaternion.identity, GameObject.Find("Enemies").transform);
                enemy.GetComponent<Rigidbody>().velocity = new Vector3(0, -100, 0);
            }
            yield return new WaitForSeconds(1);
        }
    }


    private IEnumerator Shield()
    {
        shield.SetActive(true);
        shielded = true;
        yield return new WaitForSeconds(shieldTime);
        shield.SetActive(false);
        shielded = false;
    }


    private void OnDestroy()
    {
        healthBar.transform.parent.gameObject.SetActive(false);
        foreach (Transform child in GameObject.Find("Enemies").transform)
            Destroy(child.gameObject);
        
        endBarrier.SetActive(false);

        if (!GameManager.Instance.pauseGame)
            AudioManager.Instance.KillBoss1();
    }
}
