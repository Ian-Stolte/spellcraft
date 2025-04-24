using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsManager : MonoBehaviour
{
    [SerializeField] private float selectSpeed;

    [SerializeField] private GameObject[] options;
    private int[] choices;


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
        Fader.Instance.FadeIn(1);
        yield return new WaitForSeconds(1);
        DontDestroyOnLoad(gameObject);
        string sceneToLoad = (choices[1] == 0) ? "Room 1" : "M_ Room 1";
        //SceneManager.LoadScene("Intro Dialogue");
        SceneManager.LoadScene(sceneToLoad);
        SceneManager.sceneLoaded += FinishSetup;
    }

    private void FinishSetup(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Room 1")
        {
            Transform player = GameObject.Find("Player").transform;
            player.GetComponent<PlayerMovement>().canDie = (choices[0] == 0);

            GameManager manager = GameObject.Find("Game Manager").GetComponent<GameManager>();
            /*if (choices[1] == 0)
                manager.roomSize = GameManager.RoomSize.SMALL;
            else if (choices[1] == 1)
                manager.roomSize = GameManager.RoomSize.MEDIUM;
            else if (choices[1] == 2)
                manager.roomSize = GameManager.RoomSize.BOTH;*/
            manager.firstRun = (choices[1] == 0);
            manager.staticSpawn = choices[2] == 1;
            SpellManager.Instance.StartingHand();

            SceneManager.sceneLoaded -= FinishSetup;
            Destroy(gameObject);
        }
    }
}
