using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEnd : MonoBehaviour
{
    void OnTriggerEnter(Collider hit)
    {
        if (hit.gameObject.name == "Player")
        {
            StartCoroutine(GameManager.Instance.LoadNextLevel());
        }
    }
}
