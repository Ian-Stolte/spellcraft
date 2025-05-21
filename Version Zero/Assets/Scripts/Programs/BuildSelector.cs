using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string type;
    [HideInInspector] public bool inTransition;
    [SerializeField] private CanvasGroup fader;


    public void ChooseBuild()
    {
        foreach (Transform child in transform.parent.parent)
        {
            if (child.childCount > 0)
            {
                Button b = child.GetChild(0).GetComponent<Button>();
                if (b != null)
                {
                    b.interactable = false;
                    b.GetComponent<BuildSelector>().inTransition = true;
                }
            }
        }
        AudioManager.Instance.Play("Build Select");
        Destroy(GetComponent<Button>());
        inTransition = true;
        transform.parent.SetSiblingIndex(transform.parent.parent.childCount - 1);
        StartCoroutine(ChooseBuildCor());
    }

    public IEnumerator ChooseBuildCor()
    {
        for (float i = 0; i < 1; i += 0.01f)
        {
            fader.alpha = i;
            yield return new WaitForSeconds(0.01f);
        }
        ProgramManager.Instance.CreateBlock("Damage");
        List<Block> effectBlocks = ProgramManager.Instance.ChooseRandom(1, new string[]{"Damage"}, type, "effect");
        List<Block> miscBlocks = ProgramManager.Instance.ChooseRandom(1, new string[]{"Damage"}, "none", "effect");
        List<Block> shapeBlocks = ProgramManager.Instance.ChooseRandom(2, new string[]{"Damage"}, type, "shape");
        effectBlocks.AddRange(miscBlocks);
        effectBlocks.AddRange(shapeBlocks);
        foreach (Block b in effectBlocks)
        {
            ProgramManager.Instance.CreateBlock(b.gameObject);
        }
        ProgramManager.Instance.buildpath = type;
        yield return new WaitForSeconds(1);
        Fader.Instance.FadeInOut(0.5f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        GameObject.Find("Build Selection").SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.Play("Button Hover");
        if (!inTransition)
            transform.localScale *= 1.2f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!inTransition)
            transform.localScale /= 1.2f;
    }
}
