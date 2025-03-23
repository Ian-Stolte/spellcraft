using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private int maxHealth;
    public int health;

    [Header("Refs")]
    [SerializeField] private Animator dmgFlash;


    void Start()
    {
        health = maxHealth;
    }


    public void TakeDamage(int dmg)
    {
        if (dmg > 0)
        {
            dmgFlash.Play("DamageFlash");
            health -= dmg;
            if (health <= 0)
                GameOver();
        }
    }


    public void GameOver()
    {
        Debug.Log("GAME OVER :(");
        health = maxHealth;
    }
}
