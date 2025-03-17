using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    private Rigidbody rb;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump")]
    [SerializeField] private float jumpPower;
    private float jumpDelay;
    private float jumpInputDelay;
    [SerializeField] private float upGravity;
    [SerializeField] private float downGravity;
    [SerializeField] private float hangGravity;
    [SerializeField] private float hangPoint;
    private bool grounded;

    [Header("Health")]
    [SerializeField] private int health;
    [SerializeField] private int maxHealth;

    [Header("Attacks")]
    private float attackDelay;
    private float attackInputDelay;
    [SerializeField] private Transform spear;
    [SerializeField] private Transform thrownSpear;

    [SerializeField] private BoxCollider meleeHB;
    [SerializeField] private int meleeDmg;
    [SerializeField] private int throwDmg;
    [SerializeField] private float meleeKnockback;

    [SerializeField] private float throwSpeed;
    private bool throwing;
    private float throwCharge;

    [Header("Camera Zoom")]
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private FreeLookAxisDriver camScript;
    [SerializeField] private Transform cam;

    private CinemachineFreeLook.Orbit[] originalOrbits;
    [SerializeField] private float zoomRadius;
    [SerializeField] private float zoomHeight;
    private IEnumerator cameraCor;

    [SerializeField] private Animator anim;
    [SerializeField] private Animator damageFlash;

    //private float pastVel = 0;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        originalOrbits = new CinemachineFreeLook.Orbit[3];
        for (int i = 0; i < 3; i++)
        {
            originalOrbits[i] = freeLookCamera.m_Orbits[i];
        }
        health = maxHealth;
    }


    void Update()
    {
        //Anim Values
        grounded = (Physics.CheckSphere(groundCheck.position, 0.1f, groundLayer) && jumpDelay == 0);
        anim.SetBool("airborne", !grounded);
        anim.SetBool("jumpDelay", jumpDelay > 0);
        anim.SetFloat("yVel", rb.velocity.y);

        //Jump
        jumpDelay = Mathf.Max(0, jumpDelay-Time.deltaTime);
        jumpInputDelay = Mathf.Max(0, jumpInputDelay-Time.deltaTime);
        if (Input.GetKey(KeyCode.Space))
            jumpInputDelay = 0.3f;
        if (grounded && jumpInputDelay > 0 && jumpDelay == 0 && attackDelay < 0.2f && !throwing)
        {
            jumpDelay = 0.3f;
            StartCoroutine(JumpAnim());
        }
        
        //Melee attack
        attackDelay = Mathf.Max(0, attackDelay-Time.deltaTime);
        attackInputDelay = Mathf.Max(0, attackInputDelay-Time.deltaTime);
        if (Input.GetMouseButtonDown(0))
            attackInputDelay = 0.3f;
        if (grounded && attackInputDelay > 0 && attackDelay < 0.2f)
        {
            attackDelay = 0.8f;
            StartCoroutine(MeleeAttack());
        }

        //Ranged attack
        if (grounded && Input.GetMouseButton(1) && attackDelay < 0.2f && !throwing)
        {
            throwCharge = 0;
            throwing = true;
            StartCoroutine(RangedAttack());
        }
        if (Input.GetMouseButtonUp(1) && throwing)
        {
            if (throwCharge < 0.4f)
            {
                //stop ranged attack
                anim.Play("Idle");
                if (cameraCor != null)
                    StopCoroutine(cameraCor);
                cameraCor = CameraZoom(false, 0.5f);
                StartCoroutine(cameraCor);
                throwing = false;
                attackDelay = 0.4f;
            }
            else
            {
                //throw spear
                thrownSpear.position = spear.position;
                thrownSpear.rotation = spear.rotation;
                thrownSpear.gameObject.SetActive(true);
                spear.gameObject.SetActive(false);
                Vector3 throwDir = Quaternion.Euler(0, -10, 0) * Quaternion.AngleAxis(-5f, transform.right) * transform.forward;
                float speed = throwSpeed;
                thrownSpear.GetComponent<Spear>().dmg = throwDmg;
                if (throwCharge < 0.6f)
                {
                    speed *= 0.5f;
                    thrownSpear.GetComponent<Spear>().dmg = (int)(throwDmg * 0.5f);
                }
                if (throwCharge < 1.2f)
                {
                    speed *= 0.75f;
                    thrownSpear.GetComponent<Spear>().dmg = (int)(throwDmg * 0.75);
                }
                thrownSpear.GetComponent<Rigidbody>().velocity = throwDir * speed;
                anim.Play("Attack_RangedThrow");
                if (cameraCor != null)
                    StopCoroutine(cameraCor);
                cameraCor = CameraZoom(false, 0.75f);
                StartCoroutine(cameraCor);
                throwing = false;
                attackDelay = 0.4f;
            }
        }
        if (throwing)
        {
            throwCharge += Time.deltaTime;
            Vector3 camForward = new Vector3(cam.forward.x, 0, cam.forward.z);
            Quaternion targetRot = Quaternion.Euler(0, 10, 0) * Quaternion.LookRotation(camForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
        camScript.xAxis.multiplier = throwing ? 3 : 5;
    }


    void FixedUpdate()
    {
        //Apply gravity
        if (Mathf.Abs(rb.velocity.y) < hangPoint)
            rb.AddForce(-9.8f * hangGravity * Vector3.up, ForceMode.Acceleration);
        else if (rb.velocity.y < 0)
            rb.AddForce(-9.8f * downGravity * Vector3.up, ForceMode.Acceleration);
        else
            rb.AddForce(-9.8f * upGravity * Vector3.up, ForceMode.Acceleration);

        //Movement
        if (attackDelay == 0) //TODO: allow move-cancelling recovery?
        {
            int lateral = 0;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                lateral ++;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                lateral --;
            int forward = 0;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                forward ++;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                forward --;

            Vector3 camForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;
            Vector3 moveDir = (lateral*camRight + forward*camForward).normalized;
            if (moveDir != Vector3.zero)
            {
                float spd = throwing ? speed/2f : speed;
                float rotSpd = throwing ? 0 : rotationSpeed;
                rb.MovePosition(rb.position + moveDir * spd * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), rotSpd * Time.deltaTime);
            }
        }
    }


    private IEnumerator JumpAnim()
    {
        anim.Play("Jump");
        yield return new WaitForSeconds(0.1f);
        rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
    }

    
    private IEnumerator MeleeAttack()
    {
        TurnTowardTarget();
        throwing = false;

        spear.gameObject.SetActive(true);
        anim.Play("Attack_Melee", 0, 0f);
        for (float i = 0; i < 0.1f; i += 0.01f) //move -0.2f in 0.1s
        {
            rb.MovePosition(rb.position + transform.forward * (-0.02f));
            yield return new WaitForSeconds(0.01f);
        }
        for (float i = 0; i < 0.1f; i += 0.01f) //move 0.4f in 0.1s
        {
            rb.MovePosition(rb.position + transform.forward * (0.04f));
            yield return new WaitForSeconds(0.01f);
        }
        for (float i = 0; i < 0.05f; i += 0.01f) //move 0.4f in 0.05s
        {
            rb.MovePosition(rb.position + transform.forward * (0.08f));
            yield return new WaitForSeconds(0.01f);
        }
        
        //hitbox
        Bounds b = meleeHB.bounds;
        Collider[] hitEnemies = Physics.OverlapBox(b.center, b.extents, meleeHB.transform.rotation, LayerMask.GetMask("Enemy"));

        foreach (Collider c in hitEnemies)
        {
            EnemyBehavior enemyScript = c.GetComponent<EnemyBehavior>();
            if (enemyScript != null)
            {
                Vector3 dir = (c.transform.position - transform.position).normalized + new Vector3(0, 0.1f, 0);
                StartCoroutine(enemyScript.KnockBack(dir*meleeKnockback, 0.3f, true));
                enemyScript.TakeDamage(meleeDmg);
            }
        }

        for (float i = 0; i < 0.05f; i += 0.01f) //move 0.4f in 0.05s
        {
            rb.MovePosition(rb.position + transform.forward * (0.08f));
            yield return new WaitForSeconds(0.01f);
        }
    }

    private IEnumerator RangedAttack()
    {
        if (cameraCor != null)
            StopCoroutine(cameraCor);
        cameraCor = CameraZoom(true, 1.5f);
        StartCoroutine(cameraCor);
        spear.gameObject.SetActive(true);
        anim.Play("Attack_RangedStart", 0, 0f);
        yield return null;
    }


    private void TurnTowardTarget()
    {
        Collider[] sphere = Physics.OverlapSphere(transform.position, 4, LayerMask.GetMask("Enemy"));
        
        Transform closestEnemy = null;
        float closestDistance = 99;
        foreach (Collider enemy in sphere)
        {
            Vector3 dir = (enemy.transform.position - transform.position).normalized;
            float dist = Vector3.Distance(enemy.transform.position, transform.position);
            if (Vector3.Angle(transform.forward, dir) < 60 && dist < closestDistance)
            {
                closestEnemy = enemy.transform;
                closestDistance = dist;
            }
        }

        if (closestEnemy != null)
        {
            Quaternion rot = Quaternion.LookRotation(Vector3.Scale(closestEnemy.position - transform.position, new Vector3(1, 0, 1)));
            transform.rotation = rot * Quaternion.Euler(0, -5, 0);
        }
    }


    private IEnumerator CameraZoom(bool zoomIn, float duration)
    {
        CinemachineFreeLook.Orbit[] currentOrbits = new CinemachineFreeLook.Orbit[3];
        for (int i = 0; i < 3; i++)
        {
            currentOrbits[i] = freeLookCamera.m_Orbits[i];
        }
        float t = 0;
        while (t < duration)
        {
            if (zoomIn)
            {
                for (int i = 0; i < 3; i++)
                {
                    freeLookCamera.m_Orbits[i].m_Radius = Mathf.Lerp(currentOrbits[i].m_Radius, zoomRadius, t/duration);
                    freeLookCamera.m_Orbits[i].m_Height = Mathf.Lerp(currentOrbits[i].m_Height, zoomHeight, t/duration);
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    freeLookCamera.m_Orbits[i].m_Radius = Mathf.Lerp(currentOrbits[i].m_Radius, originalOrbits[i].m_Radius, t/duration);
                    freeLookCamera.m_Orbits[i].m_Height = Mathf.Lerp(currentOrbits[i].m_Height, originalOrbits[i].m_Height, t/duration);
                }
            }
            yield return null;
            t += Time.deltaTime;
        }
    }


    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            Debug.Log("GAME OVER!!");
            health = maxHealth;
        }
        damageFlash.Play("DamageFlash");
    }
}