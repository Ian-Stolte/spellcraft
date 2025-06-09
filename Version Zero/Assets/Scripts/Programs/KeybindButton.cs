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

    public Transform programListParent;
    public Transform targetProgramList;
    public Program targetProgram;

    public KeyCode keybind;
    public string keybindStr;

    [SerializeField] private TextMeshProUGUI tutorialText;


    public void Start()
    {
        targetProgram = null;
    }

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
        ProgramManager.Instance.activeKeybind = this;
    
        foreach (Transform child in programListParent)
        {
            if (child.name.Contains("Program List"))
            {
                if (child != targetProgramList)
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
        tutorialText.text = "Click on a program to assign it to <b>" + keybindStr;
    }
}