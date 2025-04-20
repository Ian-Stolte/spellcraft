using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineHitbox : Hitbox
{
    public Vector3 dir;
    public float speed;
    public float despawnDist;
    private float distance;

    void Update()
    {
        transform.position += Time.deltaTime * dir * speed;
        distance += Time.deltaTime * dir.magnitude * speed;
        if (distance > despawnDist)
            Destroy(gameObject);
        CheckCollisions();
    }

    public override void CheckCollisions()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("Enemy"));
        if (hits.Length > 0)
        {
            GameObject.Find("Player").GetComponent<PlayerSpells>().SpellEffects(new Collider[]{hits[0]}, spell, transform.position);
            Destroy(gameObject);
        }
    }
}
