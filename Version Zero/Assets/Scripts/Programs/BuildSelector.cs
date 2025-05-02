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
        SpellManager.Instance.CreateBlock("Damage");
        List<Block> typeBlocks = SpellManager.Instance.ChooseRandom(3, new string[]{"Damage"}, type);
        List<Block> miscBlocks = SpellManager.Instance.ChooseRandom(1, new string[]{"Damage"});
        typeBlocks.AddRange(miscBlocks);
        foreach (Block b in typeBlocks)
        {
            SpellManager.Instance.CreateBlock(b.gameObject);
        }
        SpellManager.Instance.buildpath = type;
        yield return new WaitForSeconds(1);
        Fader.Instance.FadeInOut(0.5f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        GameObject.Find("Build Selection").SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!inTransition)
            transform.localScale *= 1.2f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!inTransition)
            transform.localScale /= 1.2f;
    }
}
