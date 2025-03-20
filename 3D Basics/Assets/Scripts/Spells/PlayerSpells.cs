using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpells : MonoBehaviour
{

    void Update()
    {
        foreach (Spell s in SpellManager.Instance.spells)
        {
            s.cdTimer = Mathf.Max(0, s.cdTimer - Time.deltaTime);
            s.fillTimer.GetComponent<Image>().fillAmount = s.cdTimer/s.cdMax;
            if (Input.GetKeyDown(s.keybind) && s.cdTimer <= 0)
            {
                CastSpell(s);
            }
        }
    }

    private void CastSpell(Spell s)
    {
        bool validCast = true;
        //check if valid cast
        //  find closest point in range/don't case if out of range
        //  only cast on valid terrain (flat)
        if (validCast)
        {
            s.cdTimer = s.cdMax;
            foreach (Block b in s.blocks)
            {
                if (b.tag == "shape")
                {
                    
                    Quaternion rot = Quaternion.Euler(0, Quaternion.LookRotation(transform.position - MousePos()).eulerAngles.y, 0);
                    GameObject hitbox = Instantiate(b.hitbox, MousePos(), rot);
                    StartCoroutine(FadeHitbox(hitbox, 0.5f));
                    //check for enemies, do stuff to them
                    //set spell cd
                    break;
                }
            }
        }
        else
        {
            Debug.Log("Invalid cast of " + s.name + "!");
        }   
    }


    private IEnumerator FadeHitbox(GameObject obj, float duration, bool destroyParent=false)
    {
        if (obj.transform.childCount > 0)
        {
            foreach (Transform child in obj.transform)
                StartCoroutine(FadeHitbox(child.gameObject, duration, true));
        }
        else
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
            if (destroyParent)
                Destroy(obj.transform.parent.gameObject);
            else
                Destroy(obj);
        }
    }


    private Vector3 MousePos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
}