using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private string[] plaintext;
    [SerializeField] private string[] encoded;
    private string untranslated;
    private string translated;

    [SerializeField] private GameObject txtPrefab;
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
    [SerializeField] private Animator hideLearning;

    [Header("Upload/Download")]
    [SerializeField] private TextMeshProUGUI downloadTxt;
    [SerializeField] private TextMeshProUGUI uploadTxt;
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minDelay;
    [SerializeField] private float maxDelay;

    [Header("Neural Activity")]
    [SerializeField] private RectTransform point;
    [SerializeField] private Vector2 direction;
    private int sign = 1;
    [SerializeField] private float speed;
    private int randomChance;
    [SerializeField] private Vector2 min;
    [SerializeField] private Vector2 max;
    [SerializeField] private Vector2 flipChance;


    void Start()
    {
        StartCoroutine(TypeText());
        StartCoroutine(SpeedText(downloadTxt, 2*minSpeed, 2*maxSpeed));
        StartCoroutine(SpeedText(uploadTxt, minSpeed, maxSpeed));
        direction = direction.normalized;
    }


    private void Update()
    {
        point.anchoredPosition += new Vector2(direction.x, direction.y*sign)*speed/60;
        if (Random.Range(flipChance.x, flipChance.y) <= randomChance || point.anchoredPosition.y > max.y || point.anchoredPosition.y < min.y)
        {
            randomChance = 0;
            sign *= -1;
        }
        else
        {
            randomChance++;
        }

        if (point.anchoredPosition.x > max.x)
            point.anchoredPosition = new Vector2(min.x, point.anchoredPosition.y);
    }


    private IEnumerator TypeText()
    {
        yield return new WaitForSeconds(2);

        for (int i = 0; i < plaintext.Length; i++)
        {
            if (i == 2)
                StartCoroutine(ShowLearning());

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
                scrollParent.anchoredPosition += new Vector2(0, spacing + lastHeight);
            }
            GameObject txtObj = Instantiate(txtPrefab, Vector2.zero, Quaternion.identity, scrollParent);
            txtObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(spawnPos.x, spawnPos.y - scrollParent.anchoredPosition.y);
            TextMeshProUGUI txt = txtObj.GetComponent<TextMeshProUGUI>();
            txt.text = "";
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
            }
            txt.text = translated;
            yield return new WaitForSeconds(messageDelay);
        }

        yield return new WaitForSeconds(2);
        Fader.Instance.FadeIn(2);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("Room 1");
    }


    private IEnumerator ShowLearning()
    {
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

        string message2 = errorTxt.text;
        errorTxt.text = "";
        errorTxt.gameObject.SetActive(true);
        foreach (char c in message2)
        {
            errorTxt.text += c;
            yield return new WaitForSeconds(0.001f);
        }
    }


    private IEnumerator SpeedText(TextMeshProUGUI txt, float totalMin, float totalMax)
    {
        float currSpd = Random.Range(totalMin, totalMax);
        while (true)
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
    }
}
