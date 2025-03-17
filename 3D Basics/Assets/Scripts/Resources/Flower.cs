using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : Interactable
{
    public override void Interact(PlayerInteract player)
    {
        active = false;
        player.AddItem("flowers", 1);
        gameObject.SetActive(false);
    }

    private void Start()
    {
        active = true;
    }
}
