using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Hitbox : MonoBehaviour
{
    public Spell spell;

    public abstract void CheckCollisions();
}
