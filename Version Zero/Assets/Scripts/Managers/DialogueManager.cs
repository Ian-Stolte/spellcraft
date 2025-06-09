using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("Dialogue")]
    [TextArea(3, 5)] [SerializeField] private string[] introDialogue;
    [SerializeField] private GameObject dialogue;
    [SerializeField] private GameObject[] portraits;
    //[SerializeField] private Sprite[] reyaExpressions;

    [Header("Terminals")]
    private int terminalNum;
    [HideInInspector] public string[][] terminalDialogue = new string[5][];

    [Header("First Access Pt")]
    [TextArea(3, 5)] [SerializeField] private string[] firstAccessPt;
    [SerializeField] private string[] firstEnemy;
    [SerializeField] private Transform buildSelect;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI completeTxt;

    [Header("Misc")]
    [SerializeField] private TextMeshProUGUI areaIntroText;
    [TextArea(3, 5)] [SerializeField] private string[] gardenerDialogue;

    
    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        terminalNum = 0;
        terminalDialogue = new string[5][];
    }
    
    public void PlayOrderedTerminal()
    {
        terminalNum++;
        StartCoroutine(PlayMultipleDialogues(terminalDialogue[terminalNum]));
    }

    public IEnumerator PlayMultipleDialogues(string[] lines)
    {
        foreach (string s in lines)
        {
            yield return PlayDialogue(s, 1f); 
        }
    }

    public IEnumerator PlayDialogue(string line, float waitTime=3f)
    {
        //set up portraits
        line = ShowPortraits(line);

        //type out dialogue
        TextMeshProUGUI txt = dialogue.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        txt.text = "";
        dialogue.SetActive(true);
        bool addingHTML = false;
        string html = "";
        foreach (char c in line)
        {
            if (c=='<')
            {
                addingHTML = true;
                html = "<";
            }
            else if (c=='>')
            {
                addingHTML = false;
                txt.text += html+">";
            }
            else if (addingHTML)
                html += c;
            else if (c=='*')
                yield return new WaitForSeconds(0.15f);
            else if (c != '~')
            {
                txt.text += c;
                if (c=='.' || c==',')
                    yield return new WaitForSeconds(0.15f);
                else if (c==' ')
                    yield return new WaitForSeconds(0.08f);
                else
                    yield return new WaitForSeconds(0.04f);
            }
        }
        if (line[line.Length-1] == '—')
            waitTime *= 0.5f;
        yield return new WaitForSeconds(waitTime);
        dialogue.SetActive(false);
        txt.text = "";
    }

    private string ShowPortraits(string line)
    {
        portraits[0].SetActive(line[0] != '~' && line[0] != '!');
        portraits[1].SetActive(line[0] == '~');
        portraits[2].SetActive(line[0] == '!');
        if (line[0] == '~' || line[0] == '!')
            line = line.Substring(1);
        /*if (line[0] == '[')
        {
            string portrait = line.Split("]")[0].Substring(1);
            line = line.Split("]")[1].Trim();
            Sprite newPortrait = reyaExpressions.FirstOrDefault(s => s.name == portrait);
            if (newPortrait != null)
                portraits[0].transform.GetChild(0).GetComponent<Image>().sprite = newPortrait;
            else
            {
                Debug.LogWarning("No portrait found for: " + portrait);
                portraits[0].transform.GetChild(0).GetComponent<Image>().sprite = reyaExpressions[0];
            }
        }*/
        return line;
    }



    //////////////////////////////////
    //////// SPECIFIC CUTSCENES //////
    //////////////////////////////////

    public IEnumerator IntroDialogue()
    {
        GameManager.Instance.pauseGame = true;
        dialogue.SetActive(true);
        TextMeshProUGUI txt = dialogue.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        if (!GameManager.Instance.skipDialogue)
        {
            for (int i = 0; i < introDialogue.Length; i++)
            {
                introDialogue[i] = ShowPortraits(introDialogue[i]);
                float slowDown = (i < 1) ? 1.5f : 1f;
                txt.text = "";
                foreach (char c in introDialogue[i])
                {
                    if (c == '*')
                        yield return new WaitForSeconds(0.15f);
                    else if (c != '~')
                    {
                        txt.text += c;
                        if (c == '.' || c == ',')
                            yield return new WaitForSeconds(0.10f * slowDown);
                        else if (c == ' ')
                            yield return new WaitForSeconds(0.10f * slowDown);
                        else
                            yield return new WaitForSeconds(0.05f * slowDown);
                    }
                }
                if (i == introDialogue.Length-2)
                {
                    Fader.Instance.FadeOut(10);
                    AudioManager.Instance.Play("Area 1");
                    StartCoroutine(AudioManager.Instance.StartFade("Area 1", 0.5f, 0.2f));
                }
                else if (i == introDialogue.Length-1)
                {
                    GameManager.Instance.pauseGame = false;
                }
                yield return new WaitForSeconds(2);
            }
            dialogue.SetActive(false);
        }
        else
        {
            dialogue.SetActive(false);
            AudioManager.Instance.Play("Area 1");
            StartCoroutine(AudioManager.Instance.StartFade("Area 1", 0.5f, 0.2f));
            Fader.Instance.FadeOut(0.5f);
            yield return new WaitForSeconds(0.5f);
            GameManager.Instance.pauseGame = false;
        }
        
        //show area intro text
        float waitTime = 0.1f;
        foreach (char ch in "Abandoned Rooftop, Hightower District\n(Virtual Layer)")
        {
            if (ch == '(')
                yield return new WaitForSeconds(1);
            areaIntroText.text += ch;
            yield return new WaitForSeconds(waitTime);
            if (ch == 'p')
            {
                yield return new WaitForSeconds(0.3f);
                waitTime = 0.05f;
            }
        }
        yield return new WaitForSeconds(1);
        Color col = areaIntroText.color;
        for (float i = 1; i > 0; i -= 0.01f)
        {
            yield return new WaitForSeconds(0.01f);
            areaIntroText.color = new Color(col.r, col.g, col.b, i);
        }
        Destroy(areaIntroText.gameObject);
    }


    public IEnumerator FirstAccessPt(string[] dialogue)
    {
        GameManager.Instance.playerPaused = true;
        if (SequenceManager.Instance.runNum == 1)
        {
            buildSelect.GetChild(2).gameObject.SetActive(true);
            ProgramManager.Instance.programUI.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            for (int i = 0; i < dialogue.Length; i++)
            {
                yield return PlayDialogue(dialogue[i], 1f);
                if (i == 1)
                {
                    buildSelect.GetChild(2).gameObject.SetActive(false);
                    buildSelect.GetChild(1).gameObject.SetActive(true);
                    StartCoroutine(ProgressBar());
                    yield return new WaitForSeconds(3);
                }
                if (i == 5)
                    yield return new WaitForSeconds(1);
            }
            yield return new WaitForSeconds(3);
            StartCoroutine(PlayMultipleDialogues(firstAccessPt));
        }
        else
        {
            buildSelect.GetChild(0).gameObject.SetActive(true);
            ProgramManager.Instance.programUI.gameObject.SetActive(true);
            yield return new WaitForSeconds(1);
            StartCoroutine(PlayMultipleDialogues(firstAccessPt));
        }
        yield return new WaitUntil(() => !GameManager.Instance.playerPaused);
        GameManager.Instance.UnlockBarrier(GameObject.Find("Barrier").transform);
        GameManager.Instance.FinishTerminalIcon();
        
        yield return new WaitForSeconds(2);
        StartCoroutine(GameManager.Instance.WaveEnemies(1, new Vector3(31, 0, -5)));

        yield return new WaitForSeconds(1.2f);
        StartCoroutine(PlayMultipleDialogues(firstEnemy));
        GameObject.Find("Player").GetComponent<PlayerMovement>().hpBar.gameObject.SetActive(true);
    }

    private IEnumerator ProgressBar()
    {
        completeTxt.text = "Restarting... please wait";
        progressBar.fillAmount = 0;
        float elapsed = 0;
        while (elapsed < 18)
        {
            progressBar.fillAmount = Mathf.Min(elapsed/30, progressBar.fillAmount + (Random.Range(0.01f, 0.2f)/30));
            float randomWait = Random.Range(0.01f, 0.2f);
            elapsed += randomWait;
            yield return new WaitForSeconds(randomWait);
        }
        progressBar.fillAmount = 1;
        completeTxt.text = "Restart complete!";
        AudioManager.Instance.Play("Terminal Activate");
        yield return new WaitForSeconds(2);
        buildSelect.GetChild(0).gameObject.SetActive(true);
        for (float i = 2; i > 0; i -= 0.01f)
        {
            yield return new WaitForSeconds(0.01f);
            buildSelect.GetChild(1).GetComponent<CanvasGroup>().alpha = i/2f;
        }
        buildSelect.GetChild(1).gameObject.SetActive(false);
    }


    public IEnumerator GardenerDialogue()
    {
        GameManager.Instance.pauseGame = true;
        dialogue.SetActive(true);
        for (int i = 0; i < gardenerDialogue.Length-1; i++)
        {
            yield return PlayDialogue(gardenerDialogue[i], 1f);
        }
        StartCoroutine(PlayDialogue(gardenerDialogue[gardenerDialogue.Length-1]));
        yield return new WaitForSeconds(3);
        dialogue.SetActive(false);
        GameManager.Instance.pauseGame = false;
    }
}