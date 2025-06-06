using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public bool scifiNames;
    private bool fullArea;
    public bool skipDialogue;
    [HideInInspector] public bool pauseGame;
    [HideInInspector] public bool playerPaused;
    
    [Header("Rooms")]
    private int levelNum = 1;
    [SerializeField] private TextMeshProUGUI areaText;
    [SerializeField] private LayerMask terrainLayer;
    
    [Header("Enemy Spawn")]
    public int numEnemies;
    [SerializeField] private List<string> enemyPrefabs; //TODO: change to struct w/ spawn pct, weight, etc
    [SerializeField] private string[] enemyTypes;
    private string enemyType = "Logic";
    [SerializeField] private Transform enemyParent;
    [SerializeField] private List<int> waves = new List<int>();

    private float spawnTimer;
    private bool spawningEnemies;
    private float minSpawn = 15;
    private float maxSpawn = 25;

    [Header("Terminals")]
    [SerializeField] private GameObject terminalBar;
    [HideInInspector] public Image bar;
    [HideInInspector] public Terminal currentTerminal;
    [HideInInspector] public int numTerminals;
    public KeyCode terminalBind;
    [SerializeField] private Transform terminalIcons;
    [SerializeField] private GameObject terminalIcon;

    [Header("Barrier")]
    [SerializeField] private Color unlockTextColor;
    [SerializeField] private Material barrierGreen;
    [SerializeField] private Material barrierUnlockBlue;

    [Header("Misc")]
    [SerializeField] private GameObject rewardPrefab;
    private Transform player;
    public GameObject bossTxt;
    [SerializeField] private GameObject loadingText;
    [SerializeField] private GameObject gameOver;

    
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
        if (scene.name == "Playtest Options" || scene.name == "End Screen" || scene.name == "Startup UI")
            Destroy(gameObject);
        else
        {
            enemyParent = GameObject.Find("Enemies").transform;
            numEnemies = Physics.OverlapSphere(Vector2.zero, 9999, LayerMask.GetMask("Enemy")).Length;

            foreach (Transform child in terminalIcons)
                Destroy(child.gameObject);
            
            //create an icon for each terminal in the level
            numTerminals = 0;
            foreach (GameObject g in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (g.layer == LayerMask.NameToLayer("Terminal") && g.hideFlags == HideFlags.None && g.scene.IsValid())
                    numTerminals++;
            }
            for (int i = 0; i < numTerminals; i++)
            {
                GameObject icon = Instantiate(terminalIcon, Vector2.zero, terminalIcon.transform.rotation, terminalIcons);
                icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-822, 450 - 130*i - areaText.preferredHeight);
            }

            //set spawn pct & enemies available by level (15, 25 by default)
            enemyType = enemyTypes[Random.Range(0, enemyTypes.Length)];
            if (scene.name == "Level 4")
            {
                enemyPrefabs.Add("Artillerist");
                minSpawn = 10;
                maxSpawn = 20;
            }
            else if (scene.name == "Level 5")
            {
                minSpawn = 8;
                maxSpawn = 18;
            }
        
            //replace enemies with chosen type
            if (scene.name != "Level 6")
            {
                List<GameObject> newEnemies = new List<GameObject>();
                foreach (Transform child in enemyParent)
                {
                    for (int i = 0; i < child.name.Length; i++)
                    {
                        if (child.name[i] == '_')
                        {
                            string name = child.name.Substring(0, i) + "_" + enemyType;
                            GameObject prefab = Resources.Load<GameObject>("Prefabs/Enemies/" + name);
                            
                            if (prefab != null && child.gameObject.activeSelf)
                            {
                                newEnemies.Add(Instantiate(prefab, child.position, child.rotation));
                            }
                            break;
                        }
                    }
                }
                foreach (Transform child in enemyParent)
                    Destroy(child.gameObject);

                foreach (GameObject g in newEnemies)
                    g.transform.parent = enemyParent;
            }
        }

        if (scene.name == "Level 1")
        {
            StartCoroutine(DialogueManager.Instance.IntroDialogue());
        }
        else if (scene.name == "Level 2" && SequenceManager.Instance != null)
        {
            Terminal terminal = GameObject.Find("Terminal").GetComponent<Terminal>();
            if (SequenceManager.Instance.runNum == 1)
            {
                foreach (GameObject g in terminal.hiddenRoom)
                    g.SetActive(!g.activeSelf);
            }
            else
            {
                terminal.complete = true;
                Destroy(terminalIcons.GetChild(1).gameObject);
            }
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            StartCoroutine(LoadNextLevel(GameObject.Find("End Elevator").GetComponent<Elevator>().nextArea));
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            int killed = enemyParent.childCount;
            foreach (Transform child in enemyParent)
                Destroy(child.gameObject);
            numEnemies -= killed;
        }

        if (spawningEnemies && !pauseGame)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer < 0)
            {
                StartCoroutine(WaveEnemies(1));
                spawnTimer = Random.Range(minSpawn, maxSpawn);
            }
        }
    }


    private void SetupWaves(int n)
    {
        int maxPerWave = Mathf.Max(2, (int)Mathf.Round(n*3/5));
        waves.Clear();
        foreach (Transform child in enemyParent)
        {
            Destroy(child.gameObject);
        }
        while (n > 0)
        {
            int numToAdd = Random.Range(2, Mathf.Min(n, maxPerWave)+1);
            if (n - numToAdd == 1)
                numToAdd--;
            waves.Add(numToAdd);
            n -= numToAdd;
        }
    }

    public IEnumerator WaveEnemies(int n, Vector3 setPos = default)
    {
        numEnemies += n;
        yield return new WaitForSeconds(1);
        for (int i = 0; i < n; i++)
        {
            string name = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)] + "_" + enemyType;
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Enemies/" + name);
            if (prefab != null)
            {
                int repeats = name.Contains("Swarm") ? 2 : 1;
                numEnemies += repeats-1;
                for (int j = 0; j < repeats; j++)
                {
                    if (setPos != Vector3.zero)
                    {
                        GameObject enemy = Instantiate(prefab, setPos + new Vector3(0, 15, 0), Quaternion.identity, enemyParent);
                        enemy.GetComponent<Rigidbody>().velocity = new Vector3(0, -100, 0);
                    }
                    else
                    {
                        float minDist = 5;
                        float maxDist = 10;
                        Vector3 offset = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)).normalized * Random.Range(5, 10) + new Vector3(0, 1, 0);
                        int attempts = 0;
                        //while pos overlaps something or doesn't touch the ground, regenerate
                        float checkSize = (prefab.name.Contains("Tank")) ? 1.5f : 0.5f;
                        while (Physics.OverlapSphere(player.position + offset, checkSize).Length > 0 || Physics.OverlapSphere(player.position + offset + new Vector3(0, -1.5f, 0), 1f, LayerMask.GetMask("Ground")).Length == 0)
                        {
                            offset = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)).normalized * Random.Range(minDist, maxDist) + new Vector3(0, 1, 0);
                            attempts++;
                            if (attempts == 10) //fail to find open spot
                            {
                                minDist++;
                                maxDist++;
                                attempts = 0;
                                if (maxDist > 20)
                                {
                                    Debug.Log("NO OPEN SPOT :(");
                                    numEnemies--;
                                    break;
                                }
                            }
                        }
                        if (maxDist < 20)
                        {
                            GameObject enemy = Instantiate(prefab, player.position + offset + new Vector3(0, 15, 0), Quaternion.identity, enemyParent);
                            enemy.GetComponent<Rigidbody>().velocity = new Vector3(0, -100, 0);
                        }
                    }
                    yield return new WaitForSeconds(0.5f);
                }
            }
            yield return new WaitForSeconds(0.5f);
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
    }



    public IEnumerator UseTerminal()
    {
        playerPaused = true;
        bar = Instantiate(terminalBar, player.transform.position + new Vector3(0, 1.3f, 0), Quaternion.identity).transform.GetChild(1).GetComponent<Image>();
        AudioManager.Instance.Play("Terminal Charge");
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
        Destroy(bar.transform.parent.gameObject);
        playerPaused = false;
        AudioManager.Instance.Play("Terminal Activate");
        AudioManager.Instance.Stop("Terminal Charge");
        if (currentTerminal.order == 0)
            StartCoroutine(DialogueManager.Instance.PlayMultipleDialogues(currentTerminal.dialogue));
        else
            DialogueManager.Instance.PlayOrderedTerminal();

        FinishTerminalIcon();
        numTerminals--;
        
        //disable barrier &/or show hidden room
        if (currentTerminal.barrier != null)
            UnlockBarrier(currentTerminal.barrier);
        foreach (GameObject g in currentTerminal.hiddenRoom)
            g.SetActive(!g.activeSelf);

        if (SceneManager.GetActiveScene().name != "Level 1" && SceneManager.GetActiveScene().name != "Level 2")
            spawningEnemies = true;
    }

    public void FinishTerminalIcon()
    {
        Transform iconToChange = terminalIcons.GetChild(terminalIcons.childCount - numTerminals);
        iconToChange.GetComponent<CanvasGroup>().alpha = 0.5f;
        iconToChange.GetChild(0).gameObject.SetActive(true);
    }

    public void UnlockBarrier(Transform barrier)
    {
        int numLocks = 0;
        foreach (Transform child in barrier.GetChild(0))
        {
            if (child.GetComponent<MeshRenderer>().material.name.Contains("Red"))
                numLocks++;
        }
        barrier.GetChild(0).GetChild(barrier.GetChild(0).childCount - numLocks).GetComponent<MeshRenderer>().material = barrierUnlockBlue;

        if (numLocks <= 1)
        {
            barrier.GetChild(0).gameObject.SetActive(false);
            barrier.GetChild(1).gameObject.SetActive(false);
            barrier.GetChild(2).GetComponent<MeshRenderer>().material = barrierGreen;
            barrier.GetChild(3).GetComponent<MeshRenderer>().material = barrierGreen;
            TextMeshProUGUI txt = barrier.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>();
            txt.text = "Welcome, AUTH_USER!";
            txt.color = unlockTextColor;
        }
    }


    public IEnumerator LoadNextLevel(string nextArea)
    {
        spawningEnemies = false;
        AudioManager.Instance.Play("Elevator Down");
        foreach (Transform child in enemyParent)
            Destroy(child.gameObject);
        yield return new WaitForSeconds(0.5f);
        Fader.Instance.FadeIn(1.2f, true);
        yield return new WaitForSeconds(1.2f);
        yield return new WaitForSeconds(1.5f);
        loadingText.GetComponent<TextMeshProUGUI>().text = "Now approaching: \n" + nextArea;
        loadingText.SetActive(true);
        Color c = loadingText.GetComponent<TextMeshProUGUI>().color;
        loadingText.GetComponent<TextMeshProUGUI>().color = new Color(c.r, c.g, c.b, 1);
        yield return new WaitForSeconds(2f);

        float elapsed = 1;
        StartCoroutine(ElevatorSounds());
        while (elapsed > 0)
        {
            elapsed -= Time.deltaTime;
            yield return null;
            loadingText.GetComponent<TextMeshProUGUI>().color = new Color(c.r, c.g, c.b, elapsed);
        }
        loadingText.SetActive(false);
        int levelNum = int.Parse(SceneManager.GetActiveScene().name.Substring(6))+1;
        areaText.text = nextArea;
        if (levelNum < 7)
            SceneManager.LoadScene("Level " + levelNum);
        else
        {
            Destroy(player.gameObject);
            SceneManager.LoadScene("End Screen");
        }
    }

    private IEnumerator ElevatorSounds()
    {
        yield return new WaitForSeconds(0.3f);
        StartCoroutine(AudioManager.Instance.StartFade("Elevator Down", 0, 0.5f));
        yield return new WaitForSeconds(0.5f);
        AudioManager.Instance.Play("Elevator Stop");
        AudioManager.Instance.Stop("Elevator Down");
    }


    public IEnumerator GameOver()
    {
        player.GetComponent<PlayerPrograms>().enabled = false;
        pauseGame = true;
        StartCoroutine(AudioManager.Instance.FadeOutAll(0));
        AudioManager.Instance.Play("Static");
        AudioManager.Instance.Play("Game Over");
        Camera.main.GetComponent<GlitchManager>().ShowGlitch(2, 1);

        yield return new WaitForSeconds(2);
        AudioManager.Instance.Stop("Static");
        gameOver.SetActive(true);
        yield return new WaitForSeconds(1);

        TMPro.TextMeshProUGUI txt = gameOver.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
        for (int i = 0; i < 4; i++)
        {
            txt.text = "_";
            yield return new WaitForSeconds(0.5f);
            txt.text = "";
            yield return new WaitForSeconds(0.3f);
        }
        yield return new WaitForSeconds(1);
        string message = "Program Terminated";
        foreach (char c in message)
        {
            txt.text += c;
            if (c == ' ')
                yield return new WaitForSeconds(0.1f);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1.5f);
        StartCoroutine(AudioManager.Instance.StartFade("Game Over", 2, 0));
        for (float i = 0; i < 1; i += 0.01f)
        {
            gameOver.transform.GetChild(1).GetComponent<CanvasGroup>().alpha = i;
            gameOver.transform.GetChild(2).GetComponent<CanvasGroup>().alpha = i;
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void Reset()
    {
        StartCoroutine(ResetCor());
    }

    private IEnumerator ResetCor()
    {
        Fader.Instance.FadeIn(1.5f);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("Startup UI");
    }

    public void Quit()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
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