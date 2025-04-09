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
        SpellManager.Instance.CreateBlock(blockToAdd);
        GameObject.Find("Fader").GetComponent<Fader>().FadeInOut(1, 1);
        yield return new WaitForSeconds(1);
        GameObject.Find("Spell Rewards").SetActive(false);
        SpellManager.Instance.Reforge();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale *= 1.2f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale /= 1.2f;
    }
}
