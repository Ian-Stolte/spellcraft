using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    void OnTriggerEnter(Collider hit)
    {
        if (hit.gameObject.name == "Player")
        {
            StartCoroutine(GameManager.Instance.LoadNextLevel());
            //TODO: set playerPaused to true but have Reya walk to middle of elevator
        }
    }
}
