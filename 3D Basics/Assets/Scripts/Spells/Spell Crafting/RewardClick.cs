using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardClick : MonoBehaviour
{
    public GameObject blockToAdd;
    public Transform blockParent;

    public void AddSpell()
    {
        GameObject block = Instantiate(blockToAdd, Vector2.zero, Quaternion.identity, blockParent);
        block.GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(-600, 600), Random.Range(-500, 500));
        block.name = block.name.Substring(0, block.name.Length-7);
        GameObject.Find("Spell Rewards").SetActive(false);
    }
}
