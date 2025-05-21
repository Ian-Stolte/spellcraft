using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AggroEnemy : Enemy
{   
    [Header("Values")]
    [SerializeField] private float defSpeed;

    [Header("Attack")]
    [SerializeField] private float atkRange;
    [SerializeField] private float atkDelay;
    private float atkTimer;
    [SerializeField] private float atkDuration;
    [SerializeField] private float dashSpeed;
    [SerializeField] private int dmg;
    
    [SerializeField] private GameObject atkPrefab;
    private Transform atkWarning;

    private bool attacking;
    private bool canHitPlayer;
    private bool hitboxOn;

    private IEnumerator atkCor;


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
            if (!attacking)
            {
                atkTimer = Mathf.Max(0, atkTimer - Time.deltaTime);
                Vector3 dir = Vector3.Scale(player.transform.position - transform.position, new Vector3(1, 0, 1)).normalized;
                transform.rotation = Quaternion.LookRotation(dir);
            }

            float speed = (slowTimer > 0) ? defSpeed*0.3f : defSpeed;
            
            if (dist > atkRange && !attacking)
            {
                rb.MovePosition(rb.position + transform.forward * speed * Time.deltaTime);
            }
            if (atkTimer <= 0 && dist < atkRange && !player.GetComponent<PlayerPrograms>().dashing)
            {
                anim.Play("Attack");
                atkTimer = atkDelay;
                atkCor = Attack();
                StartCoroutine(atkCor);
            }
        }
        else if (atkCor != null)
        {
            StopCoroutine(atkCor);
            if (atkWarning != null)
                Destroy(atkWarning.parent.gameObject);
            atkCor = null;
            attacking = false;
            canHitPlayer = false;
            hitboxOn = false;
        }
 
        if (canHitPlayer && hitboxOn)
        {
            if (Physics.OverlapSphere(transform.position, 1.6f, LayerMask.GetMask("Player")).Length > 0)
            {
                player.GetComponent<PlayerMovement>().TakeDamage(dmg);
                canHitPlayer = false;
            }
        }
    }

    private IEnumerator Attack()
    {
        attacking = true;
        Vector3 target = player.transform.position + player.GetComponent<PlayerMovement>().moveDir*2 + (player.transform.position - transform.position).normalized * 0.5f;
        atkWarning = Instantiate(atkPrefab, new Vector3(target.x, 0, target.z), transform.rotation).transform.GetChild(0);
        target += (player.transform.position - transform.position).normalized*5;
        float elapsed = 0;
        float dist = Vector3.Distance(target, transform.position);
        float dashTime = atkDuration - dist/dashSpeed;
        while (elapsed < atkDuration)
        {
            atkWarning.localScale = new Vector3(1, 1, 1) * elapsed/atkDuration;
            elapsed += Time.deltaTime;
            yield return null;

            if (elapsed >= dashTime)
            {
                rb.velocity = (atkWarning.transform.position - transform.position).normalized * dashSpeed;
                if (!hitboxOn)
                {
                    canHitPlayer = true;
                    hitboxOn = true;
                }
            }
        }
        rb.velocity = Vector3.zero;
        canHitPlayer = false;
        hitboxOn = false;
        
        /*Bounds b = atkWarning.parent.GetComponent<BoxCollider>().bounds;
        if (Physics.OverlapBox(b.center, b.extents, Quaternion.identity, LayerMask.GetMask("Player")).Length > 0)
        {
            player.GetComponent<PlayerMovement>().TakeDamage(dmg);
            //knockback from center of attack/stun?
        }*/
        Destroy(atkWarning.parent.gameObject);
        yield return new WaitForSeconds(0.2f);
        attacking = false;
    }

    private void OnDestroy()
    {
        if (atkWarning != null)
            Destroy(atkWarning.parent.gameObject);
    }
}
