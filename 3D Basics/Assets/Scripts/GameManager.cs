using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private int numEnemies;
    [SerializeField] private GameObject rewardPrefab;

    private bool inTransition;

    private Transform player;

    void Start()
    {
        player = GameObject.Find("Player").transform;
        numEnemies = Physics.OverlapSphere(Vector2.zero, 9999, LayerMask.GetMask("Enemy")).Length;
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
}
