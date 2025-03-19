using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpells : MonoBehaviour
{
    public List<KeyCode> spellKeybinds;
    

    void Update()
    {
        foreach (KeyCode k in spellKeybinds)
        {
            if (Input.GetKeyDown(k))
            {
                CastSpell(SpellManager.Instance.spells[spellKeybinds.IndexOf(k)]);
            }
        }
    }

    private void CastSpell(List<Block> spell)
    {
        foreach (Block b in spell)
        {
            if (b.tag == "shape")
            {
                //find closest point in range/don't case if out of range
                //only cast on valid terrain (flat)
                Quaternion rot = Quaternion.Euler(0, Quaternion.LookRotation(transform.position - MousePos()).eulerAngles.y, 0);
                GameObject hitbox = Instantiate(b.hitbox, MousePos(), rot);
                //check for enemies, do stuff to them
                //set spell cd
            }
            break;
        }
        Debug.Log("Casting: " + spell[0].name);
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
