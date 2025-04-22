using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WalkingTurret : Enemy
{   
    [Header("Values")]
    [SerializeField] private float defSpeed;

    [Header("Attack")]
    [SerializeField] private float atkDelay;
    private float atkTimer;
    [SerializeField] private int dmg;

    [Header("Missiles")]
    [SerializeField] private int numProj;
    [SerializeField] private float spread;
    [SerializeField] private GameObject projPrefab;

    [Header("Movement")]
    [SerializeField] private float targetMin;
    [SerializeField] private float targetMax;
    private Vector3 target;
    [SerializeField] private bool lineOfSight;
    [SerializeField] private LayerMask terrainLayer;

    [Header("Enemy Spawn")]
    [SerializeField] private float spawnInterval;
    [SerializeField] private GameObject spawnIndicator;
    private List<GameObject> indicators = new List<GameObject>();
    [SerializeField] private int enemiesToSpawn;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private LayerMask spawnLayer;

    
    void Start()
    {
        atkTimer = atkDelay * 0.5f;
        base.Start();
        if (shield != null)
            shielded = true;

        healthBar = GameObject.Find("Boss Fill").GetComponent<Image>();
        for (float i = spawnInterval; i < 1; i+=spawnInterval)
        {
            GameObject indicator = Instantiate(spawnIndicator, Vector2.zero, Quaternion.identity, healthBar.transform.parent);
            indicator.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(-343, 343, i), 0);
            indicators.Add(indicator);
        }

        ChooseTarget();
    }


    void Update()
    {
        base.Update();

        float dist = Vector3.Distance(player.transform.position, transform.position);
        if (dist < aggroRange)
            aggro = true;

        if (!GameManager.Instance.pauseGame && aggro && stunTimer <= 0)
        {
            atkTimer = Mathf.Max(0, atkTimer - Time.deltaTime);            
            float speed = (slowTimer > 0) ? defSpeed*0.3f : defSpeed;
            
            rb.MovePosition(rb.position + (target-rb.position).normalized * speed * Time.deltaTime);
            if (Vector3.Distance(rb.position, target) < 0.5f)
                ChooseTarget();
            //ranged attack
            if (atkTimer <= 0)
            {
                Vector3 dir = Vector3.Scale(player.transform.position - transform.position, new Vector3(1, 0, 1)).normalized;
                StartCoroutine(FireProjectiles(dir));
            }   
        }

        if (health/(maxHealth*1.0f) < spawnInterval*indicators.Count)
        {
            StartCoroutine(SpawnEnemies(enemiesToSpawn - indicators.Count));
            Destroy(indicators[indicators.Count-1]);
            indicators.RemoveAt(indicators.Count-1);
        }
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
        if (shield != null)
        {
            shielded = false;
            shield.SetActive(false);
        }
        atkTimer = atkDelay;
        anim.Play("Attack");
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < numProj; i++)
        {
            GameObject proj = Instantiate(projPrefab, transform.position + dir * 0.5f + new Vector3(0, 1, 0), Quaternion.LookRotation(dir));
            proj.GetComponent<Missile>().dmg = dmg;
            proj.GetComponent<Missile>().dir = dir * 0.5f + new Vector3(0, 2.5f+(0.1f*i), 0);
            proj.GetComponent<Missile>().target = new Vector3(player.transform.position.x, 0, player.transform.position.z) + player.GetComponent<PlayerMovement>().moveDir*5 + Quaternion.Euler(0, Random.Range(0, 360), 0) * new Vector3(Random.Range(0f, spread), 0, 0);
        }
        if (shield != null)
        {
            yield return new WaitForSeconds(2f);
            shield.SetActive(true);
            shielded = true;
        }
    }


    private IEnumerator SpawnEnemies(int n)
    {
        GameManager.Instance.numEnemies += enemiesToSpawn;
        for (int i = 0; i < n; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)).normalized * Random.Range(3, 15) + new Vector3(0, 1, 0);
            int attempts = 0;
            while (Physics.OverlapSphere(transform.position + offset, 0.5f, spawnLayer).Length > 0)
            {
                offset = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)).normalized * Random.Range(3, 15) + new Vector3(0, 1, 0);
                attempts++;
                if (attempts == 10) //fail to find open spot
                {
                    Debug.Log("NO OPEN SPOT :(");
                    GameManager.Instance.numEnemies--;
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


    private void OnDestroy()
    {
        healthBar.transform.parent.gameObject.SetActive(false);
        foreach (Transform child in GameObject.Find("Enemies").transform)
            Destroy(child.gameObject);
        
        GameManager.Instance.UpdateEnemyNum(-GameManager.Instance.numEnemies);
    }
}
