using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeybindButton : MonoBehaviour
{
    [SerializeField] private Color borderPink;
    [SerializeField] private Color textPink;
    [SerializeField] private Color borderBlue;
    [SerializeField] private Color textBlue;


    public void MakeActiveKeybind()
    {
        foreach (Transform child in transform.parent)
        {
            if (child.gameObject != gameObject)
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
    }
}