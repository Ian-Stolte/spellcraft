using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellManager : MonoBehaviour
{
    public static SpellManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("Bools")]
    public bool spellsLocked;
    public bool pauseGame;

    [Header("Parents")]
    [SerializeField] private Transform spellUI;
    [SerializeField] private Transform blockParent;
    [SerializeField] private Transform symbolParent;

    [Header("Buttons")]
    [SerializeField] private GameObject craftButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject confirmButton;
    [SerializeField] private GameObject startButton;

    [Header("Prefabs")]
    [SerializeField] private GameObject emptyImage;
    [SerializeField] private GameObject spellListItem;

    [Header("Misc")]
    [SerializeField] private Color fullSymbolColor;
    [SerializeField] private CanvasGroup fader;
    public List<List<Block>> spells = new List<List<Block>>();
    


    public void CraftSpells()
    {
        spells.Clear();
        foreach (Transform child in blockParent)
        {
            Block script = child.GetComponent<Block>();
            if (script.left == null && script.right == null)
            {
                script.symbol.canMove = false;
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
            child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0.2f);
        }

        craftButton.SetActive(false);
        confirmButton.SetActive(true);
        backButton.SetActive(true);
        spellsLocked = true;

        foreach (List<Block> spell in spells)
        {
            foreach (Block b in spell)
            {
                Symbol s = b.symbol;
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
        confirmButton.SetActive(false);
        backButton.SetActive(false);
        symbolParent.gameObject.SetActive(true);
        int index = 0;
        foreach (List<Block> spell in spells)
        {
            GameObject spellHeader = Instantiate(emptyImage, Vector2.zero, Quaternion.identity, symbolParent);
            spellHeader.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            spellHeader.name = spell[0].name;
            Vector2 totalPos = Vector2.zero;
            string spellName = "";
            foreach (Block b in spell)
            {
                spellName += b.transform.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>().text + " ";
                Vector3 scale = new Vector3(b.symbol.transform.localScale.x*b.transform.localScale.x, b.symbol.transform.localScale.y*b.transform.localScale.y, 1);
                GameObject s = Instantiate(b.symbol.gameObject, b.symbol.transform.position, Quaternion.identity, spellHeader.transform);
                s.transform.localScale = scale;
                s.GetComponent<Image>().color = fullSymbolColor;
                totalPos += s.GetComponent<RectTransform>().anchoredPosition;
            }
            foreach (Transform child in spellHeader.transform)
            {
                child.GetComponent<RectTransform>().anchoredPosition -= totalPos/spellHeader.transform.childCount;
                child.transform.localScale *= 2.5f;
                child.GetComponent<RectTransform>().anchoredPosition *= 2.5f;
                Destroy(child.GetComponent<Symbol>());
                Destroy(child.GetComponent<BoxCollider2D>());
            }
            GameObject UI = Instantiate(spellListItem, Vector2.zero, Quaternion.identity, symbolParent);
            UI.GetComponent<RectTransform>().anchoredPosition = new Vector2(-80, 350-(index*300));
            UI.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = spellName;
            spellHeader.transform.SetSiblingIndex(spellHeader.transform.parent.childCount - 1);
            spellHeader.GetComponent<RectTransform>().anchoredPosition = new Vector2 (-665, 370-(index*300));
            index++;
        }
        startButton.SetActive(true);
    }

    public void EnterGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(EnterGameCor());
        
    }

    private IEnumerator EnterGameCor()
    {
        float elapsed = 0f;
        while (elapsed < 1)
        {
            elapsed += Time.deltaTime;
            fader.alpha = elapsed;
            yield return null;
        }
        startButton.SetActive(false);
        symbolParent.gameObject.SetActive(false);
        spellUI.gameObject.SetActive(false);
        while (elapsed > 0)
        {
            elapsed -= Time.deltaTime;
            fader.alpha = elapsed;
            yield return null;
        }
        fader.alpha = 0;
        GameObject.Find("Player").GetComponent<PlayerMovement>().enabled = true;
        pauseGame = false;
    }


    private void Update()
    {
        if (spellsLocked)
        {
            bool readyToConfirm = true;
            foreach (List<Block> spell in spells)
            {
                bool finished = true;
                foreach (Block b in spell)
                {
                    if (b.symbol.adjSymbols < spell.Count)
                    {
                        finished = false;
                        readyToConfirm = false;
                        break;
                    }
                }
                foreach (Block b in spell)
                {
                    b.transform.GetChild(1).gameObject.SetActive(finished);
                }
            }
            confirmButton.GetComponent<Button>().interactable = readyToConfirm;
        }
    }
}
