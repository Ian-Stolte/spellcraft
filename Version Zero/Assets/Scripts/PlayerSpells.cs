using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpells : MonoBehaviour
{
    [Header("Aura")]
    public float auraTick;
    public float auraDampen;
    [SerializeField] GameObject auraHitbox;
    private GameObject auraObj;
    [HideInInspector] public Spell auraSpell;
    
    [Header("Auto")]
    public float autoTick;
    [SerializeField] private float autoTimer;
    [HideInInspector] public Spell autoSpell;
    
    [Header("Hitboxes")]
    [SerializeField] private GameObject[] hitboxes;

    [Header("Misc")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash")]
    [SerializeField] private float dashForce;
    [SerializeField] private float dashDur;
    [HideInInspector] public bool dashing;
    [SerializeField] private Material transparentMat;


    public void InitializeAura()
    {
        Destroy(auraObj);
        if (auraSpell.name != "")
        {
            auraObj = Instantiate(auraHitbox, transform.position + new Vector3(0, -1, 0), Quaternion.identity, transform);
            auraObj.GetComponent<Hitbox>().spell = auraSpell;
            auraObj.GetComponent<AuraHitbox>().tickRate = auraTick;
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
            else if (Input.GetKeyDown(s.keybind) && s.cdTimer <= 0.5f)
            {
                StartCoroutine(DelayedCast(s));
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


    private IEnumerator DelayedCast(Spell s)
    {
        yield return new WaitUntil(() => s.cdTimer <= 0);
        CastSpell(s);
    }

    private void CastSpell(Spell s)
    {
        Block dash = s.blocks.Find(b=>b.name == "Dash");
        if (dash != null)
        {
            StartCoroutine(Dash(s));
        }
        else
        {
            bool validCast = true;
            //check if valid cast
            //  find closest point in range/don't cast if out of range
            if (validCast)
            {
                s.cdTimer = s.cdMax;
                Quaternion rot = Quaternion.Euler(0, Quaternion.LookRotation(transform.position - MousePos()).eulerAngles.y, 0);
                foreach (Block b in s.blocks)
                {
                    if (b.name == "Circle")
                    {
                        GameObject hitbox = Instantiate(hitboxes[0], MousePos(), rot);
                        hitbox.GetComponent<Hitbox>().spell = s;
                        break;
                    }
                    else if (b.name == "Line")
                    {
                        Vector3 dir = (MousePos() - transform.position);
                        dir = new Vector3(dir.x, 0, dir.z).normalized;
                        GameObject hitbox = Instantiate(hitboxes[1], transform.position + dir, rot);
                        hitbox.GetComponent<LineHitbox>().dir = dir;
                        hitbox.GetComponent<Hitbox>().spell = s;
                        break;
                    }
                    else if (b.name == "Melee" || b.name == "Shield")
                    {
                        GameObject hitbox = Instantiate(hitboxes[2], transform.position + new Vector3(0, -0.8f, 0), Quaternion.identity);
                        hitbox.GetComponent<Hitbox>().spell = s;
                        if (b.name == "Shield")
                            GetComponent<PlayerMovement>().shieldTimer += 1f;
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("Invalid cast of " + s.name + "!");
            }   
        }
    }


    public void SpellEffects(Collider[] cols, Spell s, Vector3 pos, bool aura=false)
    {
        foreach (Collider c in cols)
        {
            Enemy script = c.GetComponent<Enemy>();
            if (script != null)
            {
                //TODO: better way to do this??
                int dmg = 0;
                int burn = 0;
                int markDmg = 0;
                float stun = 0;
                float slow = 0;
                foreach (Block b in s.blocks)
                {
                    if (b.name == "Stun")
                        stun += (aura) ? 0.3f : 1f; 
                    else if (b.name == "Slow")
                        slow += (aura) ? 0.5f : 1.5f; 
                    else if (b.name == "Damage")
                        dmg += (aura) ? 1 : 4;
                    else if (b.name == "Burn")
                        burn += (aura) ? 1 : 1;
                    else if (b.name == "Mark")
                        markDmg += (aura) ? 1 : 8;
                    //else if (b.name == "Crit")
                    //    dmg += (aura) ? Random.Range(0, 3) : Random.Range(1, 8);
                    else if (b.name == "Displace")
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
                if (burn > 0)
                {
                    if (aura)
                    {
                        if (script.auraBurn != null)
                            StopCoroutine(script.auraBurn);
                        script.auraBurn = script.ApplyBurn(burn, 4);
                        StartCoroutine(script.auraBurn);
                    }
                    else
                        StartCoroutine(script.ApplyBurn(burn, 4));
                }
                if (markDmg > 0)
                {
                    script.MarkDamage(markDmg);
                }
                if (stun > 0)
                    script.stunTimer = stun;
                if (slow > 0)
                    script.slowTimer = slow;
            }
        }
    }


    private IEnumerator Dash(Spell s)
    {
        dashing = true;
        Physics.IgnoreLayerCollision(6, 12, true);
        GetComponent<TrailRenderer>().emitting = true;
        Vector3 dir = (MousePos() - transform.position);
        dir = new Vector3(dir.x, 0, dir.z).normalized;
        StartCoroutine(GoTransparent(0.3f));

        float elapsed = 0;
        while (elapsed < dashDur)
        {
            GetComponent<Rigidbody>().velocity = dir * dashForce * (-Mathf.Pow((elapsed/dashDur), 2) + 1);
            elapsed += Time.deltaTime;
            yield return null;
        }
        //cast spell
        s.cdTimer = s.cdMax;
        GameObject hitbox = Instantiate(hitboxes[2], transform.position + new Vector3(0, -0.8f, 0), Quaternion.identity);
        hitbox.GetComponent<Hitbox>().spell = s;
        
        dashing = false;
        GetComponent<TrailRenderer>().emitting = false;
        Physics.IgnoreLayerCollision(6, 12, false);
    }


    private IEnumerator GoTransparent(float duration, float a=0.5f)
    {
        List<Material> origMats = new List<Material>();
        foreach (Transform child in transform.GetChild(1))
        {
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                origMats.Add(renderer.material);
                renderer.material = transparentMat;
                //apply custom a
            }
            else
            {
                origMats.Add(transparentMat);
            }
        }
        yield return new WaitForSeconds(duration);
        for (int i = 0; i < origMats.Count; i++)
        {
            MeshRenderer renderer = transform.GetChild(1).GetChild(i).GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = origMats[i];
                //apply custom a
            }
        }
    }


    private Vector3 MousePos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
}