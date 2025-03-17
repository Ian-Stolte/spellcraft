using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellManager : MonoBehaviour
{
    [SerializeField] private Transform blockParent;
    [SerializeField] private GameObject craftButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject confirmButton;

    public List<Block> spells = new List<Block>();


    public void CraftSpells()
    {
        spells.Clear();
        foreach (Transform child in blockParent)
        {
            Block script = child.GetComponent<Block>();
            if (script.left == null && script.right == null)
            {
                Debug.Log(child.name + " is UNCONNECTED");
            }
            else if (script.left == null && script.right != null)
            {
                spells.Add(script);
            }
            Color c = child.GetComponent<Image>().color;
            child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0.3f);
        }

        craftButton.SetActive(false);
        confirmButton.SetActive(true);
        backButton.SetActive(true);

        foreach (Block b in spells)
        {
            //TODO: bring up symbol-editing window
            //then change craft button -> confirm, disabled until all symbols combined properly
            //also add a back button
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
    }


    public void ConfirmSpells()
    {
        foreach (Block b in spells)
        {
            Debug.Log("Spell " + (spells.IndexOf(b)+1) + ": " + PrintSpell(b));
        }
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
