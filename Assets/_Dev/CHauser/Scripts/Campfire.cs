using UnityEngine;
using Unity.Netcode;
using ZinklofDev.Console;
using TMPro;
using System;

public class Campfire : NetworkBehaviour
{
    [Header("Client Side Refrences")]
    [SerializeField] private GameObject castlePrefab;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject camera;
    [SerializeField] private string healthBarName = "CampfireHealthBar";
    [SerializeField] private float campfireHealthRefrence;

    [Space(10)]

    [Header("Variables Only Needed With Host")]
    [SerializeField] private float healSpeed = 1f;
    [SerializeField] private float healAmount = 1;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float healTime = 10;
    private float healTimer;

    [Space(10)]

    [Header("Variables Synced Across Network")]
    [SerializeField] private NetworkVariable<float> campfireHealth = new NetworkVariable<float>(0);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            campfireHealth.Value = maxHealth;

        Instantiate(castlePrefab);
        healthBar = GameObject.Find(healthBarName);
        camera = GameObject.FindGameObjectWithTag("MainCamera");
        campfireHealthRefrence = campfireHealth.Value;
        campfireHealth.OnValueChanged += UpdateCampfireHealthValue;

        base.OnNetworkSpawn();
    }

    private void UpdateCampfireHealthValue(float oldValue, float newValue)
    {
        campfireHealthRefrence = newValue;
        UpdateHealthBar();
    }

    private void Update()
    {
        if (healthBar == null)
        {
            return;
        }

        HealthBarLookAtCamera();

        if (!IsOwner)
            return;
        if (!IsServer) 
            return;

        HealCastle();
    }

    private void HealCastle()
    { 
        if(campfireHealth.Value > maxHealth)
            campfireHealth.Value = maxHealth;

        if(campfireHealth.Value <  maxHealth)
        {
            if (healTimer <= 0)
            {
                campfireHealth.Value += healAmount;
                Debug.Log(OwnerClientId + "; " + campfireHealth.Value);
                healTimer = healTime;
            }
            else
            {
                healTimer -= healSpeed * Time.deltaTime;
            }
        }
    }

    private void HealthBarLookAtCamera()
    {
        healthBar.transform.parent.LookAt(camera.transform);
    }

    private void UpdateHealthBar()
    {
        float percentage = campfireHealthRefrence / maxHealth;
        healthBar.transform.localScale = new Vector3(percentage * .96f, 0.65f, 1);
    }

    /* private void CheckIfEndGame()
    {
        if (campfireHealth.Value == 0)
        {

        }
    } */

    private void DamageCastle(float damage)
    {
        campfireHealth.Value -= damage;
    }
}
