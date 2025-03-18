using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellManager : MonoBehaviour
{
    public static SpellManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] private Transform blockParent;
    [SerializeField] private GameObject craftButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject confirmButton;

    public List<List<Block>> spells = new List<List<Block>>();
    public bool spellsLocked;


    public void CraftSpells()
    {
        spells.Clear();
        foreach (Transform child in blockParent)
        {
            Block script = child.GetComponent<Block>();
            if (script.left == null && script.right == null)
            {
                child.GetChild(0).GetComponent<Symbol>().canMove = false;
            }
            else if (script.left == null && script.right != null)
            {
                List<Block> newSpell = new List<Block>();
                Block temp = script;
                while (temp != null)
                {
                    newSpell.Add(temp);
                    temp = temp.right;
                }
                spells.Add(newSpell);
            }
            Color c = child.GetComponent<Image>().color;
            child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0.3f);
        }

        craftButton.SetActive(false);
        confirmButton.SetActive(true);
        backButton.SetActive(true);
        spellsLocked = true;

        foreach (List<Block> spell in spells)
        {
            foreach (Block b in spell)
            {
                var s = b.transform.GetChild(0).GetComponent<Symbol>();
                s.min = new Vector2(-80 * spell.IndexOf(b) - 40, s.min.y);
                s.max = new Vector2(80 * (spell.Count - spell.IndexOf(b)) - 40, s.max.y);
                s.canMove = true;
            }
        }
    }


    public void UndoSpells()
    {
        foreach (Transform child in blockParent)
        {
            Color c = child.GetComponent<Image>().color;
            child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1);
        }

        craftButton.SetActive(true);
        confirmButton.SetActive(false);
        backButton.SetActive(false);
        spellsLocked = false;
    }


    public void ConfirmSpells()
    {
        /*foreach (Block b in spells)
        {
            Debug.Log("Spell " + (spells.IndexOf(b)+1) + ": " + PrintSpell(b));
        }*/
        confirmButton.SetActive(false);
        backButton.SetActive(false);
    }

    private string PrintSpell(Block b)
    {
        if (b.right == null)
            return (b.gameObject.name);
        else
            return b.gameObject.name + " + " + PrintSpell(b.right);
    }
}
