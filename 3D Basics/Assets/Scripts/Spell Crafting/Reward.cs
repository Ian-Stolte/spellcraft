using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reward : MonoBehaviour
{
    public int numOptions;
    //public int numSelections;
    [SerializeField] private KeyCode interactKey;
    [SerializeField] private float interactDist;

    private Transform player;
    private Transform cam;

    void Start()
    {
        player = GameObject.Find("Player").transform;
        cam = GameObject.Find("Main Camera").transform;
        //StartCoroutine(SlowTime(0.3f));
        Time.timeScale = 0.3f;
    }

    private IEnumerator SlowTime(float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            Time.timeScale = Mathf.Lerp(0.8f, 0.3f, elapsed/duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void Update()
    {
        if (transform.position.y < 2)
        {
            Time.timeScale = 1;
            transform.position = new Vector3(transform.position.x, 1.5f, transform.position.z);
            GetComponent<Rigidbody>().velocity = Vector2.zero;
        }

        bool playerClose = Vector3.Distance(player.position, transform.position) < interactDist;
        transform.GetChild(0).gameObject.SetActive(playerClose);
        transform.GetChild(0).transform.forward = cam.forward;

        if (playerClose && Input.GetKeyDown(interactKey))
        {
            RewardManager.Instance.Reward(numOptions);
            Destroy(gameObject);
        }
    }
}