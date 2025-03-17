using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : Interactable
{
    [Header("Chop")]
    public GameObject takeText;
    public int maxChops;
    public int chops;
    public GameObject stump;
    public float fallForce;

    [Header("Growing")]
    private float sizeDelay;
    private float elapsedDelay;
    private Vector3 currentScale;
    private float goalScale;
    
    [Header("Pinecone")]
    [SerializeField] private GameObject pineconePrefab;
    private float pineconeTimer;
    [SerializeField] private float pineconePct;

    private float tickTime;

    /*  
        Pinecone drops:
            -- maybe change with weather (how windy)?
            -- maybe can also drop when chopped down
    */


    private void Start()
    {
        tickTime = DayNightCycle.Instance.tickTime;

        active = (maxChops > 0);
        sizeDelay = maxChops * tickTime * 144;
        if (maxChops == 0)
            sizeDelay = tickTime * 144;
        currentScale = transform.localScale;
        if (maxChops == 0)
            goalScale = 4;
        else if (maxChops == 1)
            goalScale = 2;
        else
            goalScale = Mathf.Pow((maxChops+1)/(1f*maxChops), 2);
        
        pineconeTimer = Random.Range(6, 12) * tickTime;
    }

    private void Update()
    {
        if (chops < maxChops || maxChops == 0)
        {
            //grow over time
            elapsedDelay += Time.deltaTime;
            if (elapsedDelay >= sizeDelay)
            {
                maxChops++;
                sizeDelay = maxChops * tickTime * 144; //grow +1 size every n days, where n is current size
                elapsedDelay = 0;
                currentScale = transform.localScale;
                if (maxChops == 1)
                    goalScale = 2;
                else
                    goalScale = Mathf.Pow((maxChops+1)/(1f*maxChops), 2);

                active = true;
            }
            float scale = Mathf.Lerp(1, goalScale, elapsedDelay/sizeDelay);
            float halfScale = (scale-1)/2f + 1;
            transform.localScale = new Vector3(currentScale.x * halfScale, currentScale.y * scale, currentScale.z * halfScale);

            //TODO: change so leaves don't get as distorted, move up instead


            //drop pinecones
            pineconeTimer -= Time.deltaTime;
            if (pineconeTimer <= 0)
            {
                float pct = pineconePct * maxChops;
                if (maxChops == 1)
                    pct = 0;
                if (Random.Range(0f, 1f) < pct)
                {
                    Vector3 offset = new Vector3(Random.Range(0f, 2f), Random.Range(-1.5f, 1.5f), Random.Range(0f, 2f));
                    GameObject pinecone = Instantiate(pineconePrefab, transform.position + offset, Quaternion.identity);
                    pinecone.GetComponent<Interactable>().text = takeText;
                }
                pineconeTimer = Random.Range(6, 12) * tickTime;
            }
        }
    }

    public override void Interact(PlayerInteract player)
    {
        chops++;
        if (chops <= maxChops)
            player.AddItem("sticks", 1);
            
        if (chops == maxChops)
        {
            GameObject s = Instantiate(stump, new Vector3(transform.position.x, player.transform.position.y-1f, transform.position.z), Quaternion.identity);
            s.transform.localScale = new Vector3(transform.localScale.x, s.transform.localScale.y, transform.localScale.z);
            transform.localScale = new Vector3(transform.localScale.x, 0.8f*transform.localScale.y, transform.localScale.z);
            transform.GetChild(0).localScale = new Vector3(transform.GetChild(0).localScale.x, 1.25f*transform.GetChild(0).localScale.y, transform.GetChild(0).localScale.z);
            
            GetComponent<Rigidbody>().isKinematic = false;
            Vector3 playerDir = transform.position - player.transform.position;
            Vector3 forceDir = new Vector3(playerDir.x, 0, playerDir.z);
            GetComponent<Rigidbody>().AddForce(fallForce * forceDir.normalized);
            
            StartCoroutine(WaitForFall(player.transform));
            text = takeText;
            keybind = KeyCode.F;
        }
        else if (chops > maxChops)
        {
            if (maxChops == 2)
                player.AddItem("wood", 1);
            else if (maxChops <= 4)
                player.AddItem("wood", 2);
            else if (maxChops <= 6)
                player.AddItem("wood", 3);
            else
                player.AddItem("wood", 4);

            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;
            active = false;
        }
    }


    private IEnumerator WaitForFall(Transform player)
    {
        active = false;
        yield return new WaitUntil(() => transform.position.y < player.position.y && player.GetComponent<PlayerMovement>().grounded);
        if (maxChops < 2)
            GetComponent<CapsuleCollider>().enabled = false;
        else
            active = true;
    }
}
