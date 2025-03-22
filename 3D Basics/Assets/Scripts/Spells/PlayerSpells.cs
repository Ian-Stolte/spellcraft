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
            Quaternion rot = Quaternion.Euler(0, Quaternion.LookRotation(transform.position - MousePos()).eulerAngles.y, 0);
            foreach (Block b in s.blocks)
            {
                if (b.name == "Circle" || b.name == "ZigZag")
                {
                    GameObject hitbox = Instantiate(b.hitbox, MousePos(), rot);
                    hitbox.GetComponent<Hitbox>().spell = s;
                    break;
                }
                else if (b.name == "Line")
                {
                    Vector3 dir = (MousePos() - transform.position);
                    dir = new Vector3(dir.x, 0, dir.z).normalized;
                    GameObject hitbox = Instantiate(b.hitbox, transform.position + dir, rot);
                    hitbox.GetComponent<LineHitbox>().dir = dir;
                    hitbox.GetComponent<Hitbox>().spell = s;
                }
            }
        }
        else
        {
            Debug.Log("Invalid cast of " + s.name + "!");
        }   
    }

    public void SpellEffects(Collider[] cols, Spell s)
    {
        foreach (Collider c in cols)
        {
            Enemy script = c.GetComponent<Enemy>();
            if (script != null)
            {
                script.TakeDamage(3);
                foreach (Block b in s.blocks)
                {
                    if (b.tag != "shape")
                    {
                        Debug.Log("Apply " + b.name + " effect!");
                    }
                }
            }
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