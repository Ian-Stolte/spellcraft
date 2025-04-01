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
        GameObject block = Instantiate(blockToAdd, Vector2.zero, Quaternion.identity, blockParent);
        for (int j = 0; j < 5; j++)
        {
            block.GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(-600, 600), Random.Range(-500, 500));
            //if (Physics2D.OverlapCircleAll(block.GetComponent<RectTransform>().anchoredPosition, 200, LayerMask.GetMask("Block")).Length <= 1)
            //    break;
            bool noOverlap = true;
            foreach (Transform child in blockParent)
            {
                if (child.gameObject != block && child.gameObject.activeSelf && Vector3.Distance(block.GetComponent<RectTransform>().anchoredPosition, child.GetComponent<RectTransform>().anchoredPosition) < 400)
                {
                    noOverlap = false;
                }
            }
            if (noOverlap)
                break;
            //Debug.Log("Trying again " + j);
        }
        block.name = block.name.Substring(0, block.name.Length-7);
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
