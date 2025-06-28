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
    [SerializeField] private int dmg;
    [SerializeField] private GameObject atkPrefab;
    private Transform atkWarning;
    private IEnumerator atkCor;

    [Header("Dash")]
    [SerializeField] private float dashDist;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDelay;
    [SerializeField] private LayerMask terrainLayer;
    private float dashCD;
    private bool dashing;

    [Header("Bools")]
    private bool attacking;
    private bool canHitPlayer;
    private bool hitboxOn;


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
            if (Physics.OverlapSphere(transform.position, 1.1f, LayerMask.GetMask("Player")).Length > 0)
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

            if (Vector3.Distance(atkWarning.position, transform.position) < 0.5f)
            {
                rb.velocity = Vector3.zero;
            }
            else if (elapsed >= dashTime)
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


    
    private IEnumerator Dash(float angle, int sign = 1, float slowFactor = 1f, int numTimes = 1)
    {
        dashing = true;
        dashCD = dashDelay;
     
        float distMod = 0;
        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist > 15 && sign == -1)
            distMod = 2f;
        else if (dist < 3 && Mathf.Abs(angle) <= 30)
            distMod = 2f;
        else if (dist < 5 && Mathf.Abs(angle) <= 30)
            distMod = 1f;
        Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward * sign;
        Vector3 targetPoint = transform.position + direction.normalized * dashDist;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, dashDist, terrainLayer))
        {
            targetPoint = hit.point - direction.normalized * 0.5f;
            //regenerate if would hit a wall immediately
            if (Vector3.Distance(targetPoint, transform.position) < 2f)
            {
                if (Mathf.Abs(angle) >= 70) //if side dash invert direction
                {
                    StartCoroutine(Dash(-angle, sign));
                    yield break;
                }
                else //if dash away or toward, try small side dash
                {
                    int newSign = (Random.Range(0f, 1f) > 0.5f) ? 1 : -1;
                    StartCoroutine(Dash(Random.Range(70, 110) * newSign, 1, 0.5f));
                    yield break;
                }
            }
        }

        //apply slows
        float spd = (slowTimer > 0) ? dashSpeed*0.3f : dashSpeed;
        float totalDist = (dashDist + distMod) * slowFactor;

        transform.GetChild(1).GetComponent<TrailRenderer>().emitting = true;

        float dashTime = totalDist / spd;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        while (elapsed < dashTime && Vector3.Distance(transform.position, targetPoint) > 0.1f)
        {
            float t = elapsed / dashTime;
            rb.velocity = (targetPoint - startPos).normalized * (spd - t * t);

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPoint;
        rb.velocity = Vector3.zero;

        //instinct multi-dash
        if (dashDelay < 1 && Random.Range(0f, 1f) > 0.5f && numTimes <= 3)
        {
            yield return new WaitForSeconds(0.1f);
            if (attacking && dist < atkRange)
            {
                StartCoroutine(Dash(Random.Range(-30f, 30f), -1, 0.5f, numTimes + 1));
            }
            else if (attacking && dist > atkRange)
            {
                StartCoroutine(Dash(Random.Range(-30f, 30f), 1, 0.5f, numTimes + 1));
            }
            else
            {
                int newSign = (Random.Range(0f, 1f) > 0.5f) ? 1 : -1;
                StartCoroutine(Dash(Random.Range(70, 110) * newSign, 1, 0.5f, numTimes + 1));
            }
            yield break;
        }
        
        transform.GetChild(1).GetComponent<TrailRenderer>().emitting = false;
        dashing = false;
    }



    private void OnDestroy()
    {
        if (atkWarning != null)
            Destroy(atkWarning.parent.gameObject);
    }
}
