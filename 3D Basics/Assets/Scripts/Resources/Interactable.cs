using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string itemName;
    public bool active = true;
    public bool destroyParent;
    public KeyCode keybind = KeyCode.F;
    public GameObject text;

    public virtual void Interact(PlayerInteract player)
    {
        player.AddItem(itemName, 1);
        if (destroyParent)
            Destroy(transform.parent.gameObject);
        else
            Destroy(gameObject);
    }
}
