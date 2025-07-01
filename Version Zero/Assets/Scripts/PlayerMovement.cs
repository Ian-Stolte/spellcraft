using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [HideInInspector] public Vector3 moveDir;
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
    public bool grounded;

    [Header("Health")]
    public int health;
    [SerializeField] private int maxHealth;
    public Transform hpBar;
    [SerializeField] private float maxBurstDmg;
    private float immunityTimer;
    private int currentBurst;
    public bool canDie;

    [Header("Computer")]
    public Transform computer;
    [SerializeField] private float maxCompDist;
    [SerializeField] private float compDist;
    [SerializeField] private float dampTime;
    private Vector3 dampVel = Vector3.zero;
    private Vector3 diff;
    private List<Vector3> lastPos = new List<Vector3>();
    [SerializeField] private int maxPosData;
    //y-movement
    [SerializeField] private float compYFreq;
    [SerializeField] private float compYAmp;
    private float compPhase;

    [Header("Shield")]
    [SerializeField] private GameObject shield;
    [HideInInspector] public float shieldTimer;

    [Header("Misc")]
    [SerializeField] private Animator anim;
    [SerializeField] private Animator damageFlash;
    //Game Over
    private bool endingGame;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        health = maxHealth;
        for (int i = 0; i < 30; i++)
            lastPos.Add(transform.position + new Vector3(-2, 0, 0));
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        computer.position = transform.position + new Vector3(-2, 1, 0);
        lastPos.Clear();
        for (int i = 0; i < 30; i++)
            lastPos.Add(transform.position + new Vector3(-2, 0, 0));
    }


    void Update()
    {
        //Anim Values
        /*Bounds b = groundCheck.GetComponent<BoxCollider>().bounds;
        //grounded = ((Physics.OverlapBox(b.center, b.extents*2, Quaternion.identity, groundLayer) != null) && jumpDelay == 0);
        grounded = (Physics.CheckSphere(groundCheck.position, 0.1f, groundLayer) && jumpDelay == 0);
        anim.SetBool("airborne", !grounded);
        anim.SetBool("jumpDelay", jumpDelay > 0);
        anim.SetFloat("yVel", rb.velocity.y);

        //Jump
        jumpDelay = Mathf.Max(0, jumpDelay-Time.deltaTime);
        jumpInputDelay = Mathf.Max(0, jumpInputDelay-Time.deltaTime);
        if (GetComponent<PlayerInteract>() != null)
        {
            if (Input.GetKey(KeyCode.Space) && !GetComponent<PlayerInteract>().planting)
                jumpInputDelay = 0.3f;
        }
        else if (Input.GetKey(KeyCode.Space))
            jumpInputDelay = 0.3f;
        if (grounded && jumpInputDelay > 0 && jumpDelay == 0 && !GameManager.Instance.pauseGame && !GameManager.Instance.playerPaused)
        {
            jumpDelay = 0.3f;
            StartCoroutine(JumpAnim());
        }*/

        //Glitch based on HP
        if (!Camera.main.GetComponent<GlitchManager>().showingGlitch)
            Camera.main.GetComponent<Glitch>().glitch = Mathf.Lerp(0, 0.3f, Mathf.Pow((maxHealth-health)/(1f*maxHealth), 3));


        immunityTimer = Mathf.Max(0, immunityTimer-Time.deltaTime);
        shieldTimer = Mathf.Max(0, shieldTimer-Time.deltaTime);
        shield.SetActive(shieldTimer > 0);
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
        moveDir = (lateral*camRight + forward*camForward).normalized;
        if (moveDir != Vector3.zero && !GameManager.Instance.pauseGame && !GameManager.Instance.playerPaused && !GetComponent<PlayerPrograms>().dashing)
        {
            float spd = speed;
            float rotSpd = rotationSpeed;
            rb.MovePosition(rb.position + moveDir * spd * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), rotSpd * Time.deltaTime);
        }


        //COMPUTER FOLLOW
        //get average direction of movement
        Vector3 total = Vector3.zero;
        foreach (Vector3 pos in lastPos)
            total += pos;
        diff = transform.position - total/lastPos.Count;

        float distPct = Vector3.Distance(computer.position, transform.position)/maxCompDist - 0.5f;
        //x-z position
        float adjustedDampTime = Mathf.Lerp(dampTime*100, dampTime, distPct);
        computer.position = Vector3.SmoothDamp(computer.position, transform.position - diff*compDist, ref dampVel, dampTime);        
        //y position
        float freq = Mathf.Lerp(compYFreq*0.5f, compYFreq, distPct);
        float amp = Mathf.Lerp(compYAmp, compYAmp*2, distPct);
        compPhase += freq * Time.deltaTime * 2f * Mathf.PI;
        computer.position += new Vector3(0, Mathf.Sin(compPhase) * amp, 0);

        //update lastPos array
        Vector3 dist = lastPos[lastPos.Count-Mathf.Min(5, lastPos.Count)] - transform.position;
        dist = new Vector3(dist.x, 0, dist.z);
        if (dist.magnitude > 0.01f)
        {
            lastPos.Add(transform.position);
            if (lastPos.Count > maxPosData)
                lastPos.Remove(lastPos[0]);
        }
    }


    private IEnumerator JumpAnim()
    {
        anim.Play("Jump");
        yield return new WaitForSeconds(0.1f);
        rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
    }

    public void TakeDamage(int dmg)
    {
        if (immunityTimer == 0 && shieldTimer == 0)
        {
            //keep track of damage taken in last 0.5s & give immunity if past burst threshold
            currentBurst += dmg;
            StartCoroutine(UndoBurst(dmg));
            if (currentBurst > maxBurstDmg)
            {
                immunityTimer = 0.5f;
            }

            //cancel terminal progress
            StopCoroutine(GameManager.Instance.UseTerminal());
            AudioManager.Instance.Stop("Terminal Charge");
            if (GameManager.Instance.bar != null)
                Destroy(GameManager.Instance.bar.transform.parent.gameObject);
            GameManager.Instance.playerPaused = false;

            //take damage
            health = Mathf.Max(0, health-dmg);
            if (health <= 0)
            {
                if (canDie)
                {
                    if (!GameManager.Instance.pauseGame)
                        StartCoroutine(GameManager.Instance.GameOver());
                }
                else
                    health = maxHealth;
            }
            else
            {
                AudioManager.Instance.Play("Take Damage");
                damageFlash.Play("DamageFlash");
                Camera.main.GetComponent<GlitchManager>().ShowGlitch(0.5f, 0.5f);
            }
            
            //set HP bar fill
            if (hpBar != null)
            {
                hpBar.GetChild(0).GetChild(0).GetComponent<Image>().fillAmount = 0.05f + 0.95f * health/(maxHealth * 1.0f);
                RectTransform rightTri = hpBar.GetChild(0).GetChild(1).GetComponent<RectTransform>();
                rightTri.anchoredPosition = new Vector2(Mathf.Lerp(-137, 120, health/(maxHealth * 1.0f)), rightTri.anchoredPosition.y);
                hpBar.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = health + "/" + maxHealth;
            }
        }
    }

    private IEnumerator UndoBurst(int dmg)
    {
        yield return new WaitForSeconds(0.5f);
        currentBurst -= dmg;
    }
}