using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("References")]
    private Rigidbody rb;
    [SerializeField] private Animator anim;
    private GameObject player;


    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
        atkTimer = atkDelay;
    }

    void Update()
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
        }
        aggro = true;
        //show damage number

        if (health <= 0)
        {
            Destroy(gameObject);
            //play any death anims, give player any rewards for kill
        }
    }
}
