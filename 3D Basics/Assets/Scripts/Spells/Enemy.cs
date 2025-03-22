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
    [SerializeField] private int aggroRange;
    private bool aggro;
    public float stunTimer;

    [Header("Attack")]
    [SerializeField] private float atkDelay;
    private float atkTimer;
    [SerializeField] private int dmg;
    [SerializeField] private int atkRange;

    [Header("References")]
    private Rigidbody rb;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject player;


    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
            //play any death anims, give player any rewards for kill
        }

        float dist = Vector3.Distance(player.transform.position, transform.position);
        if (dist < aggroRange)
            aggro = true;

        stunTimer -= Time.deltaTime;
        if (aggro && stunTimer <= 0)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.Scale(player.transform.position - transform.position, new Vector3(1, 0, 1)));
            rb.MovePosition(rb.position + transform.forward * speed * Time.deltaTime);
        }

        anim.SetBool("Stunned", stunTimer > 0);
    }


    public void TakeDamage(int dmg)
    {
        if (dmg > 0)
        {
            anim.Play("MeleeHit");
            health -= dmg;
        }
        aggro = true;
    }
}
