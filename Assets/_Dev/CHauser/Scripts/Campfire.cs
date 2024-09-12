using UnityEngine;
using Unity.Netcode;
using ZinklofDev.Console;

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

    /*public static Command<float> DAMAGE_CAMPFIRE = new Command<float>("0001x1500000003", "damage_campfire", "Damages Campfire", false, (t1) =>
    {
        DamageCastleClientRpc(t1);
    });

    private void Awake()
    {
        Shell.RegisterCommand(DAMAGE_CAMPFIRE);
    }*/

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;
        if(!IsServer) 
            return;
        // Instantiate(castlePrefab);
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
        campfireHealth.Value = this.campfireHealth.Value;

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
    }

    [ClientRpc]
    private void DamageCastleClientRpc(float damage)
    {
        campfireHealth.Value -= damage;
    }
}
