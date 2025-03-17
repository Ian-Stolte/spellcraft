using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ResourceSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] textObjs;

    [Header("Flowers")]
    [SerializeField] private Transform flowerParent;
    [SerializeField] private float flowerRadius;
    [SerializeField] private GameObject flowerPrefab;
    [SerializeField] private float flowerMinDelay;
    [SerializeField] private float flowerMaxDelay;
    private float flowerTimer;

    [Header("Ore")]
    [SerializeField] private Ore[] ores;
    [SerializeField] private float oreMinDelay;
    [SerializeField] private float oreMaxDelay;
    private float oreTimer;
    private GameObject[] oreSpawns;

    [Header("Dogs")]
    [SerializeField] private GameObject dogPrefab;
    [SerializeField] private float dogMinDelay;
    [SerializeField] private float dogMaxDelay;
    [SerializeField] private float dogSpawnPct;
    [SerializeField] private float dogSpawnDist;
    [SerializeField] private float maxDogs;
    private float dogTimer;

    private Transform player;
    private Terrain terrain;


    void Start()
    {
        flowerTimer = Random.Range(flowerMinDelay, flowerMaxDelay);
        oreTimer = Random.Range(oreMinDelay, oreMaxDelay);
        dogTimer = Random.Range(dogMinDelay, dogMaxDelay);
        oreSpawns = GameObject.FindGameObjectsWithTag("OreSpawn");
        player = GameObject.Find("Player").transform;
        terrain = GameObject.Find("Terrain").GetComponent<Terrain>();
    }

    void Update()
    {
        //FLOWERS
        flowerTimer -= Time.deltaTime / DayNightCycle.Instance.tickTime;
        if (flowerTimer <= 0)
        {
            Vector3 randomPos = new Vector3(Random.Range(-flowerRadius, flowerRadius), 0, Random.Range(-flowerRadius, flowerRadius));
            //check where player is looking & don't spawn right in front of them (either wait or regenerate)
            GameObject flower = Instantiate(flowerPrefab, flowerParent.position + randomPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), flowerParent);
            flower.GetComponent<Flower>().text = textObjs[0];
            //set any vars needed --- rarity?
            flowerTimer = Random.Range(flowerMinDelay, flowerMaxDelay);
        }


        //ORE
        oreTimer -= Time.deltaTime / DayNightCycle.Instance.tickTime;
        if (oreTimer <= 0)
        {
            //randomly sort oreSpawns
            for (int i = oreSpawns.Length - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (oreSpawns[i], oreSpawns[randomIndex]) = (oreSpawns[randomIndex], oreSpawns[i]);
            }

            Transform chosenSpawn = null;
            for (int i = 0; i < oreSpawns.Length; i++)
            {
                if (oreSpawns[i].transform.childCount <= 1)
                {
                    chosenSpawn = oreSpawns[i].transform;
                    break;
                }
            }
            if (chosenSpawn == null)
            {
                Debug.Log("No free ore spawn point.");
            }
            else
            {
                float totalPct = 0;
                foreach (Ore o in ores)
                {
                    totalPct += o.spawnPct;
                }
                float rarity = Random.Range(0f, totalPct);
                foreach (Ore o in ores)
                {
                    rarity -= o.spawnPct;
                    if (rarity <= 0)
                    {
                        GameObject ore = Instantiate(o.prefab, chosenSpawn.position, Quaternion.identity, chosenSpawn); //offset pos, random rot?
                        ore.transform.GetChild(0).GetComponent<Interactable>().text = textObjs[0];
                        break;
                    }
                }
            }
            oreTimer = Random.Range(oreMinDelay, oreMaxDelay);
        }


        //DOGS
        dogTimer -= Time.deltaTime / DayNightCycle.Instance.tickTime;
        if (dogTimer <= 0)
        {
            int numDogs = GameObject.FindGameObjectsWithTag("Dog").Length;
            float random = Random.Range(0f, 1f);
            float spawnPct = dogSpawnPct - ((dogSpawnPct/maxDogs)*numDogs);
            if (random < spawnPct)
            {
                Vector3 offset = Vector3.zero;
                bool lineOfSight = true;
                for (int i = 0; i < 10; i++)
                {
                    offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * dogSpawnDist;
                    offset += new Vector3(0, terrain.SampleHeight(player.position + offset) + 1, 0);
                    lineOfSight = !Physics.Raycast(player.position + new Vector3(0, 2, 0), offset.normalized, dogSpawnDist, LayerMask.GetMask("Ground")) && Vector3.Angle(-offset.normalized, GameObject.Find("Main Camera").transform.forward) < 80;
                    if (!lineOfSight)
                        break;
                }
                GameObject dog = Instantiate(dogPrefab, player.position + offset, Quaternion.identity);
            }
            dogTimer = Random.Range(dogMinDelay, dogMaxDelay);
        }
    }



    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Handles.color = Color.blue;
        Handles.DrawWireArc(flowerParent.position, Vector3.up, Vector3.forward, 360, flowerRadius);
    }
    #endif
}


[System.Serializable]
public class Ore
{
    public GameObject prefab;
    public float spawnPct;
}