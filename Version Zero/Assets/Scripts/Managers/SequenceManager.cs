using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SequenceManager : MonoBehaviour
{
    public static SequenceManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public int runNum;

    
    public void LoadGame(bool newGame)
    {
        runNum = (newGame) ? 1 : 2;
        StartCoroutine(LoadGameCor());
    }

    private IEnumerator LoadGameCor()
    {
        Fader.Instance.FadeIn(2);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("Startup UI");
    }

    public void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}
