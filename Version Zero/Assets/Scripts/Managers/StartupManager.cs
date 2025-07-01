using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartupManager : MonoBehaviour
{
    [Header("Bools")]
    [SerializeField] private bool fail;
    [SerializeField] private bool instantFail;
    private bool skipping;

    [Header("Dialogue")]
    [SerializeField] private string[] plaintext;
    [SerializeField] private string[] encoded;
    [SerializeField] private string[] failtext;
    [SerializeField] private string[] failcodes;
    private string untranslated;
    private string translated;

    [SerializeField] private GameObject txtPrefab;
    private TextMeshProUGUI txt;
    [SerializeField] private RectTransform scrollParent;
    
    [Header("Customizable")]
    [SerializeField] private Vector2 spawnPos;
    [SerializeField] private float spacing;
    [SerializeField] private float typeSpeed;
    [SerializeField] private float messageDelay;
    [SerializeField] private int transDelay;
    [SerializeField] private int convFactor;

    [Header("Learning")]
    [SerializeField] private TextMeshProUGUI learningTitle;
    [SerializeField] private TextMeshProUGUI ellipsisTxt;
    [SerializeField] private TextMeshProUGUI errorTxt;
    [SerializeField] private TextMeshProUGUI statsTxt;
    [SerializeField] private Animator hideLearning;
    private float rawTime;
    private float gameplayTime;

    [Header("Upload/Download")]
    [SerializeField] private TextMeshProUGUI downloadTxt;
    [SerializeField] private TextMeshProUGUI uploadTxt;
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minDelay;
    [SerializeField] private float maxDelay;
    private bool uploadFailed;

    [Header("Neural Activity")]
    [SerializeField] private RectTransform point;
    [SerializeField] private Vector2 direction;
    private int sign = 0;
    [SerializeField] private float speed;
    private int randomChance;
    [SerializeField] private Vector2 min;
    [SerializeField] private Vector2 max;
    [SerializeField] private Vector2 flipChance;
    [SerializeField] private GameObject pointTrail;
    [SerializeField] private float trailDensity;
    private int trailCount;
    private bool manualControl;

    [Header("Progress Bar")]
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject completeTxt;

    [Header("Misc")]
    [SerializeField] private GameObject errorFlash;
    [SerializeField] private CanvasGroup spaceToSkip;
    [SerializeField] private TextMeshProUGUI versionTxt;


    void Start()
    {
        if (SequenceManager.Instance != null)
        {
            plaintext[5] = "Procedure complete. Version " + SequenceManager.Instance.runNum + ".0 online.";
            versionTxt.text = "v " + SequenceManager.Instance.runNum + ".0";
            fail = (SequenceManager.Instance.runNum == 1);
            rawTime = SequenceManager.Instance.rawTimer;
            SequenceManager.Instance.rawTimer = 0;
            gameplayTime = SequenceManager.Instance.gameplayTimer;
            SequenceManager.Instance.gameplayTimer = 0;
        }

        if (!instantFail)
        {
            StartCoroutine(NeuralActivity());
            AudioManager.Instance.Play("Startup UI");
        }
        StartCoroutine(SpeedText(downloadTxt, 2 * minSpeed, 2 * maxSpeed));
        direction = new Vector2(direction.x / point.parent.localScale.x, direction.y / point.parent.localScale.y).normalized;

        if (instantFail)
            StartCoroutine(UploadFailed());

        StartCoroutine(FadeOutSpaceToSkip());
    }

    private IEnumerator FadeOutSpaceToSkip()
    {
        yield return new WaitForSeconds(2);
        float elapsed = 0;
        while (elapsed < 1)
        {
            spaceToSkip.alpha = elapsed;
            elapsed += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(1);
        elapsed = 0;
        while (elapsed < 2)
        {
            spaceToSkip.alpha = 1 - elapsed / 2;
            elapsed += Time.deltaTime;
            yield return null;
        }
        spaceToSkip.alpha = 0;
    }


    private void Update()
    {
        if (Input.GetKey(KeyCode.Space) && !skipping)
        {
            skipping = true;
            StartCoroutine(SkipIntro());
        }
    }

    private IEnumerator SkipIntro()
    {
        Fader.Instance.FadeIn(2);
        StartCoroutine(AudioManager.Instance.StartFade("Startup UI", 2, 0f));
        yield return new WaitForSeconds(2);
        AudioManager.Instance.Stop("Startup UI");
        SceneManager.LoadScene("Level 1");
    }


    private void FixedUpdate()
    {
        point.anchoredPosition += new Vector2(direction.x, direction.y * sign) * speed / 60;
        if (!manualControl)
        {
            if (Random.Range(flipChance.x, flipChance.y) <= randomChance || point.anchoredPosition.y > max.y || point.anchoredPosition.y < min.y)
            {
                randomChance = 0;
                if (sign == 0)
                {
                    if (Random.Range(0f, 1f) < 0.5f)
                        sign = 1;
                    else
                        sign = -1;
                }
                if (Random.Range(0f, 1f) < 0.5f)
                    sign *= -1;
                else
                    sign = 0;

                point.anchoredPosition = new Vector2(point.anchoredPosition.x, Mathf.Clamp(point.anchoredPosition.y, min.y + 5, max.y - 5));
            }
            else
                randomChance++;
        }
        trailCount++;
        if (trailCount >= trailDensity)
        {
            GameObject trailObj = Instantiate(pointTrail, point.position, Quaternion.identity, point.parent);
            trailObj.transform.localScale = new Vector2(trailObj.transform.localScale.x / point.parent.localScale.x, trailObj.transform.localScale.y / point.parent.localScale.y);
            trailCount = 0;
        }

        if (point.anchoredPosition.x > max.x)
            point.anchoredPosition = new Vector2(min.x, point.anchoredPosition.y);
    }


    private IEnumerator NeuralActivity()
    {
        manualControl = true;
        sign = 0;
        yield return new WaitForSeconds(4);
        StartCoroutine(Peak(0.1f));
        yield return new WaitForSeconds(2);
        StartCoroutine(Peak(0.15f));
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(Peak(0.1f));
        if (fail)
            StartCoroutine(FailSeq());
        else
            StartCoroutine(SuccessSeq());
        yield return new WaitForSeconds(3);
        sign = 1;
        manualControl = false;
    }

    private IEnumerator PauseNeural()
    {
        yield return new WaitForSeconds(2);
        manualControl = true;
        sign = -1;
        direction *= 2;
        speed *= 2;
        //trailDensity *= 0.25f;
        yield return new WaitUntil(() => point.anchoredPosition.y < min.y + 70);
        sign = 0;
        direction *= 0.5f;
        speed *= 0.5f;
    }

    private IEnumerator Peak(float duration)
    {
        sign = 1;
        yield return new WaitForSeconds(duration);
        sign = -1;
        yield return new WaitForSeconds(duration);
        sign = 0;
    }


    private IEnumerator SuccessSeq()
    {
        for (int i = 0; i < plaintext.Length; i++)
        {
            if (i == 1)
                StartCoroutine(ShowLearning());
            else if (i == 2)
            {
                yield return new WaitForSeconds(3);
                StartCoroutine(SpeedText(uploadTxt, minSpeed, maxSpeed));
                StartCoroutine(ProgressBar(4));
            }
            else if (i == 3)
            {
                StartCoroutine(ProgressBar(2));
                StartCoroutine(PauseNeural());
            }
            else if (i == 4)
            {
                sign = 1;
                manualControl = false;
            }
            /*else if (i == 5)
                StartCoroutine(ProgressBar(8));
            else if (i == 6)
                yield return new WaitForSeconds(3);*/

            yield return StartCoroutine(TypeText(i));
        }

        Fader.Instance.FadeIn(2);
        StartCoroutine(AudioManager.Instance.StartFade("Startup UI", 2, 0));
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("Level 1");
    }

    private IEnumerator FailSeq()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i == 1)
                StartCoroutine(ShowLearning());
            else if (i == 3)
            {
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(SpeedText(uploadTxt, minSpeed, maxSpeed));
                StartCoroutine(ProgressBar(3.5f, 2.5f));
                //StartCoroutine(AudioManager.Instance.StartFade("Startup UI", 3, 0));
            }
            
            yield return StartCoroutine(TypeText(i));
        }

        //type out error warnings
        AudioManager.Instance.Stop("Startup UI");
        AudioManager.Instance.Play("Alarm");
        yield return new WaitForSeconds(1);
        for (int i = 0; i < failtext.Length; i++)
        {
            yield return SpawnText(failtext[i] + failcodes[i]);
            int charsTyped = 0;
            untranslated = "";
            translated = "";
            for (int j = 0; j < failtext[i].Length; j++)
            {
                untranslated += failtext[i][j];
                if (Random.Range(0f, 1f) < 0.1f)
                    untranslated += "<color=#95EAE1>" + failcodes[i][j] + "</color>";
                //translated += failcodes[i][j];
                txt.text = untranslated + "<color=#95EAE1> " + translated;
                yield return new WaitForSeconds(typeSpeed);
            }
            if (failtext[i] != "Restarting syst—")
               yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator SpawnText(string text)
    {
        if (spawnPos.y > -250)
        {
            if (scrollParent.childCount == 0)
                spawnPos += new Vector2(0, spacing);
            else
            {
                float lastHeight = scrollParent.GetChild(scrollParent.childCount-1).GetComponent<TextMeshProUGUI>().preferredHeight;
                spawnPos -= new Vector2(0, spacing + lastHeight);
            }
        }
        else
        {
            float lastHeight = scrollParent.GetChild(scrollParent.childCount-1).GetComponent<TextMeshProUGUI>().preferredHeight;
            TextMeshProUGUI testObj = Instantiate(txtPrefab, Vector2.zero, Quaternion.identity, scrollParent).GetComponent<TextMeshProUGUI>();
            testObj.text = text;
            yield return null;
            scrollParent.anchoredPosition += new Vector2(0, spacing + testObj.preferredHeight);
            spawnPos -= new Vector2(0, lastHeight - testObj.preferredHeight);
            Destroy(testObj.gameObject);
        }
        GameObject txtObj = Instantiate(txtPrefab, Vector2.zero, Quaternion.identity, scrollParent);
        txtObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(spawnPos.x, spawnPos.y - scrollParent.anchoredPosition.y);
        txt = txtObj.GetComponent<TextMeshProUGUI>();
        txt.text = "";
    }

    private IEnumerator TypeText(int i)
    {
        yield return SpawnText(plaintext[i]);
        untranslated = "";
        translated = "";
    
        for (int j = 0; j < plaintext[i].Length + transDelay; j++)
        {
            int convFactor = Random.Range(1, 3);
            for (int k = 0; k < convFactor; k++)
            {
                if (j*convFactor + k < encoded[i].Length)
                {
                    untranslated += encoded[i][j*convFactor + k];
                    txt.text = translated + "<color=#95EAE1> " + untranslated;
                    yield return new WaitForSeconds(typeSpeed);
                }
            }
            if (j >= transDelay)
            {
                //string end = (j-transDelay+convFactor < currTxt.Length) ? currTxt.Substring(j-transDelay+convFactor) : "";
                untranslated = (convFactor < untranslated.Length) ? untranslated.Substring(convFactor) : "";
                translated += plaintext[i][j-transDelay];
                txt.text = translated + "<color=#95EAE1> " + untranslated;
                yield return new WaitForSeconds(typeSpeed);
            }

            if (uploadFailed)
                yield break;
        }
        txt.text = translated;
        yield return new WaitForSeconds(messageDelay);
    }


    private IEnumerator ShowLearning()
    {
        if (!fail)
            yield return new WaitForSeconds(1f);
        else
            yield return new WaitForSeconds(3f);
        string message1 = learningTitle.text;
        learningTitle.text = "";
        learningTitle.gameObject.SetActive(true);
        foreach (char c in message1)
        {
            learningTitle.text += c;
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);
        
        Vector2 startingPos = hideLearning.GetComponent<RectTransform>().anchoredPosition;
        hideLearning.Play("Transition");
        yield return new WaitForSeconds(2);

        ellipsisTxt.text = "";
        ellipsisTxt.gameObject.SetActive(true);
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                ellipsisTxt.text += ".";
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(0.3f);
            ellipsisTxt.text = "";
            yield return new WaitForSeconds(0.5f);
        }
        ellipsisTxt.text = ".";
        yield return new WaitForSeconds(0.3f);
        ellipsisTxt.text = "";

        if (fail)
        {
            yield return new WaitForSeconds(0.3f);
            string errorMessage = errorTxt.text;
            errorTxt.text = "";
            errorTxt.gameObject.SetActive(true);
            foreach (char c in errorMessage)
            {
                errorTxt.text += c;
                yield return new WaitForSeconds(0.001f);
            }
        }
        else if (SequenceManager.Instance.lastRoom == 0)
        {
            foreach (char c in "No data found.")
            {
                statsTxt.text += c;
                yield return new WaitForSeconds(0.01f);
            }   
        }
        else
        {
            foreach (string s in new string[]{"Furthest level reached: "+SequenceManager.Instance.lastRoom, "Time (gameplay): "+FormatTime(gameplayTime), "Time (total): "+FormatTime(rawTime), "Reason for reset:", "Corrupted memory files, neural overload", "(Compromised data removed to preserve cognitive integrity)"})
            {
                if (s.Contains("Reason") || s.Contains("Compromised data"))
                {
                    statsTxt.text += "\n";
                    yield return new WaitForSeconds(1.5f);
                }
                foreach (char c in s)
                {
                    statsTxt.text += c;
                    yield return new WaitForSeconds(0.01f);
                }
                statsTxt.text += "\n";
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private string FormatTime(float time)
    {
        if (time > 3600)
            return "> 1hr";
        int minutes = (int)time/60;
        int seconds = (int)Mathf.Min(59, Mathf.Round(time%60));
        string secondsStr = (seconds < 10) ? ":0" + seconds : ":" + seconds;
        return minutes + secondsStr;
    }


    private IEnumerator SpeedText(TextMeshProUGUI txt, float totalMin, float totalMax)
    {
        float currSpd = Random.Range(totalMin, totalMax);
        while (!uploadFailed)
        {
            float randomNoise = Random.Range(0f, 1f);
            if (randomNoise < 0.05f && currSpd > 1)
            {
                currSpd = Mathf.Max(totalMin, currSpd*0.2f);
            }
            else if (randomNoise < 0.1f)
            {
                currSpd = Mathf.Min(totalMax, currSpd*5);;
            }
            float min = Mathf.Max(totalMin, currSpd*0.9f);
            float max = Mathf.Min(totalMax, currSpd*1.1f);
            txt.text = Mathf.Round(10*Random.Range(min, max))/10f + "";
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        }
        txt.text = "####";
    }


    private IEnumerator ProgressBar(float duration, float failTime=0)
    {
        completeTxt.SetActive(false);
        progressBar.gameObject.SetActive(true);
        progressBar.fillAmount = 0;
        float elapsed = 0;
        while (elapsed < duration)
        {
            progressBar.fillAmount = Mathf.Min(elapsed/duration, progressBar.fillAmount + (Random.Range(0.01f, 0.2f)/duration));
            float randomWait = Random.Range(0.01f, 0.2f);
            elapsed += randomWait;
            yield return new WaitForSeconds(randomWait);

            if (failTime != 0 && elapsed > failTime)
            {
                StartCoroutine(UploadFailed());
                yield break;
            }
        }
        progressBar.fillAmount = 1;
        completeTxt.SetActive(true);
    }

    private IEnumerator UploadFailed()
    {
        uploadFailed = true;

        //change neural activity
        direction.y = 2;
        direction.x = 0.1f;
        speed = 1200;
        trailDensity = 1;
        flipChance = new Vector2(1, 10);
    
        if (!instantFail)
            yield return new WaitForSeconds(4);
        errorFlash.SetActive(true);
        if (instantFail)
            yield return new WaitForSeconds(3);
        else
            yield return new WaitForSeconds(12);
        Fader.Instance.GetComponent<CanvasGroup>().alpha = 1;
        AudioManager.Instance.Stop("Alarm");
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Level 1");
    }
}
