using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("Values")]
    public int maxHealth;
    public int health;

    [Header("States")]
    public int aggroRange;
    [HideInInspector] public bool aggro;
    [HideInInspector] public float stunTimer;
    [HideInInspector] public float slowTimer;

    [Header("Canvas")]
    public Image healthBar;
    [SerializeField] private TextMeshProUGUI statusTxt;
    [SerializeField] private GameObject damageNumber;

    [Header("References")]
    public Animator anim;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public GameObject player;

    [Header("Mark")]
    [SerializeField] private GameObject mark;
    private int markDmg;
    private float markTimer;

    [Header("Misc")]
    [HideInInspector] public bool shielded;
    [HideInInspector] public IEnumerator auraBurn;


    public void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
    }

    public void Update()
    {
        if (transform.position.y < 2)
            Destroy(GetComponent<TrailRenderer>());
        if (!GameManager.Instance.pauseGame)
        {
            stunTimer -= Time.deltaTime;
            slowTimer -= Time.deltaTime;   
            anim.SetBool("Stunned", stunTimer > 0);
            if (stunTimer > 0)
                statusTxt.text = "stunned_";
            else if (slowTimer > 0)
                statusTxt.text = "slowed_";
            else
                statusTxt.text = "";

            transform.GetChild(0).transform.forward = Camera.main.transform.forward;
        }

        markTimer -= Time.deltaTime;
        if (markTimer <= 0)
        {
            mark.SetActive(false);
            markDmg = 0;
        }
    }


    public IEnumerator ApplyBurn(int burn, int ticks)
    {
        for (int i = 0; i < ticks; i++)
        {
            TakeDamage(burn);
            yield return new WaitForSeconds(0.33f);
        }
    }

    public void MarkDamage(int dmg)
    {
        if (markDmg > 0)
        {
            int tempDmg = markDmg;
            markDmg = 0;
            TakeDamage(tempDmg);
        }

        markDmg = dmg;
        mark.SetActive(true);
        markTimer = 2;
    }

    public void TakeDamage(int dmg)
    {
        if (markDmg > 0)
            dmg += markDmg;
        if (mark != null)
            mark.SetActive(false);

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

        if (dmg > 0 && !shielded)
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
            GameManager.Instance.numEnemies -= 1;
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
