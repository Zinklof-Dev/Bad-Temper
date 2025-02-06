using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using ZinklofDev.ConsoleV2;

public class BuildObject : NetworkBehaviour
{
    public float serverHealth = 100; // Health that is stored only on the server side. This is the main health that will be used in calculating when to break and other calculations based on health.
    public float localHealth = 100; // This health is stored on the client, and is the health that the client updates and reads. Can be used for health bar system or for potential healing that the client does
    public float prevServerHealth = 100; // This value will save the server health at the end of ServerUpdate so we only update the local health when the server health changes
    public bool hasCriticalObject = false;
    public GameObject criticalObject;

    public override void OnNetworkSpawn()
    {
        Server server = GameObject.FindGameObjectWithTag("Server").GetComponent<Server>();
        server.ServerTick += ServerUpdate;
        prevServerHealth = serverHealth;
        
        base.OnNetworkSpawn();
    }

    private void ServerUpdate()
    {
        if(prevServerHealth != serverHealth)
        {
            UpdateLocalHealthRPC(serverHealth);
            prevServerHealth = serverHealth;
        }

        if (serverHealth <= 0)
        {
            Server server = GameObject.FindGameObjectWithTag("Server").GetComponent<Server>();
            server.ServerTick -= ServerUpdate;
            NetworkObject.Despawn();
            Destroy(gameObject);
        }
        
        if (!hasCriticalObject)
            return;

        if (criticalObject == null)
        {
            Server server = GameObject.FindGameObjectWithTag("Server").GetComponent<Server>();
            server.ServerTick -= ServerUpdate;
            NetworkObject.Despawn();
            Destroy(gameObject);
        }

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateLocalHealthRPC(float serverHealth)
    {
        localHealth = serverHealth;
    }
}
