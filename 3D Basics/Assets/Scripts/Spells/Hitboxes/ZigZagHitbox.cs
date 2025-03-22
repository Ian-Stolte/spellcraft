using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZigZagHitbox : Hitbox
{
    void Start()
    {
        foreach (Transform child in transform)
        {
            StartCoroutine(Fade(child, 0.5f));
        }
        CheckCollisions();
    }

    private IEnumerator Fade(Transform obj, float duration)
    {
        Material mat = obj.GetComponent<MeshRenderer>().material;
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
        Destroy(obj.parent.gameObject);
    }

    public override void CheckCollisions()
    {
        List<Collider> cols = new List<Collider>();
        foreach (Transform child in transform)
        {
            Bounds b = child.GetComponent<BoxCollider>().bounds;
            Collider[] newCols = Physics.OverlapBox(b.center, b.extents, Quaternion.identity, LayerMask.GetMask("Enemy"));
            foreach (Collider c in newCols)
                if (!cols.Contains(c))
                    cols.Add(c);
        }
        if (cols.Count > 0)
            GameObject.Find("Player").GetComponent<PlayerSpells>().SpellEffects(cols.ToArray(), spell);
    }
}
