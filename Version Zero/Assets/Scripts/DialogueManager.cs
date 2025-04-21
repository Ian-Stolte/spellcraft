using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private string[] plaintext;
    [SerializeField] private string[] encoded;

    [SerializeField] private GameObject txtPrefab;
    [SerializeField] private RectTransform scrollParent;
    
    [Header ("Customizable")]
    [SerializeField] private Vector2 spawnPos;
    [SerializeField] private float spacing;
    [SerializeField] private float typeSpeed;
    [SerializeField] private float messageDelay;
    [SerializeField] private int transDelay;
    [SerializeField] private int convFactor;


    void Start()
    {
        StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        yield return new WaitForSeconds(2);

        for (int i = 0; i < plaintext.Length; i++)
        {
            if (spawnPos.y > -400)
            {
                if (scrollParent.childCount == 0)
                    spawnPos -= new Vector2(0, spacing);
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
        
            for (int j = 0; j < plaintext[i].Length + transDelay; j++)
            {
                int convFactor = Random.Range(1, 3);
                for (int k = 0; k < convFactor; k++)
                {
                    if (j*convFactor + k < encoded[i].Length)
                    {
                        txt.text += encoded[i][j*convFactor + k];
                        yield return new WaitForSeconds(typeSpeed);
                    }
                }
                if (j >= transDelay)
                {
                    string end = (j-transDelay+convFactor < txt.text.Length) ? txt.text.Substring(j-transDelay+convFactor) : "";
                    txt.text = txt.text.Substring(0, j-transDelay) + plaintext[i][j-transDelay] + end;
                    yield return new WaitForSeconds(typeSpeed);
                }
            }
            txt.text = txt.text.Substring(0, plaintext[i].Length);
            yield return new WaitForSeconds(messageDelay);
        }
    }
}
