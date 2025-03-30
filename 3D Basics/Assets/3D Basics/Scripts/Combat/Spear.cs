using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
    public int dmg;
    private BoxCollider box;

    void Start()
    {
        box = GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (GetComponent<Rigidbody>().velocity.magnitude < 1)
        {
            gameObject.SetActive(false);
        }

        Collider[] collisions = Physics.OverlapBox(box.bounds.center, box.bounds.extents/2, transform.rotation, LayerMask.GetMask("Enemy"));
        foreach (Collider collider in collisions)
        {
            EnemyBehavior script = collider.GetComponent<EnemyBehavior>();
            if (script != null)
            {
                if (script.health > dmg)
                {
                    Vector3 dir = GetComponent<Rigidbody>().velocity.normalized + new Vector3(0, 0.1f, 0);
                    script.KnockBackHelper(dir*dmg*1.5f, (1f/6f)*dmg, false);
                }
                script.TakeDamage(dmg);
                gameObject.SetActive(false);
            }
        }
    }
}
