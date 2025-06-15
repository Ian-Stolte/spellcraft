using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AccessPoint : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey;
    [SerializeField] private float interactDist;
    private bool used;

    public Transform barrier;
    [TextArea(3, 5)][SerializeField] private string[] dialogue;

    private Transform player;
    private Transform cam;


    void Start()
    {
        player = GameObject.Find("Player").transform;
        cam = GameObject.Find("Main Camera").transform;
    }

    void Update()
    {
        bool playerClose = Vector3.Distance(player.position, transform.position) < interactDist;
        transform.GetChild(0).gameObject.SetActive(playerClose);
        transform.GetChild(0).transform.forward = cam.forward;

        if (playerClose && Input.GetKeyDown(interactKey))
        {
            if (!used)
            {
                used = true;
                if (SceneManager.GetActiveScene().name == "Level 2")
                    StartCoroutine(DialogueManager.Instance.FirstAccessPt(dialogue));
                else
                {
                    RewardManager.Instance.Reward(3);
                    if (dialogue.Length > 0)
                        StartCoroutine(DelayedDialogue());
                }
            }
            else
            {
                ProgramManager.Instance.Reforge();
            }
        }
    }

    private IEnumerator DelayedDialogue()
    {
        DialogueManager.Instance.StopCoroutines();
        yield return new WaitForSeconds(1);
        DialogueManager.Instance.PlayMultiple(dialogue);
    }
}