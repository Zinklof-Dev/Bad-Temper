using Unity;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

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

        timeSinceDamage = 0;

        UpdateUI();
    }

    public bool UseMana(float val)
    {
        if (mana - val > 0)
        {
            mana -= val;
            timeSinceMana = 0;
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
            timeSinceStam = 0;

            return true;
        }
        else 
            return false;
    }

    public void Death()
    {
        SomeoneDiedRPC();
    }

    public void Regenerate()
    {
        if (timeSinceDamage > timeBeforeHeal)
            health += healthRate * Time.deltaTime;
        if (timeSinceStam > timeBeforeStam)
            stam += stamRate * Time.deltaTime;
        if (timeSinceMana > timeBeforeMana)
            mana += manaRate * Time.deltaTime;

        health =  Mathf.Clamp(health, -100, maxHealth);
        stam = Mathf.Clamp(stam, -100, maxStam);
        mana = Mathf.Clamp(mana, -100, maxMana);
    }

    public void Update()
    {
        timeSinceDamage += Time.deltaTime;
        timeSinceMana += Time.deltaTime;
        timeSinceStam += Time.deltaTime;

        Regenerate();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void CrashRPC()
    {
        Application.Quit();
    }

    [Rpc(SendTo.Server)]
    public void SomeoneDiedRPC()
    { 
        CrashRPC();
    }
}