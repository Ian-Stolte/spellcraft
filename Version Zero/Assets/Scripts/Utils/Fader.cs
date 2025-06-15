using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fader : MonoBehaviour
{
    public static Fader Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            if (SceneManager.GetActiveScene().name == "Playtest Options" || SceneManager.GetActiveScene().name == "Startup UI")
            {
                Destroy(Instance.gameObject);
                Instance = this;
            }
            else
                Destroy(gameObject);
        }
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
        if (scene.name != "Level 1")
        {
            if (scene.name.Contains("Level"))
                FadeOut(1.5f);
            else
               FadeOut(0.5f);
        }
    }

    public void FadeIn(float n, bool quad=false)
    {
        StartCoroutine(FadeInCor(n, quad));
    }

    private IEnumerator FadeInCor(float n, bool quad)
    {
        float elapsed = 0f;
        while (elapsed < n)
        {
            elapsed += Time.deltaTime;
            if (quad)
                GetComponent<CanvasGroup>().alpha = Mathf.Pow(elapsed/n, 2);
            else
                GetComponent<CanvasGroup>().alpha = elapsed/n;
            yield return null;
        }
        GetComponent<CanvasGroup>().alpha = 1;
    }

    public void FadeOut(float n)
    {
        StartCoroutine(FadeOutCor(n));
    }

    private IEnumerator FadeOutCor(float n)
    {
        float elapsed = n;
        while (elapsed > 0)
        {
            elapsed -= Time.deltaTime;
            GetComponent<CanvasGroup>().alpha = elapsed/n;
            yield return null;
        }
        GetComponent<CanvasGroup>().alpha = 0;
    }

    public void FadeInOut(float n1, float n2)
    {
        StartCoroutine(FadeInOutCor(n1, n2));
    }

    private IEnumerator FadeInOutCor(float n1, float n2)
    {
        float elapsed = 0f;
        while (elapsed < n1)
        {
            elapsed += Time.deltaTime;
            GetComponent<CanvasGroup>().alpha = elapsed/n1;
            yield return null;
        }

        elapsed = n2;
        while (elapsed > 0)
        {
            elapsed -= Time.deltaTime;
            GetComponent<CanvasGroup>().alpha = elapsed/n2;
            yield return null;
        }
        GetComponent<CanvasGroup>().alpha = 0;
    }
}
