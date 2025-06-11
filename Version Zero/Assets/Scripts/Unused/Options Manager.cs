using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsManager : MonoBehaviour
{
    [SerializeField] private float selectSpeed;

    [SerializeField] private GameObject[] options;
    private int[] choices;

    //[SerializeField] private string sceneToLoad;


    public void SelectOption(GameObject chosen)
    {
        RectTransform selection  = chosen.transform.parent.GetChild(1).GetComponent<RectTransform>();
        StartCoroutine(MoveSelect(selection, chosen.GetComponent<RectTransform>()));
    }

    private IEnumerator MoveSelect(RectTransform obj, RectTransform target)
    {
        Vector2 origPos = obj.anchoredPosition;
        float elapsed = 0;
        while (elapsed < selectSpeed)
        {
            obj.anchoredPosition = Vector2.Lerp(origPos, target.anchoredPosition, Mathf.Pow(elapsed/selectSpeed, 2));
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.anchoredPosition = target.anchoredPosition;
    }


    public void LoadGame()
    {
        choices = new int[options.Length];

        for (int i = 0; i < options.Length; i++)
        {
            float selectX = options[i].transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition.x;
            float minDist = 9999;
            for (int j = 2; j < options[i].transform.childCount; j++)
            {
                float dist = Mathf.Abs(options[i].transform.GetChild(j).GetComponent<RectTransform>().anchoredPosition.x - selectX);
                if (dist < minDist)
                {
                    minDist = dist;
                    choices[i] = j-2;
                }
            }
        }
        StartCoroutine(LoadGameCor());
    }
    
    private IEnumerator LoadGameCor()
    {
        Fader.Instance.FadeIn(2);
        yield return new WaitForSeconds(2);
        DontDestroyOnLoad(gameObject);
        string sceneToLoad = (choices[0] == 1) ? "Level 1" : "Startup UI";
        SceneManager.LoadScene(sceneToLoad);
        SequenceManager.Instance.runNum++;
        SceneManager.sceneLoaded += FinishSetup;
    }

    private void FinishSetup(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Level 1")
        {
            ProgramManager.Instance.StartingHand();
            GameManager.Instance.skipDialogue = (choices[0] == 1);

            SceneManager.sceneLoaded -= FinishSetup;
            Destroy(gameObject);
        }
    }
}
