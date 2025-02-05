using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using ZinklofDev.ConsoleV2;

public class BuildObject : NetworkBehaviour
{
    public float serverHealth = 100;
    public float localHealth = 100;
    public bool hasCriticalObject = false;
    public GameObject criticalObject;

    public override void OnNetworkSpawn()
    {
        Server server = GameObject.FindGameObjectWithTag("Server").GetComponent<Server>();
        server.ServerTick += ServerUpdate;

        base.OnNetworkSpawn();
    }

    private void ServerUpdate()
    {
        UpdateLocalHealthRPC(serverHealth);

        if (serverHealth <= 0)
        {
            NetworkObject.Spawn(false);
            Destroy(gameObject);
        }

        if (!hasCriticalObject)
            return;

        if (criticalObject == null)
        {
            NetworkObject.Spawn(false);
            Destroy(gameObject);
        }

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateLocalHealthRPC(float serverHealth)
    {
        localHealth = serverHealth;
    }
}
