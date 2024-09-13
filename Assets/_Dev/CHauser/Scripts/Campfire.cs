using UnityEngine;
using Unity.Netcode;
using ZinklofDev.Console;
using TMPro;

public class Campfire : NetworkBehaviour
{
    [Header("Variables Synced Across Network")]
    [SerializeField] private GameObject castlePrefab;
    [SerializeField] private NetworkVariable<float> campfireHealth = new NetworkVariable<float>(100);

    [Space(10)]

    [Header("Variables Only Needed With Host")]
    [SerializeField] private float healSpeed = 1f;
    [SerializeField] private float healAmount = 1;
    [SerializeField] private float maxHealth = 100;
    private float healTimer;
    [SerializeField] private TMP_Text healthText;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if (IsServer)
            {
                var instance = Instantiate(castlePrefab);
                var instanceNetworkObject = instance.GetComponent<NetworkObject>();
                instanceNetworkObject.Spawn();
            }

            // healthText = GameObject.Find("HealthText").GetComponent<TMP_Text>;
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;
        if(!IsServer) 
            return;

        HealCastleClientRpc();
    }

    [ClientRpc]
    private void HealCastleClientRpc()
    { 
        if(campfireHealth.Value > maxHealth)
            campfireHealth.Value = maxHealth;

        if(campfireHealth.Value <  maxHealth)
        {
            if (healTimer <= 0)
            {
                campfireHealth.Value += healAmount;
                Debug.Log(OwnerClientId + "; " + campfireHealth.Value);
                healTimer = 10;
            }
            else
            {
                healTimer -= healSpeed * Time.deltaTime;
            }
        }
        
        healthText.text = campfireHealth.Value.ToString();
    }

    [ClientRpc]
    private void DamageCastleClientRpc(float damage)
    {
        campfireHealth.Value -= damage;
    }
}
