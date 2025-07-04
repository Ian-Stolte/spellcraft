using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleHitbox : Hitbox
{
    void Start()
    {
        StartCoroutine(Fade(0.5f));
        AudioManager.Instance.Play("Impact");
        CheckCollisions();
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
        Collider[] cols = Physics.OverlapSphere(transform.position, transform.localScale.x/2, LayerMask.GetMask("Enemy"));
        if (cols.Length > 0)
            GameObject.Find("Player").GetComponent<PlayerPrograms>().SpellEffects(cols, spell, transform.position);
    }
}
