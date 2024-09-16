using UnityEngine;
using Unity.Netcode;
using ZinklofDev.Console;
using TMPro;

public class Campfire : NetworkBehaviour
{
    [Header("Variables Synced Across Network")]
    [SerializeField] private NetworkVariable<float> campfireHealth = new NetworkVariable<float>(100);

    [Space(10)]

    [Header("Client Side Refrences")]
    [SerializeField] private GameObject castlePrefab;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private string healthBarName = "CampfireHealthBar";
    [SerializeField] private float campfireHealthRefrence;

    [Space(10)]

    [Header("Variables Only Needed With Host")]
    [SerializeField] private float healSpeed = 1f;
    [SerializeField] private float healAmount = 1;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float healTime = 10;
    private float healTimer;


    public override void OnNetworkSpawn()
    {
        Instantiate(castlePrefab);
        healthBar = GameObject.Find(healthBarName);
        campfireHealth.OnValueChanged += UpdateCampfireHealthValue;
    }

    private void UpdateCampfireHealthValue(float oldValue, float newValue)
    {
        campfireHealthRefrence = newValue;
    }

    private void Update()
    {
        UpdateHealthBar();

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

    private void UpdateHealthBar()
    {
        float xScale = campfireHealthRefrence / 10f;
        healthBar.transform.localScale = new Vector3(xScale, 1, 1);
    }

    private void DamageCastle(float damage)
    {
        campfireHealth.Value -= damage;
    }
}
