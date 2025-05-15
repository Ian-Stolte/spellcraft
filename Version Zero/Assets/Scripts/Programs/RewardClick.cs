using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RewardClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject blockToAdd;
    public Transform blockParent;

    public void AddSpell()
    {
        StartCoroutine(AddSpellCor());
    }

    public IEnumerator AddSpellCor()
    {
        AudioManager.Instance.Play("Reward Click");
        if (SpellManager.Instance.spellUI.gameObject.activeSelf)
        {
            yield return null;
            SpellManager.Instance.skipButton.SetActive(true);
            SpellManager.Instance.compileButton.SetActive(true);
        }
        else
        {
            GameObject.Find("Fader").GetComponent<Fader>().FadeInOut(1, 1);
            yield return new WaitForSeconds(1);
            SpellManager.Instance.Reforge();
        }
        GameObject.Find("Spell Rewards").SetActive(false);
        SpellManager.Instance.CreateBlock(blockToAdd);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.Play("Button Hover");
        transform.localScale *= 1.2f;
        transform.GetChild(5).localScale /= 1.2f;
        RectTransform rect = transform.GetChild(5).GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y/1.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale /= 1.2f;
        transform.GetChild(5).localScale *= 1.2f;
        RectTransform rect = transform.GetChild(5).GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y*1.2f);
    }
}
