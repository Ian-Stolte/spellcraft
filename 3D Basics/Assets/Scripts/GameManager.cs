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
    [HideInInspector] public bool smallRooms;
    [SerializeField] private Room[] rooms;
    private int roomNum = 1;
    [SerializeField] private TextMeshProUGUI roomText;
    [SerializeField] private LayerMask terrainLayer;

    [Header("Enemy Spawn")]
    [HideInInspector] public bool staticSpawn;
    [SerializeField] private int numEnemies;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform nodeParent;
    [SerializeField] private Transform enemyParent;

    [Header("Misc")]
    [SerializeField] private GameObject rewardPrefab;

    private bool inTransition;

    private Transform player;

    
    void Start()
    {
        if (SceneManager.GetActiveScene().name.Contains("s_"))
            smallRooms = true;
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
        if (roomNum != 1)
            SpawnEnemies(roomNum + Random.Range(1, 4));
        else
            numEnemies = Physics.OverlapSphere(Vector2.zero, 9999, LayerMask.GetMask("Enemy")).Length;
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
            foreach (Transform child in GameObject.Find("Enemies").transform)
                Destroy(child.gameObject);
            FinishLevel();
        }
    }


    private void SpawnEnemies(int n)
    {
        Transform nodeParent = GameObject.Find("Spawn Nodes").transform;
        Transform enemyParent = GameObject.Find("Enemies").transform;
        foreach (Transform child in enemyParent)
        {
            Destroy(child.gameObject);
        }
        numEnemies = n;
        
        while (n > 0)
        {
            int numToAdd = Random.Range(2, Mathf.Min(n, 5)+1);
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
            n -= numToAdd;
        }
    }


    public void UpdateEnemyNum(int n)
    {
        numEnemies += n;
        if (numEnemies <= 0 && !inTransition) //level cleared
        {
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
            string roomToLoad= (smallRooms) ? "s_ " + chosen.name : chosen.name;
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