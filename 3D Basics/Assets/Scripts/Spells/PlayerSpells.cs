using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpells : MonoBehaviour
{
    [HideInInspector] public Spell auraSpell;
    [SerializeField] private float auraTick;
    [SerializeField] GameObject auraHitbox;
    
    [HideInInspector] public Spell autoSpell;
    public float autoTick;
    [SerializeField] private float autoTimer;


    private void Start()
    {
        if (auraSpell.name != "")
        {
            GameObject aura = Instantiate(auraHitbox, transform.position + new Vector3(0, -1, 0), Quaternion.identity, transform);
            aura.GetComponent<Hitbox>().spell = auraSpell;
            aura.GetComponent<AuraHitbox>().tickRate = auraTick;
        }
    }

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

        if (autoSpell.name != "")
        {
            autoTimer = Mathf.Max(0, autoTimer - Time.deltaTime);
            if (autoTimer <= 0)
            {
                CastSpell(autoSpell);
                autoTimer = autoTick;
            }
            autoSpell.fillTimer.GetComponent<Image>().fillAmount = autoTimer/autoTick;
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
                    break;
                }
                else if (b.name == "Melee")
                {
                    GameObject hitbox = Instantiate(b.hitbox, transform.position + new Vector3(0, -0.8f, 0), Quaternion.identity);
                    hitbox.GetComponent<Hitbox>().spell = s;
                    break;
                }
            }
        }
        else
        {
            Debug.Log("Invalid cast of " + s.name + "!");
        }   
    }


    public void SpellEffects(Collider[] cols, Spell s, Vector3 pos, bool aura=false)
    {
        foreach (Collider c in cols)
        {
            Enemy script = c.GetComponent<Enemy>();
            if (script != null)
            {
                int dmg = 0;
                foreach (Block b in s.blocks)
                {
                    if (b.name == "Stun")
                        script.stunTimer = (aura) ? 0.5f : 1f;   //change duration based on block??
                    else if (b.name == "Damage")
                        dmg += (aura) ? 2 : 4;
                    else if (b.name == "Knockback")
                    {
                        Vector3 dir = (c.transform.position - pos);
                        dir = (new Vector3(dir.x, 0.2f, dir.z)).normalized;
                        int kbStrength = (aura) ? 600 : 1000;
                        c.GetComponent<Rigidbody>().AddForce(dir*kbStrength, ForceMode.Impulse);
                        script.stunTimer = 0.5f;
                    }
                }
                if (dmg > 0)
                    script.TakeDamage(dmg);
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