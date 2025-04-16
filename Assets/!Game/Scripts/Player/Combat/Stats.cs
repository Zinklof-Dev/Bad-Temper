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
    [Header("Regen Stats")]
    [SerializeField] float healthRate = 1;
    [SerializeField] float stamRate = 10;
    [SerializeField] float manaRate = 5;
    [SerializeField] float timeBeforeStam = 2;
    [SerializeField] float timeBeforeHeal = 5;
    [SerializeField] float timeBeforeMana = 1.5f;
    [Header("UI")]
    // [SerializeField] 

    private float maxHealth = 100;
    private float maxStam = 100;
    private float maxMana = 150;

    private float timeSinceStam;
    private float timeSinceDamage;
    private float timeSinceMana;

    public void UpdateUI()
    {
        Debug.LogError("UpdateUI Not Yet Implimented");

        //update bars

        //update screen effects
    }

    public void Damage(float val)
    {
        health -= val;
        
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        else if (health < 0)
            Death();

        UpdateUI();
    }

    public bool UseMana(float val)
    {
        if (mana - val > 0)
        {
            mana -= val;
            return true;
        }
        else
            return false;
    }

    public bool UseStamina(float val)
    {
        if (stam - val > 0)
        {
            stam -= val;

            return true;
        }
        else 
            return false;
    }

    public void Death()
    {
        // code to set screen to black

        // contact player class then teleport them somewhere, wait some time, teleport back into combat.
    }

    public void Regenerate()
    {
        if (timeSinceDamage > timeBeforeHeal)
            health += healthRate * Time.deltaTime;
        if (timeSinceStam > timeBeforeStam)
            health += stamRate * Time.deltaTime;
        if (timeSinceMana > timeBeforeMana)
            mana += manaRate * Time.deltaTime;
    }

    public void Update()
    {
        timeSinceDamage += Time.deltaTime;
        timeSinceMana += Time.deltaTime;
        timeSinceStam += Time.deltaTime;

        Regenerate();
    }
}