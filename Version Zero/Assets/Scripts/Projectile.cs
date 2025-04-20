using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{   
    public Vector3 dir;
    public float speed;
    public int dmg;

    public float despawnDist;
    private float distance;

    private Transform player;


    private void Start()
    {
        player = GameObject.Find("Player").transform;
    }

    private void Update()
    {
        transform.position += Time.deltaTime * dir * speed;
        distance += Time.deltaTime * dir.magnitude * speed;
        if (distance > despawnDist)
            Destroy(gameObject);
        
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (Collider c in hits)
        {
            if (c.CompareTag("Player") && !player.GetComponent<PlayerSpells>().dashing)
            {
                player.GetComponent<PlayerMovement>().TakeDamage(dmg);
                Destroy(gameObject);
            }
        }
    }
}
