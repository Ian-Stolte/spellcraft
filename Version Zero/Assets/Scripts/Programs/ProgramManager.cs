using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgramManager : MonoBehaviour
{
    public static ProgramManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("Bools")]
    public bool SKIP_CRAFTING;
    [SerializeField] private bool showTutorial;
    [HideInInspector] public bool spellsLocked;
    private bool loadNextLevel;
    private bool musicOn;
    private bool moreInfo;

    [Header("Parents")]
    public Transform programUI;
    [SerializeField] private Transform blockParent;
    [SerializeField] private Transform keybindUI;
    [SerializeField] private Transform keybindIcons;
    [SerializeField] private Transform programList;
    [SerializeField] private Transform cdParent;

    [Header("Buttons")]
    public GameObject compileButton;
    [SerializeField] private TextMeshProUGUI infoButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject confirmButton;
    [SerializeField] private GameObject randomButton;
    [SerializeField] private GameObject startButton;
    public GameObject skipButton;

    [Header("Prefabs")]
    [SerializeField] private GameObject emptyImage;
    [SerializeField] private GameObject spellListItem;
    [SerializeField] private GameObject cdIconPrefab;

    [Header("Colors")]
    [SerializeField] private Color fullSymbolColor;
    [SerializeField] private Color[] typeColors;

    [Header("Misc")]
    [SerializeField] private PlayerPrograms player;
    [SerializeField] private GameObject[] tutorials;
    public GameObject buildSelect;

    [Header("Keybinds")]
    public KeyCode[] defaultBinds;
    public string[] bindTxt;
    [HideInInspector] public KeybindButton activeKeybind;

    [Header("Program Data")]
    public string buildpath;
    //TODO: read directly from prefab folders
    public List<GameObject> shapeBlocks;
    public List<GameObject> effectBlocks;
    public List<GameObject> modBlocks;
    private List<GameObject> blocks = new List<GameObject>();
    public List<Program> programs = new List<Program>();
    [HideInInspector] public List<Program> spellSave = new List<Program>();
    

    public void Start()
    {
        if (GameObject.Find("Options Manager") == null)
            StartingHand();
    }

    public void StartingHand()
    {
        buildpath = "logic";
        foreach (GameObject g in shapeBlocks)
            blocks.Add(g);
        foreach (GameObject g in effectBlocks)
            blocks.Add(g);
        foreach (GameObject g in modBlocks)
            blocks.Add(g);
        player = GameObject.Find("Player").GetComponent<PlayerPrograms>();
        if (SKIP_CRAFTING)
        {
            if (buildSelect != null)
                buildSelect.SetActive(false);

            string[] startingBlocks = new string[]{"Line", "Damage", "Circle", "Displace", "Melee", "Stun", "Damage"};
            foreach (string s in startingBlocks)
            {
                GameObject prefab = blocks.Find(b=>b.name == s);
                if (prefab == null)
                    Debug.LogError("Starting block prefab not found!");
                else
                    CreateBlock(prefab);
            }
            List<Block> lineStun = new List<Block>();
            lineStun.Add(GameObject.Find("Line").GetComponent<Block>());
            lineStun.Add(GameObject.Find("Damage").GetComponent<Block>());
            Program lineStunSpell = new Program(lineStun, KeyCode.Mouse0);
            programs.Add(lineStunSpell);
            List<Block> circleDisplace = new List<Block>();
            circleDisplace.Add(GameObject.Find("Circle").GetComponent<Block>());
            circleDisplace.Add(GameObject.Find("Displace").GetComponent<Block>());
            Program circleDisplaceSpell = new Program(circleDisplace, KeyCode.Mouse1);
            programs.Add(circleDisplaceSpell);
            List<Block> meleeUlt = new List<Block>();
            meleeUlt.Add(GameObject.Find("Melee").GetComponent<Block>());
            meleeUlt.Add(GameObject.Find("Stun").GetComponent<Block>());
            meleeUlt.Add(GameObject.Find("Damage").GetComponent<Block>());
            Program meleeUltSpell = new Program(meleeUlt, KeyCode.Mouse2);
            programs.Add(meleeUltSpell);
            ConfirmSpells();
            EnterGame();
        }
        tutorials[0].SetActive(true);
        tutorials[0].transform.GetChild(0).gameObject.SetActive(showTutorial);
    }


    public void CreateBlock(string blockName)
    {
        CreateBlock(blocks.Find(b=>b.name == blockName));
    }

    public void CreateBlock(GameObject prefab)
    {
        GameObject block = Instantiate(prefab, Vector2.zero, Quaternion.identity, blockParent);
        RectTransform r = block.GetComponent<RectTransform>();
        for (int j = 0; j < 20; j++)
        {
            r.anchoredPosition = new Vector2(Random.Range(-(860 - r.sizeDelta.x), 850 - r.sizeDelta.x), Random.Range(-415, 415));
            //if (Physics2D.OverlapCircleAll(r.anchoredPosition, 200, LayerMask.GetMask("Block")).Length <= 1)
            //    break;
            bool noOverlap = true;
            foreach (Transform child in blockParent)
            {
                if (child.gameObject != block && child.gameObject.activeSelf && Vector3.Distance(block.GetComponent<RectTransform>().anchoredPosition, child.GetComponent<RectTransform>().anchoredPosition) < 400)
                {
                    noOverlap = false;
                }
            }
            if (noOverlap)
                break;
        }
        block.name = block.name.Substring(0, block.name.Length-7);
    }


    public void Reforge()
    {
        //loadNextLevel = true;
        GameManager.Instance.pauseGame = true;
        //player.GetComponent<PlayerMovement>().enabled = false;
        player.enabled = false;
        cdParent.gameObject.SetActive(false);
        programUI.gameObject.SetActive(true);
        skipButton.SetActive(true);
        compileButton.SetActive(true);
        confirmButton.SetActive(false);
        randomButton.SetActive(false);
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
                    b.typeTxt.GetComponent<CanvasGroup>().alpha = 0.5f;
                    b.typeTxt.gameObject.SetActive(true);
                    b.cdTxt.gameObject.SetActive(false);
                }
                else
                {
                    Color c = child.GetComponent<Image>().color;
                    child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1);
                    b.symbol.GetComponent<Image>().enabled = false;
                    b.nameTxt.GetComponent<CanvasGroup>().alpha = 1;
                    b.typeTxt.GetComponent<CanvasGroup>().alpha = 1;
                    b.cdTxt.GetComponent<CanvasGroup>().alpha = 1;
                    b.cdTxt.gameObject.SetActive(true);
                }
            }
        }
        spellSave.Clear();
        foreach (Program p in programs)
        {
            spellSave.Add(p);
        }
    }


    public void ExitReforge()
    {
        StartCoroutine(ExitReforgeCor());
    }

    private IEnumerator ExitReforgeCor()
    {
        
        if (loadNextLevel)
        {
            StartCoroutine(GameManager.Instance.LoadNextLevel(GameObject.Find("End Elevator").GetComponent<Elevator>().nextArea));
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
        programs.Clear();
        foreach (Program p in spellSave)
        {
            programs.Add(p);
        }
        GameManager.Instance.pauseGame = false;
        GameManager.Instance.playerPaused = false;
        programUI.gameObject.SetActive(false);
        cdParent.gameObject.SetActive(true);
        player.GetComponent<PlayerMovement>().enabled = true;
        player.enabled = true;

                    
        if (loadNextLevel)
            loadNextLevel = false;
        else
            Fader.Instance.FadeOut(0.5f);
    }


    public void CompileSpells()
    {
        programs.Clear();
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
                    b.typeTxt.GetComponent<CanvasGroup>().alpha = 0.1f;
                    b.cdTxt.GetComponent<CanvasGroup>().alpha = 0.1f;
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
                        programs.Add(new Program(newSpell));
                    }
                    Color c = child.GetComponent<Image>().color;
                    child.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0.3f);
                    b.typeTxt.GetComponent<CanvasGroup>().alpha = 0.5f;
                    b.cdTxt.SetActive(false);
                }
            }
        }

        if (showTutorial)
        {
            tutorials[0].SetActive(false);
            tutorials[1].SetActive(true);
        }
        compileButton.SetActive(false);
        confirmButton.SetActive(true);
        randomButton.SetActive(true);
        backButton.SetActive(true);
        spellsLocked = true;

        foreach (Program p in programs)
        {
            foreach (Block b in p.blocks)
            {
                b.transform.GetChild(4).gameObject.SetActive(false);
                b.transform.GetChild(0).GetComponent<Image>().enabled = true;
                Symbol sym = b.symbol;
                sym.min = new Vector2(-80 * p.blocks.IndexOf(b) - 40, sym.min.y);
                sym.max = new Vector2(80 * (p.blocks.Count - p.blocks.IndexOf(b)) - 40, sym.max.y);
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
                b.typeTxt.GetComponent<CanvasGroup>().alpha = 1;
                b.cdTxt.GetComponent<CanvasGroup>().alpha = 1;
                b.cdTxt.SetActive(true);

                child.GetChild(0).GetComponent<Image>().enabled = false;
                b.highlight.gameObject.SetActive(false); //highlight
                b.cdTxt.gameObject.SetActive(true); //cd text
            }
        }

        if (showTutorial)
        {
            tutorials[0].SetActive(true);
            tutorials[1].SetActive(false);
        }
        compileButton.SetActive(true);
        confirmButton.SetActive(false);
        randomButton.SetActive(false);
        backButton.SetActive(false);
        spellsLocked = false;
    }


    public void RandomSymbols()
    {
        foreach (Transform child in blockParent)
        {
            Block b = child.GetComponent<Block>();
            Vector2 offset = new Vector2(Random.Range(-20f, 20f), Random.Range(-10f, 10f));
            b.symbol.GetComponent<RectTransform>().anchoredPosition = new Vector2((b.symbol.min.x + b.symbol.max.x)/2f * 1.35f, (b.symbol.min.y + b.symbol.max.y)/2f) + offset;

        }
        ConfirmSpells();
    }


    public void ConfirmSpells()
    {
        showTutorial = false;
        foreach (GameObject g in tutorials)
            g.SetActive(false);
        confirmButton.SetActive(false);
        randomButton.SetActive(false);
        backButton.SetActive(false);
        keybindUI.gameObject.SetActive(true);
        
        foreach (Transform child in programList)
            Destroy(child.gameObject);

        //filter out aura and auto programs
        player.auraProgram.name = "";
        player.autoProgram.name = "";
        foreach (Program p in programs)
        {
            float cd = 0;
            bool addedAuto = false;
            bool addedAura = false;
            foreach (Block b in p.blocks)
            {
                cd += b.cd;
                if (b.name == "Aura")
                {
                    player.auraProgram = p;
                    addedAura = true;
                }
                else if (b.name == "Auto")
                {
                    player.autoProgram = p;
                    addedAuto = true;
                }
            }
            if (addedAuto)
                player.autoTick = cd/2f;
        }

        //add other programs to results UI
        int index = 0;
        int bindIndex = 0;
        programList.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60);
        programList.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        foreach (Program p in programs)
        {
            //spawn symbol
            p.symbol = Instantiate(emptyImage, Vector2.zero, Quaternion.identity, programList);
            p.symbol.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            p.symbol.name = p.name;
            Vector2 totalPos = Vector2.zero;
            string spellName = "";
            float cd = 0;
            foreach (Block b in p.blocks)
            {
                spellName += b.nameTxt.text + " + ";
                cd += b.cd;
                Vector3 scale = new Vector3(b.symbol.transform.localScale.x*b.transform.localScale.x, b.symbol.transform.localScale.y*b.transform.localScale.y, 1);
                GameObject sym = Instantiate(b.symbol.gameObject, b.symbol.transform.position, Quaternion.identity, p.symbol.transform);
                sym.transform.localScale = scale;
                sym.GetComponent<Image>().color = fullSymbolColor;
                totalPos += sym.GetComponent<RectTransform>().anchoredPosition;
            }
            foreach (Transform child in p.symbol.transform)
            {
                child.GetComponent<RectTransform>().anchoredPosition -= totalPos/p.symbol.transform.childCount;
                child.transform.localScale *= 1.7f;
                child.GetComponent<RectTransform>().anchoredPosition *= 1.7f;
                Destroy(child.GetComponent<Symbol>());
                Destroy(child.GetComponent<BoxCollider2D>());
            }

            //create program list UI
            p.programList = Instantiate(spellListItem, Vector2.zero, Quaternion.identity, programList);
            p.symbol.transform.SetSiblingIndex(p.symbol.transform.parent.childCount - 3);
            p.programList.GetComponent<RectTransform>().anchoredPosition = new Vector2(-80, -index*260);
            programList.parent.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 260);

            p.programList.GetComponent<ProgramList>().hasKeybind = (p != player.autoProgram && p != player.auraProgram);
            p.programList.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = spellName.Substring(0, spellName.Length-3);
            string cdTxt = ((""+cd).Length == 1) ? cd + ".0s" : cd + "s"; 
            p.programList.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = cdTxt;
            if (p == player.autoProgram)
                p.programList.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "AUTO";
            else if (p == player.auraProgram)
                p.programList.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "AURA";
            
            //set program values
            p.name = spellName.Substring(0, spellName.Length-3);
            p.cdMax = cd;
            p.symbol.transform.SetSiblingIndex(p.symbol.transform.parent.childCount - 1);
            p.symbol.GetComponent<RectTransform>().anchoredPosition = new Vector2(-630, -index*260);

            if (index > 0)
                programList.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 130);
            index++;

            //default keybinds
            /*if (bindIndex < defaultBinds.Length && p != player.autoProgram && p != player.auraProgram)
            {
                p.keybind = defaultBinds[bindIndex];
                keybindIcons.GetChild(bindIndex).GetComponent<KeybindButton>().targetProgramList = p.programList.transform;
                TextMeshProUGUI txt = p.programList.transform.GetChild(6).GetComponent<TextMeshProUGUI>();
                if (bindIndex == 0)
                    txt.text = "Left Click";
                else if (bindIndex == 1)
                    txt.text = "Right Click";
                else if (bindIndex == 2)
                    txt.text = "Middle Click";
                bindIndex++;
            }*/
        }
        if (player.auraProgram.name != "")
            programs.Remove(player.auraProgram);
        if (player.autoProgram.name != "")
            programs.Remove(player.autoProgram);
        startButton.SetActive(true);

        //Keybinds
        /*for (int i = 0; i < Mathf.Min(programs.Count, 3); i++)
        {
            Transform symbol = Instantiate(programs[i].symbol, Vector2.zero, Quaternion.identity, keybindIcons.GetChild(i)).transform;
            symbol.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            symbol.localScale /= 1.7f;
            Block shape = programs[i].blocks.Find(b=>b.tag == "shape");
            keybindIcons.GetChild(i).GetChild(4).GetComponent<TextMeshProUGUI>().text = shape.name;
        }*/
        keybindIcons.GetChild(0).GetComponent<KeybindButton>().MakeActiveKeybind();
    }


    private void Update()
    {
        //TODO: show warning if player attaches passive to just effect/just shape & disable compile button

        if (spellsLocked)
        {
            //enable/disable confirm button depending on valid programs
            if (programs.Count == 0)
            {
                confirmButton.GetComponent<Button>().interactable = false;
            }
            else
            {
                //check validity of symbols
                bool readyToConfirm = true;
                foreach (Program p in programs)
                {
                    bool finished = true;
                    foreach (Block b in p.blocks)
                    {
                        if (b.symbol.adjSymbols < p.blocks.Count)
                        {
                            finished = false;
                            readyToConfirm = false;
                            break;
                        }
                    }
                    foreach (Block b in p.blocks)
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
        AudioManager.Instance.Play("Enter Game");
        if (!musicOn)
        {
            musicOn = true;
            //StartCoroutine(AudioManager.Instance.StartFade("Dreamer", 5, 0.1f));
            //AudioManager.Instance.Play("Dreamer");
        }
        foreach (Transform child in cdParent)
        {
            Destroy(child.gameObject);
        }
        cdParent.gameObject.SetActive(true);

        if (loadNextLevel)
        {
            StartCoroutine(GameManager.Instance.LoadNextLevel(GameObject.Find("End Elevator").GetComponent<Elevator>().nextArea));
            yield return new WaitForSeconds(1);
        }
        else
        {
            Fader.Instance.FadeIn(0.5f);
            yield return new WaitForSeconds(0.5f);
        }
        
        /*int index = 0;
        foreach (Program p in programs)
        {
            Block shape = p.blocks.Find(b=>b.tag == "shape");
            if (index < defaultBinds.Length && shape != null)
            {
                CreateProgramIcon(p, new Vector2(-800 + (170*index), -450), bindTxt[index], shape.nameTxt.text);
            }
            index++;
        }*/
        int index = 0;
        foreach (Transform child in keybindIcons)
        {
            KeybindButton button = child.GetComponent<KeybindButton>();
            if (button.targetProgram != null)
            {
                Block shape = button.targetProgram.blocks.Find(b=>b.tag == "shape");
                CreateProgramIcon(button.targetProgram, new Vector2(-800 + (170*index), -450), button.keybindStr, shape.name);
            }
            index++;
        }
        
        index = 0;
        if (player.auraProgram.name != "")
        {
            CreateProgramIcon(player.auraProgram, new Vector2(800, -450), "AURA", "");
            index++;
        }
        if (player.autoProgram.name != "")
        {
            Block shape = player.autoProgram.blocks.Find(b=>b.tag == "shape");
            if (shape != null)
                CreateProgramIcon(player.autoProgram, new Vector2(800 - (170*index), -450), "AUTO", shape.name);
        }

        startButton.SetActive(false);
        keybindUI.gameObject.SetActive(false);
        programUI.gameObject.SetActive(false);

        if (loadNextLevel)
            loadNextLevel = false;
        else
            Fader.Instance.FadeOut(0.5f);

        GameManager.Instance.pauseGame = false;
        GameManager.Instance.playerPaused = false;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.enabled = true;
        player.InitializeAura();
    }

    private void CreateProgramIcon(Program p, Vector2 pos, string type, string shape)
    {
        Transform cdIcon = Instantiate(cdIconPrefab, Vector2.zero, Quaternion.identity, cdParent).transform;
        cdIcon.GetComponent<RectTransform>().anchoredPosition = pos;
        cdIcon.GetChild(0).GetComponent<TextMeshProUGUI>().text = type;
        Transform symbol = Instantiate(p.symbol, Vector2.zero, Quaternion.identity, cdIcon).transform;
        symbol.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        symbol.localScale /= 1.7f;
        symbol.SetSiblingIndex(cdIcon.childCount - 2);
        p.fillTimer = cdIcon.GetChild(cdIcon.childCount-1).gameObject;
        if (type == "AURA")
            p.fillTimer.GetComponent<Image>().fillAmount = 1;
        cdIcon.GetChild(1).GetComponent<TextMeshProUGUI>().text = shape;
    }


    public List<Block> ChooseRandom(int n, string[] forbidden=null, string type="none", string category="none")
    {
        //TODO: add diff percents --- keep in 3 separate lists, but decrement pct of given list when chosen (e.g 40-40-20, then choose effect -> 50-25-25)
        if (forbidden == null)
            forbidden = new string[0];

        bool skipAura = false;
        bool skipAuto = false;
        foreach (Transform child in blockParent)
        {
            if (child.name == "Aura")
                skipAura = true;
            else if (child.name == "Auto")
                skipAuto = true;
        }
        List<Block> starting = new List<Block>();
        List<Block> chosen = new List<Block>();
        foreach (GameObject g in blocks)
        {
            if (!((skipAura && g.name == "Aura") || (skipAuto && g.name == "Auto")) && (type == "none" || type == g.GetComponent<Block>().type) && (category == "none" || category == g.GetComponent<Block>().tag) && !forbidden.Contains(g.name))
                starting.Add(g.GetComponent<Block>());
        }
        
        for (int i = 0; i < n; i++)
        {
            if (starting.Count > 0)
            {
                Block b = starting[Random.Range(0, starting.Count)];
                chosen.Add(b);
                starting.Remove(b);
            }
            else
            {
                Debug.LogError("No valid reward blocks!");
                break;
            }
        }

        return chosen;
    }


    public Color ColorFromType(string type)
    {
        if (type == "logic")
            return typeColors[0];
        else if (type == "memory")
            return typeColors[1];
        else if (type == "instinct")
            return typeColors[2];
        else if (type == "perception")
            return typeColors[3];
        else
            return new Color(1, 1, 1, 1);
    }

    public void ButtonClick()
    {
        AudioManager.Instance.Play("Button Click");
    }

    public void Info()
    {
        moreInfo = !moreInfo;
        foreach (Transform child in blockParent)
        {
            GameObject info = child.GetComponent<Block>().infoTxt.gameObject;
            info.SetActive(moreInfo);
        }
        string buttonTxt = (moreInfo) ? "Less Info" : "More Info";
        infoButton.text = buttonTxt;
    }
}



[System.Serializable]
public class Program
{
    public Program(List<Block> blocks_, KeyCode bind)
    {
        blocks = blocks_;
        name = blocks_[0].name;
        keybind = bind;
    }

    public Program(List<Block> blocks_)
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
    public GameObject programList;
}