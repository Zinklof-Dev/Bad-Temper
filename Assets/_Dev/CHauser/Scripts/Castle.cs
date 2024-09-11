using UnityEngine;
using Unity.Netcode;

public class Castle : NetworkBehaviour
{
    [Header("Variables Synced Across Network")]
    [SerializeField] private GameObject castlePrefab;
    [SerializeField] private NetworkVariable<float> castleHealth = new NetworkVariable<float>(100);

    [Space(10)]

    [Header("Variables Only Needed With Host")]
    [SerializeField] private float healSpeed;
    [SerializeField] private float healAmount;
    [SerializeField] private float maxHealth;

    public override void OnNetworkSpawn()
    {
        if(!IsServer) 
            return;

        GameObject spawnedCastle = Instantiate(castlePrefab);
        spawnedCastle.GetComponent<NetworkObject>().Spawn(true);
    }

    private void Update()
    {
        if(!IsServer) 
            return;

        HealCastle();
    }

    private void HealCastle()
    {
        
    }

    private void DamageCastle(float damage)
    {
        castleHealth.Value -= damage;
    }
}
