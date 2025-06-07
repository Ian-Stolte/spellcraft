using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terminal : MonoBehaviour
{
    [SerializeField] private float activateDist;
    [SerializeField] private GameObject activateTxt;
    [HideInInspector] public bool complete;
    
    public Transform barrier;
    public GameObject[] hiddenRoom;

    [TextArea(3, 5)] public string[] dialogue;
    public int order;

    private Transform player;
    private Transform cam;


    void Start()
    {
        player = GameObject.Find("Player").transform;
        cam = GameObject.Find("Main Camera").transform;
        if (order != 0)
            DialogueManager.Instance.terminalDialogue[order] = dialogue;
    }

    void Update()
    {
        if (Vector3.Distance(player.position, transform.position) < activateDist && !complete && !GameManager.Instance.playerPaused)
        {
            activateTxt.SetActive(true);
            if (Input.GetKeyDown(GameManager.Instance.terminalBind))
            {
                GameManager.Instance.currentTerminal = this;
                StartCoroutine(GameManager.Instance.UseTerminal());
            }
        }
        else
        {
            activateTxt.SetActive(false);
        }

        activateTxt.transform.forward = cam.forward;
    }
}
