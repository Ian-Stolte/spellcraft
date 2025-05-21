using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapHitbox : Hitbox
{
    [SerializeField] private float maxLifetime;
    private float lifeTimer;

    [SerializeField] private float activateTime;
    [SerializeField] private Material activatedMat;


    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifetime)
            Destroy(gameObject);

        if (lifeTimer >= activateTime)
            GetComponent<MeshRenderer>().material = activatedMat;
    }

    void OnTriggerEnter(Collider hit)
    {
        if (hit.gameObject.layer == LayerMask.NameToLayer("Enemy") && lifeTimer > activateTime)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            StartCoroutine(Fade(0.5f));
            CheckCollisions();
        }
    }

    private IEnumerator Fade(float duration)
    {
        Material mat = GetComponent<MeshRenderer>().material;
        Color startColor = mat.color;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed/duration);
            mat.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        mat.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        Destroy(gameObject);
    }

    public override void CheckCollisions()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, transform.GetChild(0).localScale.x/2, LayerMask.GetMask("Enemy"));
        if (cols.Length > 0)
            GameObject.Find("Player").GetComponent<PlayerPrograms>().SpellEffects(cols, spell, transform.position);
    }
}
