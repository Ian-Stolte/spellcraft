using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BasicEnemy : Enemy
{   
    [Header("Values")]
    [SerializeField] private float defSpeed;

    [Header("Attack")]
    [SerializeField] private float atkRange;
    [SerializeField] private float atkDelay;
    private float atkTimer;
    [SerializeField] private float meleeRange;
    [SerializeField] private int dmg;
    [SerializeField] private GameObject projPrefab;
    [SerializeField] private float projSpeed;

    [Header("States")]
    [SerializeField] private float retreatRange;
    [SerializeField] private float retreatThreshold;

    void Start()
    {
        atkTimer = atkDelay;
        base.Start();
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
            //look at player
            Vector3 dir = Vector3.Scale(player.transform.position - transform.position, new Vector3(1, 0, 1)).normalized;
            transform.rotation = Quaternion.LookRotation(dir);
            
            float speed = (slowTimer > 0) ? defSpeed*0.3f : defSpeed;
            
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
                    proj.GetComponent<Projectile>().speed = projSpeed;
                    proj.GetComponent<Projectile>().despawnDist = atkRange + 2f;
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
                if (atkTimer <= 0 && dist < meleeRange && !player.GetComponent<PlayerPrograms>().dashing)
                {
                    player.GetComponent<PlayerMovement>().TakeDamage(dmg);
                    anim.Play("Attack");
                    rb.AddForce(dir * -400, ForceMode.Impulse);
                    atkTimer = atkDelay;
                }
            }
        }
    }
}
