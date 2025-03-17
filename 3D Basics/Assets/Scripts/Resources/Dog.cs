using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float rotSpeed;
    [SerializeField] private float moveDist;
    private Vector3 randomPos;

    [SerializeField] private float waitMin;
    [SerializeField] private float waitMax;
    private float waitTimer;

    [Header("Despawn")]
    [SerializeField] private float despawnChance;
    [SerializeField] private float despawnDist;
    [SerializeField] private float minLifetime;
    private float lifetime;

    private Transform player;
    private Rigidbody rb;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player").transform;
        randomPos = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        randomPos *= Random.Range(3f, moveDist);
        randomPos += new Vector3(0, transform.position.y + 1, 0);
    }


    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime > minLifetime)
        {
            float playerDist = Vector3.Distance(transform.position, player.position);
            if (playerDist > despawnDist && Random.Range(0f, 1f) < despawnChance)
            {
                Vector3 dir = (player.position - transform.position).normalized;
                bool lineOfSight = !Physics.Raycast(transform.position, dir, playerDist, LayerMask.GetMask("Ground")) && Vector3.Angle(-1*dir, GameObject.Find("Main Camera").transform.forward) < 80; 
                if (!lineOfSight)
                Destroy(gameObject);
            }
        }
    }


    void FixedUpdate()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
        }
        else
        {
            Vector3 moveDir = (randomPos-transform.position).normalized;
            rb.MovePosition(rb.position + moveDir * speed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveDir.x, 0, moveDir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, targetRotation.eulerAngles.y-90, 0), rotSpeed*Time.deltaTime);
            
            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(randomPos.x, 0, randomPos.z)) < 0.5f)
            {
                waitTimer = Random.Range(waitMin, waitMax);
                randomPos = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                randomPos *= Random.Range(3f, moveDist);
                randomPos += new Vector3(0, transform.position.y + 1, 0);
            }
        }
    }
}
