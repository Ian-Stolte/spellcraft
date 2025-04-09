using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private int maxHealth;
    public int health;
    [SerializeField] private float speed;

    [Header("States")]
    public int aggroRange;
    private bool aggro;
    [SerializeField] private float retreatRange;
    [SerializeField] private float retreatThreshold;
    [HideInInspector] public float stunTimer;

    [Header("Attack")]
    [SerializeField] private float atkDelay;
    private float atkTimer;
    [SerializeField] private float atkRange;
    [SerializeField] private float meleeRange;
    [SerializeField] private int dmg;
    [SerializeField] private GameObject projPrefab;

    [Header("Canvas")]
    [SerializeField] private Image healthBar;
    [SerializeField] private TextMeshProUGUI statusTxt;
    [SerializeField] private GameObject damageNumber;

    [Header("References")]
    private Rigidbody rb;
    [SerializeField] private Animator anim;
    private GameObject player;
    private Transform cam;


    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
        atkTimer = atkDelay;
        cam = GameObject.Find("Main Camera").transform;
    }

    void Update()
    {
        if (transform.position.y < 1)
            Destroy(GetComponent<TrailRenderer>());
        if (!GameManager.Instance.pauseGame)
        {
            float dist = Vector3.Distance(player.transform.position, transform.position);
            if (dist < aggroRange)
                aggro = true;

            stunTimer -= Time.deltaTime;
            if (aggro && stunTimer <= 0)
            {
                atkTimer = Mathf.Max(0, atkTimer - Time.deltaTime);

                Vector3 dir = Vector3.Scale(player.transform.position - transform.position, new Vector3(1, 0, 1)).normalized;
                transform.rotation = Quaternion.LookRotation(dir);
                if ((health/(1f*maxHealth) <= retreatThreshold)) //ranged mode
                {
                    if (dist < retreatRange)
                        rb.MovePosition(rb.position + transform.forward * -speed * Time.deltaTime);
                    else if (dist > retreatRange + 1)
                        rb.MovePosition(rb.position + transform.forward * speed * Time.deltaTime);

                    if (atkTimer <= 0 && dist < atkRange)
                    {
                        GameObject proj = Instantiate(projPrefab, transform.position + dir * 0.5f, Quaternion.LookRotation(dir));
                        proj.GetComponent<Projectile>().dmg = dmg;
                        proj.GetComponent<Projectile>().dir = dir;
                        anim.Play("Attack");
                        rb.AddForce(dir * -200, ForceMode.Impulse);
                        atkTimer = atkDelay;
                    }   
                }
                else //melee mode
                {
                    if (dist > meleeRange)
                    {
                        rb.MovePosition(rb.position + transform.forward * speed * Time.deltaTime);
                    }
                    if (atkTimer <= 0 && dist < meleeRange)
                    {
                        player.GetComponent<PlayerMovement>().TakeDamage(dmg);
                        anim.Play("Attack");
                        rb.AddForce(dir * -400, ForceMode.Impulse);
                        atkTimer = atkDelay;
                    }
                }
            }

            anim.SetBool("Stunned", stunTimer > 0);
            statusTxt.text = (stunTimer > 0) ? "stunned_" : "";

            transform.GetChild(0).transform.forward = cam.forward;
        }
    }


    public void TakeDamage(int dmg)
    {
        //warn other enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject e in enemies)
        {
            Enemy script = e.GetComponent<Enemy>();
            Vector3 dir = e.transform.position - transform.position;
            float dist = Vector3.Distance(e.transform.position, transform.position);
            if (dist < script.aggroRange/2 && !Physics.Raycast(transform.position, dir, dist, LayerMask.GetMask("Ground")))
            {
                script.TakeDamage(0);
            }
        }

        if (dmg > 0)
        {
            anim.Play("MeleeHit");
            health -= dmg;
            //show damage number
            GameObject dmgNumber = Instantiate(damageNumber, transform.position, Quaternion.identity, transform.GetChild(0));
            dmgNumber.transform.forward = transform.GetChild(0).forward;
            dmgNumber.GetComponent<TextMeshProUGUI>().text = "" + dmg;
            Vector2 randomPos = new Vector2(Random.Range(-100, 100), Random.Range(0, 100));
            dmgNumber.GetComponent<RectTransform>().anchoredPosition = randomPos;
            StartCoroutine(FadeText(dmgNumber, 0.5f, randomPos.normalized * 100));
        }
        aggro = true;
        healthBar.fillAmount = health/(maxHealth*1.0f);
        if (health <= 0)
        {
            GameManager.Instance.UpdateEnemyNum(-1);
            Destroy(gameObject);
            //play any death anims, give player any rewards for kill
        }
    }


    private IEnumerator FadeText(GameObject txt, float duration, Vector2 dir)
    {
        Vector2 origScale = txt.transform.localScale;
        float elapsed = 0;
        while (elapsed < 0.3f)
        {
            txt.GetComponent<RectTransform>().anchoredPosition += Time.deltaTime * dir;
            txt.GetComponent<CanvasGroup>().alpha = elapsed/0.3f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0;
        while (elapsed < duration)
        {
            txt.GetComponent<RectTransform>().anchoredPosition += Time.deltaTime * dir;
            txt.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(1, 0.5f, elapsed/duration);
            txt.transform.localScale = origScale * Mathf.Lerp(1, 0.75f, elapsed/duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(txt);
    }
}
