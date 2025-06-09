using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProgramList : MonoBehaviour
{
    public bool hasKeybind;


    public void MakeActive()
    {
        if (hasKeybind && ProgramManager.Instance.activeKeybind != null)
        {
            KeybindButton button = ProgramManager.Instance.activeKeybind;

            foreach (Program p in ProgramManager.Instance.programs)
            {
                TextMeshProUGUI txt = p.programList.transform.GetChild(6).GetComponent<TextMeshProUGUI>();
                if (p.programList != gameObject)
                {
                    //clear keybind's previous program
                    if (txt.text == button.keybindStr)
                    {
                        txt.text = "";
                        p.keybind = KeyCode.None;
                    }
                }
                else
                {
                    //get rid of this program's old binding
                    if (txt.text != "")
                    {
                        foreach (Transform child in button.transform.parent)
                        {
                            if (child.GetComponent<KeybindButton>().keybindStr == txt.text)
                            {
                                Destroy(child.GetChild(5).gameObject);
                                child.GetChild(4).GetComponent<TextMeshProUGUI>().text = "";
                                child.GetComponent<KeybindButton>().targetProgramList = null;
                                child.GetComponent<KeybindButton>().targetProgram = null;
                            }
                        }
                    }

                    //set the new keybind
                    txt.text = button.keybindStr;
                    p.keybind = button.keybind;
                    if (button.transform.childCount >= 6)
                        Destroy(button.transform.GetChild(5).gameObject);
                    Transform symbol = Instantiate(p.symbol, Vector2.zero, Quaternion.identity, button.transform).transform;
                    symbol.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    symbol.localScale /= 1.7f;
                    Block shape = p.blocks.Find(b=>b.tag == "shape");
                    button.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = shape.name;
                    button.GetComponent<KeybindButton>().targetProgram = p;
                }
            }
            transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = button.keybindStr;
            button.targetProgramList = transform;
            button.MakeActiveKeybind();
        }
    }
}
