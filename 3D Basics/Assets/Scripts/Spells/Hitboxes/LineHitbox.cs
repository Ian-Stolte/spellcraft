using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineHitbox : Hitbox
{
    public Vector3 dir;
    public float speed;

    void Update()
    {
        transform.position += Time.deltaTime * dir * speed;
        CheckCollisions();
    }

    public override void CheckCollisions()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("Enemy"));
        if (hits.Length > 0)
        {
            GameObject.Find("Player").GetComponent<PlayerSpells>().SpellEffects(new Collider[]{hits[0]}, spell);
            Destroy(gameObject);
        }
    }
}
