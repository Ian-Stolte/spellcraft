using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnemyBehavior : MonoBehaviour
{
    public string state = "IDLE";

    [Header("Values")]
    [SerializeField] private int maxHealth;
    public int health;
    [SerializeField] private float speed;

    [Header("States")]
    private Vector3 startingPos;
    [SerializeField] private int alertRange;
    [SerializeField] private int aggroRange;
    [SerializeField] private int meleeRange;
    private Vector3 lastPlayerPos;
    bool wasHostile;
    [SerializeField] private float idleTimer;
    [SerializeField] private float retreatThreshold;
    [SerializeField] private float fleeThreshold;
    private bool knockback;
    private bool lineOfSight;

    [Header("Attacks")]
    [SerializeField] private float attackDelay;
    private float attackTimer;
    [SerializeField] private int dmg;

    [Header("References")]
    private Rigidbody rb;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject player;


    private void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody>();
        startingPos = transform.position;
    }

    private void Update()
    {
        Vector3 dir = (player.transform.position - transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(Vector3.Scale(dir, new Vector3(1, 0, 1)));
        float playerDist = Vector3.Distance(player.transform.position, transform.position);
        float startDist = Vector3.Distance(startingPos, transform.position);
        lineOfSight = !Physics.Raycast(transform.position, dir, playerDist, LayerMask.GetMask("Ground")) && (Vector3.Angle(dir, transform.forward) < 80 || playerDist < 5);

        //change states
        if (playerDist < alertRange && (state == "IDLE" || state == "ALERT") && lineOfSight)
        {
            if (wasHostile)
            {
                state = "HOSTILE";
                attackTimer = attackDelay;
                anim.SetInteger("State", 2);
            }
            else
            {
                state = "ALERT";
                anim.SetInteger("State", 1);
            }
        }
        else if (idleTimer <= 0)
        {
            if (state == "HOSTILE")
                wasHostile = true;
            state = "IDLE";
            anim.SetInteger("State", 0);
            idleTimer = 999;
        }

        //do stuff
        if (!knockback)
        {
            if (state == "IDLE")
            {
                //patrol route/randomly wander around within a radius
            }
            else if (state == "ALERT")
            {
                if (lineOfSight)
                {
                    transform.rotation = rot * Quaternion.Euler(0, -5, 0);
                    idleTimer = 2;
                }
                else
                {
                    idleTimer -= Time.deltaTime;
                }
            }
            else if (state == "HOSTILE")
            {
                attackTimer = Mathf.Max(0, attackTimer - Time.deltaTime);
                if (lineOfSight)
                {
                    lastPlayerPos = player.transform.position;
                    idleTimer = 1;
                    if (attackTimer == 0)
                    {
                        anim.Play("RangedAttack");
                        StartCoroutine(KnockBack(Quaternion.Euler(0, -180, 0) * transform.forward * 3, 0.3f, false));
                        player.GetComponent<PlayerCombat>().TakeDamage(dmg); //TODO: scale by distance?
                        attackTimer = attackDelay;
                    }
                }
                //look at player's last pos
                Quaternion lastRot = Quaternion.LookRotation(Vector3.Scale(lastPlayerPos - transform.position, new Vector3(1, 0, 1)));
                transform.rotation = lastRot * Quaternion.Euler(0, -5, 0);
                
                if ((health/(1f*maxHealth) < retreatThreshold)) //retreat to a safe dist if hurt
                {
                    if (playerDist < meleeRange*2)
                        rb.MovePosition(rb.position + transform.forward * speed*(-1) * Time.deltaTime);
                    else if (playerDist > meleeRange*2 + 1)
                        rb.MovePosition(rb.position + transform.forward * speed * Time.deltaTime);
                }
                else if (Vector3.Distance(transform.position, lastPlayerPos) < 0.5f && !lineOfSight) //lose aggro if no LOS
                    idleTimer -= Time.deltaTime;
                else if (!(lineOfSight && attackTimer < 0.5f) && !(lineOfSight && Vector3.Distance(player.transform.position, transform.position) < meleeRange))
                    rb.MovePosition(rb.position + transform.forward * speed * Time.deltaTime); //move toward the player if healthy
            }
            else if (state == "FLEE")
            {
                transform.rotation = rot * Quaternion.Euler(0, 175, 0);
                rb.MovePosition(rb.position + transform.forward * speed*2 * Time.deltaTime);
            }
        }

        //Die from falling off world
        if (transform.position.y < -8)
        {
            Destroy(gameObject);
        }
    }


    public void TakeDamage(int dmg)
    {
        if (dmg > 0)
        {
            anim.Play("MeleeHit");

            //warn other enemies
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject e in enemies)
            {
                EnemyBehavior script = e.GetComponent<EnemyBehavior>();
                Vector3 dir = e.transform.position - transform.position;
                float dist = Vector3.Distance(e.transform.position, transform.position);
                if (dist < script.aggroRange && !Physics.Raycast(transform.position, dir, dist, LayerMask.GetMask("Ground")))
                {
                    script.TakeDamage(0);
                }
            }
        }
        if (health/(maxHealth*1f) < fleeThreshold)
        {
            state = "FLEE";
            anim.SetInteger("State", 0);
        }
        else if (Vector3.Distance(player.transform.position, transform.position) < aggroRange && lineOfSight)
        {
            if (state != "HOSTILE")
            {
                state = "HOSTILE";
                attackTimer = attackDelay;
                idleTimer = 1;
                Quaternion rot = Quaternion.LookRotation(Vector3.Scale(lastPlayerPos - transform.position, new Vector3(1, 0, 1)));
                transform.rotation = rot * Quaternion.Euler(0, -5, 0);
                anim.SetInteger("State", 2);
            }
        }
        else
        {
            state = "ALERT";
            wasHostile = true;
            anim.SetInteger("State", 1);
        }

        health -= dmg;
        if (health <= 0)
        {
            //play death anim
            Destroy(gameObject);
        }
    }

    public void KnockBackHelper(Vector3 force, float duration, bool smooth)
    {
        StartCoroutine(KnockBack(force, duration, smooth));
    }

    public IEnumerator KnockBack(Vector3 force, float duration, bool smooth)
    {
        knockback = true;
        if (smooth)
        {
            float timer = 0f;
            while (timer < duration)
            {
                float fadeFactor = 1 - (timer / duration); // Linearly decreases force
                if (rb != null)
                    rb.velocity = force * fadeFactor;
                timer += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            rb.velocity = force;
            yield return new WaitForSeconds(duration + 0.2f);
        }
        knockback = false;
    }


    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Handles.color = Color.blue;
        Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360, alertRange);
        Handles.color = Color.red;
        Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360, aggroRange);
    }
    #endif
}
