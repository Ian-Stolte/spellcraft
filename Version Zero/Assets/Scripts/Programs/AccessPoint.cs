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

        if (playerClose && Input.GetKeyDown(interactKey) && !used)
        {
            used = true;
            if (SceneManager.GetActiveScene().name == "Level 2")
                StartCoroutine(GameManager.Instance.FirstAccessPt());
            else
                ProgramManager.Instance.Reforge();

            //TODO: play access pt dialogue?
        }
    }
}