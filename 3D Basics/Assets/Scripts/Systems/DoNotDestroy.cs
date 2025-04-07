using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoNotDestroy : MonoBehaviour
{
    [SerializeField] string objTag;

    void Awake()
    {
        GameObject[] obj = GameObject.FindGameObjectsWithTag(objTag);
        if (obj.Length > 1)
        {
            if (name == "Player") //bring player to starting pos
            {
                foreach (GameObject g in obj)
                {
                    if (g != gameObject)
                    {
                        g.transform.position = transform.position;
                        g.transform.rotation = transform.rotation;
                    }
                }
            }
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Playtest Options")
        {
            if (name != "Options Manager")
                Destroy(gameObject);
        }
    }
}