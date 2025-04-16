using Unity;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Stats : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] float health = 100;
    [SerializeField] float stam = 100;
    [SerializeField] float mana = 150;
    [Header("UI")]
    // [SerializeField] 

    private float maxHealth = 100;
    private float maxStam = 100;
    private float maxMana = 150;

    public void UpdateUI()
    {
        Debug.LogError("UpdateUI Not Yet Implimented");
    }

    public void Damage(float val)
    {
        health -= val;
        
        if (health > maxHealth)
        {
            health = maxHealth
        }
        else if (health < 0)
            Death();

        UpdateUI();
    }

    public bool UseMana(float val)
    {
        
    }

    public void Death()
    {
        // code to set screen to black

        // contact player class then teleport them somewhere, wait some time, teleport back into combat.
    }
}