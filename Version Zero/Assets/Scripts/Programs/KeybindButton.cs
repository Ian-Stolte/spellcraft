 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeybindButton : MonoBehaviour
{
    [SerializeField] private Color borderPink;
    [SerializeField] private Color textPink;
    [SerializeField] private Color dropShadowPink;
    [SerializeField] private Color borderBlue;
    [SerializeField] private Color textBlue;
    [SerializeField] private Color dropShadowBlue;

    public Transform programList;
    public Transform targetProgram;


    public void MakeActiveKeybind()
    {
        foreach (Transform child in transform.parent)
        {
            if (child != transform)
            {
                child.GetChild(0).GetComponent<TextMeshProUGUI>().color = textBlue;
                child.GetChild(1).GetComponent<Image>().color = borderBlue;
            }
            else
            {
                child.GetChild(0).GetComponent<TextMeshProUGUI>().color = textPink;
                child.GetChild(1).GetComponent<Image>().color = borderPink;
            }
        }
        ProgramManager.Instance.activeKeybind = transform.GetSiblingIndex();
    
        foreach (Transform child in programList)
        {
            if (child.name.Contains("Spell List"))
            {
                if (child != targetProgram)
                {
                    child.GetChild(0).GetComponent<Image>().color = dropShadowBlue;
                    child.GetChild(2).GetComponent<Image>().color = dropShadowBlue;
                    child.GetChild(6).GetComponent<TextMeshProUGUI>().color = new Color(233/255f, 233/255f, 233/255f, 1);
                }
                else
                {
                    child.GetChild(0).GetComponent<Image>().color = dropShadowPink;
                    child.GetChild(2).GetComponent<Image>().color = dropShadowPink;
                    child.GetChild(6).GetComponent<TextMeshProUGUI>().color = textPink;
                }
            }
        }
    }
}