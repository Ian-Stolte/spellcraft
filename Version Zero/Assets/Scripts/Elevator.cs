using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public string nextArea;
    public bool final;

    /*void Start()
    {
        transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Elevator to: \n" + nextArea;
    }*/

    void OnTriggerEnter(Collider hit)
    {
        if (hit.gameObject.name == "Player")
        {
            if (!final)
            {
                StartCoroutine(LowerElevator(hit.transform));
                //TODO: set playerPaused to true but have Reya walk to middle of elevator
            }
            else
            {
                StartCoroutine(GameManager.Instance.FinalNextLevel(nextArea));
            }
        }
    }

    private IEnumerator LowerElevator(Transform player)
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(GameManager.Instance.LoadNextLevel(nextArea));
        for (float i = 0; i < 2.5f; i += 0.01f)
        {
            yield return new WaitForSeconds(0.01f);
            transform.position -= new Vector3(0, 0.02f * Mathf.Pow(i, 2), 0);
            player.transform.position -= new Vector3(0, 0.02f * Mathf.Pow(i, 2), 0);
        }
    }
}
