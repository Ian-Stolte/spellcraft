using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [Header("Rooms")]
    [SerializeField] private Room[] rooms;
    private int roomNum = 1;
    [SerializeField] private TextMeshProUGUI roomText;
    [SerializeField] private LayerMask terrainLayer;
    public enum RoomSize
    {
        SMALL,
        MEDIUM,
        BOTH
    }
    public RoomSize roomSize;
    
    [Header("Enemy Spawn")]
    public bool staticSpawn;
    [SerializeField] private int numEnemies;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform nodeParent;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private List<int> waves = new List<int>();

    [Header("Misc")]
    [SerializeField] private GameObject rewardPrefab;

    private bool inTransition;

    private Transform player;

    
    void Start()
    {
        if (SceneManager.GetActiveScene().name.Contains("M_"))
            roomSize = RoomSize.MEDIUM;
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
        enemyParent = GameObject.Find("Enemies").transform;
        nodeParent = GameObject.Find("Spawn Nodes").transform;
        if (roomNum != 1)
        {
            if (staticSpawn)
                SetupEnemies(roomNum + Random.Range(1, 4));
            else
            {
                if (roomNum == 2)
                    SetupWaves(3);
                else
                    SetupWaves(roomNum*2 + Random.Range(1, 4));
            }
        }
        else
        {
            numEnemies = Physics.OverlapSphere(Vector2.zero, 9999, LayerMask.GetMask("Enemy")).Length;
        }
        inTransition = false;
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
                Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], nodeParent.GetChild(nodeNum).position + offset, Quaternion.identity, enemyParent);
        }
        return numToAdd;
    }

    private IEnumerator WaveEnemies(int n)
    {
        yield return new WaitForSeconds(1);
        for (int i = 0; i < n; i++)
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
                GameObject enemy = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], player.position + offset + new Vector3(0, 15, 0), Quaternion.identity, enemyParent);
                enemy.GetComponent<Rigidbody>().velocity = new Vector3(0, -100, 0);
            }
            yield return new WaitForSeconds(1);
        }
        if (numEnemies <= 0)
            UpdateEnemyNum(0);
    }


    public void UpdateEnemyNum(int n)
    {
        numEnemies += n;
        if (numEnemies <= 0 && !inTransition)
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


    public IEnumerator LoadNextRoom()
    {
        Fader.Instance.FadeIn(1);
        yield return new WaitForSeconds(1);
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
            roomNum++;
            string areaStr = (roomNum < 10) ? "0" + roomNum : "" + roomNum;
            roomText.text = "Area_" + areaStr;
            bool medium = roomSize == RoomSize.MEDIUM;
            if (roomSize == RoomSize.BOTH)
            {
                if (Random.Range(0f, 1f) < 0.5f)
                    medium = true;
            }
            string roomToLoad = (medium) ?  "M_ " + chosen.name : chosen.name;
            SceneManager.LoadScene(roomToLoad);
            chosen.active = true;
            chosen.weight *= 0.5f;
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