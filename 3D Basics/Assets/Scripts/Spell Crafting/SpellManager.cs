using System.Linq;
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
    private bool loadNextRoom;
    private bool musicOn;

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
    [SerializeField] private GameObject skipButton;

    [Header("Prefabs")]
    [SerializeField] private GameObject emptyImage;
    [SerializeField] private GameObject spellListItem;
    [SerializeField] private GameObject cdIconPrefab;

    [Header("Misc")]
    [SerializeField] private Vector2 spellUIStart;
    [SerializeField] private Color fullSymbolColor;
    [SerializeField] private PlayerSpells player;

    [Header("Keybinds")]
    public KeyCode[] defaultBinds;
    public string[] bindTxt;

    [Header("Spell Data")]
    public List<GameObject> blocks;
    public List<Spell> spells = new List<Spell>();
    [HideInInspector] public List<Spell> spellSave = new List<Spell>();
    

    public void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerSpells>();

        if (SKIP_CRAFTING)
        {
            List<Block> lineStun = new List<Block>();
            lineStun.Add(GameObject.Find("Line").GetComponent<Block>());
            lineStun.Add(GameObject.Find("Damage").GetComponent<Block>());
            Spell lineStunSpell = new Spell("versus noxa", lineStun, 5, KeyCode.Mouse0);
            spells.Add(lineStunSpell);
            List<Block> circleHeal = new List<Block>();
            circleHeal.Add(GameObject.Find("Circle").GetComponent<Block>());
            circleHeal.Add(GameObject.Find("Knockback").GetComponent<Block>());
            Spell circleHealSpell = new Spell("orbis repellare", circleHeal, 3, KeyCode.Mouse1);
            spells.Add(circleHealSpell);
            List<Block> meleeUlt = new List<Block>();
            meleeUlt.Add(GameObject.Find("Melee").GetComponent<Block>());
            meleeUlt.Add(GameObject.Find("Stun").GetComponent<Block>());
            meleeUlt.Add(GameObject.Find("Damage").GetComponent<Block>());
            Spell meleeUltSpell = new Spell("ipsus stupefaciunt noxa", meleeUlt, 12, KeyCode.Mouse2);
            spells.Add(meleeUltSpell);
            ConfirmSpells();
            EnterGame();
        }
    }


    public void Reforge()
    {
        loadNextRoom = true;
        pauseGame = true;
        player.GetComponent<PlayerMovement>().enabled = false;
        player.enabled = false;
        cdParent.gameObject.SetActive(false);
        spellUI.gameObject.SetActive(true);
        skipButton.SetActive(true);
        craftButton.SetActive(true);
        confirmButton.SetActive(false);
        spellsLocked = false;

        foreach (Transform child in blockParent)
        {
            Block b = child.GetComponent<Block>();
            if (child.gameObject.activeSelf)
            {
                b.SaveState();

                if (b.left != null || b.right != null) //if crafted spell
                {
                    Color c = child.GetComponent<Image>().color;
                    child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0.5f);
                    b.symbol.GetComponent<Image>().enabled = true;
                    b.highlight.gameObject.SetActive(true);
                    b.nameTxt.GetComponent<CanvasGroup>().alpha = 1;
                    b.latin.GetComponent<CanvasGroup>().alpha = 0.5f;
                    b.cdText.gameObject.SetActive(false);
                }
                else
                {
                    Color c = child.GetComponent<Image>().color;
                    child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1);
                    b.symbol.GetComponent<Image>().enabled = false;
                    b.nameTxt.GetComponent<CanvasGroup>().alpha = 1;
                    b.latin.GetComponent<CanvasGroup>().alpha = 1;
                    b.cdText.GetComponent<CanvasGroup>().alpha = 1;
                    b.cdText.gameObject.SetActive(true);
                }
            }
        }
        spellSave.Clear();
        foreach (Spell s in spells)
        {
            spellSave.Add(s);
        }
    }


    public void ExitReforge()
    {
        StartCoroutine(ExitReforgeCor());
    }

    private IEnumerator ExitReforgeCor()
    {
        if (loadNextRoom)
        {
            StartCoroutine(GameManager.Instance.LoadNextRoom());
            yield return new WaitForSeconds(1f);
        }
        else
        {
            Fader.Instance.FadeIn(0.5f);
            yield return new WaitForSeconds(0.5f);
        }

        foreach (Transform child in blockParent)
        {
            if (child.gameObject.activeSelf)
                child.GetComponent<Block>().ReadState();
        }
        spells.Clear();
        foreach (Spell s in spellSave)
        {
            spells.Add(s);
        }
        pauseGame = false;
        spellUI.gameObject.SetActive(false);
        cdParent.gameObject.SetActive(true);
        player.GetComponent<PlayerMovement>().enabled = true;
        player.enabled = true;

                    
        if (loadNextRoom)
            loadNextRoom = false;
        else
            Fader.Instance.FadeOut(0.5f);
    }


    public void CraftSpells()
    {
        spells.Clear();
        foreach (Transform child in blockParent)
        {
            if (child.gameObject.activeSelf)
            {
                Block b = child.GetComponent<Block>();
                if (b.left == null && b.right == null)
                {
                    b.symbol.canMove = false;
                    Color c = child.GetComponent<Image>().color;
                    child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0.1f);
                    b.nameTxt.GetComponent<CanvasGroup>().alpha = 0.5f;
                    b.latin.GetComponent<CanvasGroup>().alpha = 0.1f;
                    b.cdText.GetComponent<CanvasGroup>().alpha = 0.1f;
                }
                else
                {
                    if (b.left == null && b.right != null)
                    {
                        List<Block> newSpell = new List<Block>();
                        Block temp = b;
                        while (temp != null)
                        {
                            newSpell.Add(temp);
                            temp = temp.right;
                        }
                        spells.Add(new Spell(newSpell));
                    }
                    Color c = child.GetComponent<Image>().color;
                    child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0.3f);
                    b.latin.GetComponent<CanvasGroup>().alpha = 0.5f;
                    b.cdText.SetActive(false);
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
                Block b = child.GetComponent<Block>();
                Color c = child.GetComponent<Image>().color;
                child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1);
                b.nameTxt.GetComponent<CanvasGroup>().alpha = 1;
                b.latin.GetComponent<CanvasGroup>().alpha = 1;
                b.cdText.GetComponent<CanvasGroup>().alpha = 1;
                b.cdText.SetActive(true);

                child.GetChild(0).GetComponent<Image>().enabled = false;
                b.highlight.gameObject.SetActive(false); //highlight
                b.cdText.gameObject.SetActive(true); //cd text
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
        
        foreach (Transform child in symbolParent)
        {
            Destroy(child.gameObject);
        }

        //filter out aura and auto spells
        foreach (Spell s in spells)
        {
            float cd = 0;
            bool addedAuto = false;
            foreach (Block b in s.blocks)
            {
                cd += b.cd;
                if (b.name == "Aura")
                {
                    player.auraSpell = s;
                }
                else if (b.name == "Auto")
                {
                    player.autoSpell = s;
                    addedAuto = true;
                }
            }
            if (addedAuto)
            {
                player.autoTick = cd/2f;
            }
        }

        //add other spells to results UI
        int index = 0;
        int bindIndex = 0;
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
                spellName += b.latin.GetComponent<TMPro.TextMeshProUGUI>().text + " ";
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
            s.symbol.GetComponent<RectTransform>().anchoredPosition = spellUIStart + new Vector2(0, -(index*300));
            index++;
            //TODO: let player assign keybinds
            if (bindIndex < defaultBinds.Length && s != player.autoSpell && s != player.auraSpell)
            {
                s.keybind = defaultBinds[bindIndex];
                bindIndex++;
            }
        }
        if (player.auraSpell.name != "")
            spells.Remove(player.auraSpell);
        if (player.autoSpell.name != "")
            spells.Remove(player.autoSpell);
        startButton.SetActive(true);
    }


    private void Update()
    {
        //TODO: show warning if player attaches passive to just effect/just shape & disable craft button

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
                        b.highlight.gameObject.SetActive(finished);
                    }
                }
                confirmButton.GetComponent<Button>().interactable = readyToConfirm;
            }
        }
    }


    public void EnterGame()
    {
        StartCoroutine(EnterGameCor());
    }

    public IEnumerator EnterGameCor()
    {
        if (!musicOn)
        {
            musicOn = true;
            StartCoroutine(AudioManager.Instance.StartFade("Dreamer", 5, 0.4f));
            AudioManager.Instance.Play("Dreamer");
        }
        foreach (Transform child in cdParent)
        {
            Destroy(child.gameObject);
        }
        cdParent.gameObject.SetActive(true);

        if (loadNextRoom)
            StartCoroutine(GameManager.Instance.LoadNextRoom());
        else
            Fader.Instance.FadeIn(1);
        yield return new WaitForSeconds(1);
        
        int index = 0;
        foreach (Spell s in spells)
        {
            if (index < defaultBinds.Length)
            {
                SpawnSpellIcon(s, new Vector2(-800 + (170*index), -465), bindTxt[index]);
            }
            index++;
        }
        index = 0;
        if (player.auraSpell.name != "")
        {
            SpawnSpellIcon(player.auraSpell, new Vector2(800, -465), "AURA");
            index++;
        }
        if (player.autoSpell.name != "")
        {
            SpawnSpellIcon(player.autoSpell, new Vector2(800 - (170*index), -465), "AUTO");
        }

        startButton.SetActive(false);
        symbolParent.gameObject.SetActive(false);
        spellUI.gameObject.SetActive(false);

        if (loadNextRoom)
            loadNextRoom = false;
        else
            Fader.Instance.FadeOut(1);
        player.GetComponent<PlayerMovement>().enabled = true;
        player.enabled = true;
        player.InitializeAura();
        pauseGame = false;
    }

    private void SpawnSpellIcon(Spell s, Vector2 pos, string txt)
    {
        Transform cdIcon = Instantiate(cdIconPrefab, Vector2.zero, Quaternion.identity, cdParent).transform;
        cdIcon.GetComponent<RectTransform>().anchoredPosition = pos;
        cdIcon.GetChild(0).GetComponent<TextMeshProUGUI>().text = txt;
        Transform symbol = Instantiate(s.symbol, Vector2.zero, Quaternion.identity, cdIcon).transform;
        symbol.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        symbol.localScale /= 2.5f;
        symbol.SetSiblingIndex(cdIcon.childCount - 2);
        s.fillTimer = cdIcon.GetChild(cdIcon.childCount-1).gameObject;
        if (txt == "AURA")
            s.fillTimer.GetComponent<Image>().fillAmount = 1;
    }


    public List<Block> ChooseRandom(int n)
    {
        //TODO: add diff percents
        //TODO: add rarities

        List<Block> starting = new List<Block>();
        List<Block> chosen = new List<Block>();
        foreach (GameObject g in blocks)
        {
            starting.Add(g.GetComponent<Block>());   
        }

        for (int i = 0; i < n; i++)
        {
            Block b = starting[Random.Range(0, starting.Count)];
            chosen.Add(b);
            starting.Remove(b);
            float rarity = Random.Range(0f, 1f);
            if (rarity > 0.9f)
                b.rarity = 4;
            else if (rarity > 0.7f)
                b.rarity = 3;
            else if (rarity > 0.4f)
                b.rarity = 2;
            else
                b.rarity = 1;
        }

        return chosen;
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