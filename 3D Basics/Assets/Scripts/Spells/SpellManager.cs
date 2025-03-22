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
    public bool SKIP_CRAFTING;
    public bool spellsLocked;
    public bool pauseGame;

    [Header("Parents")]
    [SerializeField] private Transform spellUI;
    [SerializeField] private Transform blockParent;
    [SerializeField] private Transform symbolParent;
    [SerializeField] private Transform cdParent;

    [Header("Buttons")]
    [SerializeField] private GameObject craftButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject confirmButton;
    [SerializeField] private GameObject startButton;

    [Header("Prefabs")]
    [SerializeField] private GameObject emptyImage;
    [SerializeField] private GameObject spellListItem;
    [SerializeField] private GameObject cdIconPrefab;

    [Header("Misc")]
    [SerializeField] private Color fullSymbolColor;
    [SerializeField] private GameObject player;
    [SerializeField] private CanvasGroup fader;

    [Header("Spell Data")]
    public KeyCode[] defaultBinds;
    public string[] bindTxt;
    public List<Spell> spells = new List<Spell>();
    

    public void Start()
    {
        if (SKIP_CRAFTING)
        {
            List<Block> lineStun = new List<Block>();
            lineStun.Add(GameObject.Find("Line").GetComponent<Block>());
            lineStun.Add(GameObject.Find("Stun").GetComponent<Block>());
            Spell lineStunSpell = new Spell("versus stupefaciunt", lineStun, 5, KeyCode.Mouse0);
            spells.Add(lineStunSpell);
            List<Block> circleHeal = new List<Block>();
            circleHeal.Add(GameObject.Find("Circle").GetComponent<Block>());
            circleHeal.Add(GameObject.Find("Heal").GetComponent<Block>());
            Spell circleHealSpell = new Spell("orbis sano", circleHeal, 3, KeyCode.Mouse1);
            spells.Add(circleHealSpell);
            List<Block> zigzagUlt = new List<Block>();
            zigzagUlt.Add(GameObject.Find("ZigZag").GetComponent<Block>());
            zigzagUlt.Add(GameObject.Find("Stun").GetComponent<Block>());
            zigzagUlt.Add(GameObject.Find("Heal").GetComponent<Block>());
            Spell zigzagUltSpell = new Spell("oblicus stupefaciunt sano", zigzagUlt, 12, KeyCode.Mouse2);
            spells.Add(zigzagUltSpell);
            ConfirmSpells();
            EnterGame();
        }
    }


    public void CraftSpells()
    {
        spells.Clear();
        foreach (Transform child in blockParent)
        {
            if (child.gameObject.activeSelf)
            {
                Block script = child.GetComponent<Block>();
                if (script.left == null && script.right == null)
                {
                    script.symbol.canMove = false;
                    Color c = child.GetComponent<Image>().color;
                    child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0.1f);
                    child.GetChild(2).GetComponent<CanvasGroup>().alpha = 0.5f;
                    child.GetChild(3).GetComponent<CanvasGroup>().alpha = 0.1f;
                    child.GetChild(4).GetComponent<CanvasGroup>().alpha = 0.1f;
                }
                else
                {
                    if (script.left == null && script.right != null)
                    {
                        List<Block> newSpell = new List<Block>();
                        Block temp = script;
                        while (temp != null)
                        {
                            newSpell.Add(temp);
                            temp = temp.right;
                        }
                        spells.Add(new Spell(newSpell));
                    }
                    Color c = child.GetComponent<Image>().color;
                    child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0.3f);
                    child.GetChild(3).GetComponent<CanvasGroup>().alpha = 0.5f;
                }
            }
        }

        craftButton.SetActive(false);
        confirmButton.SetActive(true);
        backButton.SetActive(true);
        spellsLocked = true;

        foreach (Spell s in spells)
        {
            foreach (Block b in s.blocks)
            {
                b.transform.GetChild(4).gameObject.SetActive(false);
                b.transform.GetChild(0).GetComponent<Image>().enabled = true;
                Symbol sym = b.symbol;
                sym.min = new Vector2(-80 * s.blocks.IndexOf(b) - 40, sym.min.y);
                sym.max = new Vector2(80 * (s.blocks.Count - s.blocks.IndexOf(b)) - 40, sym.max.y);
                sym.canMove = true;
            }
        }
    }


    public void UndoSpells()
    {
        foreach (Transform child in blockParent)
        {
            if (child.gameObject.activeSelf)
            {
                Color c = child.GetComponent<Image>().color;
                child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1);
                child.GetChild(2).GetComponent<CanvasGroup>().alpha = 1;
                child.GetChild(3).GetComponent<CanvasGroup>().alpha = 1;
                child.GetChild(4).GetComponent<CanvasGroup>().alpha = 1;
                child.GetChild(4).gameObject.SetActive(true);
                child.GetChild(0).GetComponent<Image>().enabled = false;
            }
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
        foreach (Spell s in spells)
        {
            s.symbol = Instantiate(emptyImage, Vector2.zero, Quaternion.identity, symbolParent);
            s.symbol.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            s.symbol.name = s.name;
            Vector2 totalPos = Vector2.zero;
            string spellName = "";
            float cd = 0;
            foreach (Block b in s.blocks)
            {
                spellName += b.transform.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>().text + " ";
                cd += b.cd;
                Vector3 scale = new Vector3(b.symbol.transform.localScale.x*b.transform.localScale.x, b.symbol.transform.localScale.y*b.transform.localScale.y, 1);
                GameObject sym = Instantiate(b.symbol.gameObject, b.symbol.transform.position, Quaternion.identity, s.symbol.transform);
                sym.transform.localScale = scale;
                sym.GetComponent<Image>().color = fullSymbolColor;
                totalPos += sym.GetComponent<RectTransform>().anchoredPosition;
            }
            foreach (Transform child in s.symbol.transform)
            {
                child.GetComponent<RectTransform>().anchoredPosition -= totalPos/s.symbol.transform.childCount;
                child.transform.localScale *= 2.5f;
                child.GetComponent<RectTransform>().anchoredPosition *= 2.5f;
                Destroy(child.GetComponent<Symbol>());
                Destroy(child.GetComponent<BoxCollider2D>());
            }
            GameObject UI = Instantiate(spellListItem, Vector2.zero, Quaternion.identity, symbolParent);
            UI.GetComponent<RectTransform>().anchoredPosition = new Vector2(-80, 350-(index*300));
            UI.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = spellName;
            s.name = spellName;
            string cdTxt = ((""+cd).Length == 1) ? cd + ".0s" : cd + "s"; 
            UI.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = cdTxt;
            s.cdMax = cd;
            s.symbol.transform.SetSiblingIndex(s.symbol.transform.parent.childCount - 1);
            s.symbol.GetComponent<RectTransform>().anchoredPosition = new Vector2 (-665, 370-(index*300));
            //TODO: let player assign keybinds
            if (index < defaultBinds.Length)
                s.keybind = defaultBinds[index];
            index++;
        }
        startButton.SetActive(true);
    }


    public void EnterGame()
    {
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
        int index = 0;
        foreach (Spell s in spells)
        {
            if (index < defaultBinds.Length)
            {
                Transform cdIcon = Instantiate(cdIconPrefab, Vector2.zero, Quaternion.identity, cdParent).transform;
                cdIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-800 + (170*index), -465);
                cdIcon.GetChild(0).GetComponent<TextMeshProUGUI>().text = bindTxt[index];
                Transform symbol = Instantiate(s.symbol, Vector2.zero, Quaternion.identity, cdIcon).transform;
                symbol.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                symbol.localScale /= 2.5f;
                symbol.SetSiblingIndex(cdIcon.childCount - 2);
                s.fillTimer = cdIcon.GetChild(cdIcon.childCount-1).gameObject;
            }
            index++;
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
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerSpells>().enabled = true;
        pauseGame = false;
    }


    private void Update()
    {
        if (spellsLocked)
        {
            if (spells.Count == 0)
            {
                confirmButton.GetComponent<Button>().interactable = false;
            }
            else
            {
                bool readyToConfirm = true;
                foreach (Spell s in spells)
                {
                    bool finished = true;
                    foreach (Block b in s.blocks)
                    {
                        if (b.symbol.adjSymbols < s.blocks.Count)
                        {
                            finished = false;
                            readyToConfirm = false;
                            break;
                        }
                    }
                    foreach (Block b in s.blocks)
                    {
                        b.transform.GetChild(1).gameObject.SetActive(finished);
                    }
                }
                confirmButton.GetComponent<Button>().interactable = readyToConfirm;
            }
        }
    }
}


[System.Serializable]
public class Spell
{
    public Spell(string name_, List<Block> blocks_, float cd, KeyCode bind)
    {
        name = name_;
        blocks = blocks_;
        cdMax = cd;
        keybind = bind;
    }

    public Spell(List<Block> blocks_)
    {
        blocks = blocks_;
        name = blocks_[0].name;
    }

    public string name;
    public List<Block> blocks;
    public float cdMax;
    public float cdTimer;
    [HideInInspector] public GameObject fillTimer;
    public KeyCode keybind;
    public GameObject symbol;
}