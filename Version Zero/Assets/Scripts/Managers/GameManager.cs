using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("Bools")]
    [HideInInspector] public bool pauseGame;
    [HideInInspector] public bool playerPaused;
    private bool inTransition;
    public bool doubleSpeed;
    public bool scifiNames;
    private bool fullArea;
    public bool skipDialogue;
    
    [Header("Rooms")]
    [SerializeField] private Room[] rooms;
    private int roomNum = 1;
    [SerializeField] private TextMeshProUGUI roomText;
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private int[] bossRooms;
    private int bossIndex;
    
    [Header("Enemy Spawn")]
    public bool staticSpawn;
    public int numEnemies;
    [SerializeField] private string[] enemyPrefabs; //change to struct w/ spawn pct, weight, etc
    [SerializeField] private string[] enemyTypes;
    private string enemyType = "Logic";
    [SerializeField] private Transform nodeParent;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private List<int> waves = new List<int>();

    [Header("Terminals")]
    [HideInInspector] public int numTerminals;
    [SerializeField] private GameObject terminalBar;
    [HideInInspector] public Image bar;
    [HideInInspector] public Terminal currentTerminal;
    public KeyCode terminalBind;
    [SerializeField] private Transform terminalIcons;
    [SerializeField] private GameObject terminalIcon;
    [SerializeField] private Color unlockedColor;

    [Header("Dialogue")]
    [SerializeField] private string[] reyaDialogue;
    [SerializeField] private GameObject dialogue;
    [SerializeField] private GameObject[] portraits;

    [Header("Misc")]
    [SerializeField] private GameObject rewardPrefab;
    private Transform player;
    [SerializeField] private GameObject bossTxt;

    
    void Start()
    {
        if (SceneManager.GetActiveScene().name.Contains("Level"))
            fullArea = true;
        player = GameObject.Find("Player").transform;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Playtest Options")
            Destroy(gameObject);
        else
        {
            enemyParent = GameObject.Find("Enemies").transform;
            nodeParent = GameObject.Find("Spawn Nodes").transform;
            if (roomNum != 1 && !scene.name.Contains("Boss"))
            {
                enemyType = enemyTypes[Random.Range(0, enemyTypes.Length)];
                if (staticSpawn)
                    SetupEnemies(roomNum + Random.Range(1, 4));
                else
                {
                    if (roomNum == 2)
                        SetupWaves(3);
                    else
                        SetupWaves(roomNum + Random.Range(1, 4));
                }
            }
            else
            {
                numEnemies = Physics.OverlapSphere(Vector2.zero, 9999, LayerMask.GetMask("Enemy")).Length;
            }
            inTransition = false;

            if (scene.name.Contains("Level"))
            {
                foreach (Transform child in terminalIcons)
                    Destroy(child.gameObject);
                numTerminals = Physics.OverlapSphere(Vector2.zero, 9999, LayerMask.GetMask("Terminal")).Length;
                //StartCoroutine(SpawnInfiniteWaves());
                for (int i = 0; i < numTerminals; i++)
                {
                    GameObject icon = Instantiate(terminalIcon, Vector2.zero, terminalIcon.transform.rotation, terminalIcons);
                    //icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(920 - 150*terminalIcons.childCount, -460);
                    icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-822, 480 - 130*i);
                }
            }
        }

        if (scene.name == "Level 1")
        {
            StartCoroutine(IntroDialogue());
        }
    }

    private IEnumerator IntroDialogue()
    {
        dialogue.SetActive(true);
        TextMeshProUGUI txt = dialogue.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        if (!skipDialogue)
        {
            for (int i = 0; i < reyaDialogue.Length; i++)
            {
                txt.text = "";
                foreach (char c in reyaDialogue[i])
                {
                    if (c=='*')
                        yield return new WaitForSeconds(0.1f);
                    else
                    {
                        txt.text += c;
                        if (c=='.' || c==',')
                            yield return new WaitForSeconds(0.15f);
                        else if (c==' ')
                            yield return new WaitForSeconds(0.15f);
                        else
                            yield return new WaitForSeconds(0.08f);
                    }
                }
                if (i == reyaDialogue.Length-2)
                    Fader.Instance.FadeOut(12);
                else if (i == reyaDialogue.Length-1)
                {
                    player.GetComponent<PlayerMovement>().enabled = true;
                    pauseGame = false;
                }
                yield return new WaitForSeconds(2);
            }
            dialogue.SetActive(false);
        }
        else
        {
            dialogue.SetActive(false);
            Fader.Instance.FadeOut(0.5f);
            yield return new WaitForSeconds(0.5f);
            player.GetComponent<PlayerMovement>().enabled = true;
            pauseGame = false;
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            StartCoroutine(LoadNextRoom());
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            int killed = enemyParent.childCount;
            foreach (Transform child in enemyParent)
                Destroy(child.gameObject);
            UpdateEnemyNum(-killed);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("Spawning more enemies!");
            if (staticSpawn)
                SetupEnemies(roomNum + Random.Range(1, 4));
            else
                SetupWaves(roomNum*2 + Random.Range(1, 4), true);
        }
    }


    private void SetupEnemies(int n)
    {
        foreach (Transform child in enemyParent)
        {
            Destroy(child.gameObject);
        }
        numEnemies = n;
        
        while (n > 0)
        {
            n -= RandomEnemies(n);
        }
    }

    private void SetupWaves(int n, bool skip=false)
    {
        int maxPerWave = Mathf.Max(2, (int)Mathf.Round(n*3/5));
        waves.Clear();
        foreach (Transform child in enemyParent)
        {
            Destroy(child.gameObject);
        }
        if (!skip)
        {
            numEnemies = RandomEnemies(n, maxPerWave);
            n -= numEnemies;
        }
        while (n > 0)
        {
            int numToAdd = Random.Range(2, Mathf.Min(n, maxPerWave)+1);
            if (n - numToAdd == 1)
                numToAdd--;
            waves.Add(numToAdd);
            n -= numToAdd;
        }
        if (skip)
        {
            UpdateEnemyNum(0);
        }
    }

    private int RandomEnemies(int n, int max=5)
    {
        int numToAdd = Random.Range(2, Mathf.Min(n, max)+1);
        if (n - numToAdd == 1)
            numToAdd--;
        int nodeNum = Random.Range(0, nodeParent.childCount);
        for (int i = 0; i < numToAdd; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-2, 2), 1, Random.Range(-2, 2));
            int attempts = 0;
            while (Physics.OverlapSphere(nodeParent.GetChild(nodeNum).position + offset, 0.5f, LayerMask.GetMask("Enemy")).Length > 0)
            {
                offset = new Vector3(Random.Range(-2, 2), 1, Random.Range(-2, 2));
                attempts++;
                if (attempts == 10) //fail to find open spot
                    break;
            }
            if (attempts < 10)
            {
                string name = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)] + "_" + enemyType;
                GameObject prefab = Resources.Load<GameObject>("Prefabs/Enemies/" + name);
                if (prefab != null)
                    Instantiate(prefab, nodeParent.GetChild(nodeNum).position + offset, Quaternion.identity, enemyParent);
            }
        }
        return numToAdd;
    }

    public IEnumerator WaveEnemies(int n)
    {
        numEnemies += n;
        yield return new WaitForSeconds(1);
        for (int i = 0; i < n; i++)
        {
            string name = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)] + "_" + enemyType;
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Enemies/" + name);
            if (prefab != null)
            {
                int repeats = name.Contains("Fast") ? 2 : 1;
                numEnemies += repeats-1;
                for (int j = 0; j < repeats; j++)
                {
                    Vector3 offset = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)).normalized * Random.Range(5, 10) + new Vector3(0, 1, 0);
                    int attempts = 0;
                    while (Physics.OverlapSphere(player.position + offset, 0.5f).Length > 0)
                    {
                        offset = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)).normalized * Random.Range(5, 10) + new Vector3(0, 1, 0);
                        attempts++;
                        if (attempts == 10) //fail to find open spot
                        {
                            Debug.Log("NO OPEN SPOT :(");
                            numEnemies--;
                            break;
                        }
                    }
                    if (attempts < 10)
                    {
                        GameObject enemy = Instantiate(prefab, player.position + offset + new Vector3(0, 15, 0), Quaternion.identity, enemyParent);
                        enemy.GetComponent<Rigidbody>().velocity = new Vector3(0, -100, 0);
                    }
                    yield return new WaitForSeconds(0.5f);
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
        if (numEnemies <= 0)
            UpdateEnemyNum(0);
    }

    public IEnumerator SpawnInfiniteWaves()
    {
        int maxTerminals = numTerminals;
        yield return new WaitUntil(() => numTerminals != maxTerminals);
        while (true)
        {
            StartCoroutine(WaveEnemies(Random.Range(1, 3)));
            yield return new WaitForSeconds(20);
        }
    }


    public void UpdateEnemyNum(int n)
    {
        numEnemies += n;
        if (numEnemies <= 0 && !inTransition && !fullArea)
        {
            if (!staticSpawn && waves.Count > 0) //spawn more waves!
            {
                StartCoroutine(WaveEnemies(waves[0]));
                numEnemies = waves[0];
                waves.Remove(waves[0]);
            }
            else
               FinishLevel();
        }
    }

    private void FinishLevel()
    {
        float rot = Random.Range(0, 360);
        Vector3 rewardPos = player.position + Quaternion.Euler(0, rot, 0) * player.forward * 5;
        int attempts = 0;
        while (Physics.OverlapSphere(rewardPos, 1, terrainLayer).Length > 0)
        {
            rot = Random.Range(0, 360);
            rewardPos = player.position + Quaternion.Euler(0, rot, 0) * player.forward * 5;
            attempts++;
            if (attempts >= 40) //quit out after some max # of attempts
            {
                Debug.LogError("No valid location!");
                break;
            }
        } 
        GameObject reward = Instantiate(rewardPrefab, rewardPos + new Vector3(0, 20, 0), Quaternion.identity);
        reward.GetComponent<Rigidbody>().velocity = new Vector3(0, -100, 0);
        reward.GetComponent<Reward>().numOptions = 3;
        inTransition = true;
    }


    public IEnumerator UseTerminal()
    {
        playerPaused = true;
        bar = Instantiate(terminalBar, player.transform.position + new Vector3(0, 1.3f, 0), Quaternion.identity).transform.GetChild(1).GetComponent<Image>();
        float elapsed = 0;
        while (elapsed < 4)
        {
            if (bar == null)
                yield break;
            bar.fillAmount = elapsed/4f;
            yield return null;
            elapsed += Time.deltaTime;
        }
        currentTerminal.complete = true;
        Transform iconToChange = terminalIcons.GetChild(terminalIcons.childCount - numTerminals);
        iconToChange.GetComponent<CanvasGroup>().alpha = 0.5f;
        iconToChange.GetChild(0).gameObject.SetActive(true);
        Destroy(bar.transform.parent.gameObject);
        playerPaused = false;
        StartCoroutine(PlayMultipleDialogues(currentTerminal.dialogue));
        numTerminals--;
        if (numTerminals <= 0)
        {
            //Time.timeScale = 0.3f;
            //Time.timeScale = 1;
            //StartCoroutine(LoadNextRoom());
            GameObject.Find("Barrier").SetActive(false);
            GameObject.Find("Barrier Text").GetComponent<TextMeshProUGUI>().text = "Welcome, AUTH_USER!";
            GameObject.Find("Barrier Text").GetComponent<TextMeshProUGUI>().color = unlockedColor;
        }
    }

    public IEnumerator PlayMultipleDialogues(string[] lines)
    {
        foreach (string s in lines)
        {
            yield return PlayDialogue(s, 1f); 
        }
    }

    public IEnumerator PlayDialogue(string line, float waitTime=3f)
    {
        portraits[0].SetActive(line[0] != '~');
        portraits[1].SetActive(line[0] == '~');
        TextMeshProUGUI txt = dialogue.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        txt.text = "";
        dialogue.SetActive(true);
        foreach (char c in line)
        {
            if (c=='*')
                yield return new WaitForSeconds(0.15f);
            else if (c != '~')
            {
                txt.text += c;
                if (c=='.' || c==',')
                    yield return new WaitForSeconds(0.15f);
                else if (c==' ')
                    yield return new WaitForSeconds(0.08f);
                else
                    yield return new WaitForSeconds(0.04f);
            }
        }
        yield return new WaitForSeconds(waitTime);
        dialogue.SetActive(false);
        txt.text = "";
    }


    public IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(1);
        Fader.Instance.FadeIn(2);
        yield return new WaitForSeconds(2);
        int levelNum = int.Parse(SceneManager.GetActiveScene().name.Substring(6))+1;
        string areaStr = (levelNum < 10) ? "0" + levelNum : "" + levelNum;
        roomText.text = "Area_" + areaStr;
        SceneManager.LoadScene("Level " + levelNum);
    }

    public IEnumerator LoadNextRoom()
    {
        Fader.Instance.FadeIn(1);
        yield return new WaitForSeconds(1);
        roomNum++;
        string areaStr = (roomNum < 10) ? "0" + roomNum : "" + roomNum;
        roomText.text = "Area_" + areaStr;
        if (roomNum == bossRooms[bossIndex])
        {
            SceneManager.LoadScene("Boss " + (bossIndex+1));
            if (bossIndex < bossRooms.Length-1)
                bossIndex++;
            bossTxt.SetActive(true);
        }
        else
        {
            float totalWeight = 0;
            foreach (Room r in rooms)
            {
                if (!r.active)
                    totalWeight += r.weight;
            }
            float rand = Random.Range(0f, totalWeight);
            Room chosen = null;
            foreach (Room r in rooms)
            {
                if (!r.active)
                {
                    rand -= r.weight;
                    if (rand < 0 && chosen == null)
                    {
                        chosen = r;
                    }
                    else
                    {
                        r.weight += 1;
                    }
                }
                r.active = false;
            }
            if (chosen == null)
            {
                Debug.LogError("Could not find a scene to load!");
            }
            else
            {
                SceneManager.LoadScene(chosen.name);
                chosen.active = true;
                chosen.weight *= 0.5f;
            }
        }
    }
}


[System.Serializable]
public class Room
{
    public string name;
    public bool active;
    public float weight;
    //tags like encounter type, etc.
}