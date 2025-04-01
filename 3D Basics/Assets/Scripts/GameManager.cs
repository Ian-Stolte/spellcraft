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

    [SerializeField] private Room[] rooms;
    private int roomNum = 1;
    [SerializeField] private TextMeshProUGUI roomText;

    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private int numEnemies;
    [SerializeField] private GameObject rewardPrefab;

    private bool inTransition;

    private Transform player;

    
    void Start()
    {
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
        numEnemies = Physics.OverlapSphere(Vector2.zero, 9999, LayerMask.GetMask("Enemy")).Length;
        inTransition = false;
    }


    public void UpdateEnemyNum(int n)
    {
        numEnemies += n;
        if (numEnemies <= 0 && !inTransition) //level cleared
        {
            float rot = Random.Range(0, 360);
            Vector3 rewardPos = player.position + Quaternion.Euler(0, rot, 0) * player.forward * 5;
            while (Physics.OverlapSphere(rewardPos, 1, terrainLayer).Length > 0)
            {
                rot += 10;
                rewardPos = player.position + Quaternion.Euler(0, rot, 0) * player.forward * 5;
            } 
            GameObject reward = Instantiate(rewardPrefab, rewardPos + new Vector3(0, 20, 0), Quaternion.identity);
            reward.GetComponent<Rigidbody>().velocity = new Vector3(0, -100, 0);
            reward.GetComponent<Reward>().numOptions = 3;
            inTransition = true;
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            StartCoroutine(LoadNextRoom());
        }
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
            SceneManager.LoadScene(chosen.name);
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