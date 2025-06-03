using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup[] symbols;

    [SerializeField] private TextMeshProUGUI txt;
    [SerializeField] private string[] colorTags;

    [SerializeField] private CanvasGroup fader;


    void Start()
    {
        //TODO: set symbols based on save data (show if already won with that build)

        string build = "Instinct";
        if (ProgramManager.Instance != null)
            build = ProgramManager.Instance.buildpath;

        int index = 0;
        if (build == "logic")
        {
            index = 1;
            build = "Logic";
        }
        else if (build == "memory")
        {
            index = 2;
            build = "Memory";
        }
        else
            build = "Instinct";

        if (symbols[index].alpha < 1)
            StartCoroutine(FadeInSymbol(symbols[index]));

        txt.text = "You've reached the end of the demo with a " + colorTags[index] + build + "</color> build.";
    }

    private IEnumerator FadeInSymbol(CanvasGroup image)
    {
        yield return new WaitForSeconds(1);
        float elapsed = 0;
        while (elapsed < 1)
        {
            elapsed += Time.deltaTime;
            image.alpha = elapsed;
            yield return null;
        }
        image.alpha = 1;
        AudioManager.Instance.Play("Terminal Activate");
    }

    public void Quit()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void Continue()
    {
        StartCoroutine(ContinueCor());
    }

    private IEnumerator ContinueCor()
    {
        StartCoroutine(AudioManager.Instance.StartFade("Area 1", 1f, 0));
        float elapsed = 0;
        while (elapsed < 1)
        {
            elapsed += Time.deltaTime;
            fader.alpha = elapsed;
            yield return null;
        }
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Startup UI");
    }
}
