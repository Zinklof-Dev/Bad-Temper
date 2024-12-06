using System;
using UnityEngine;
using Unity.Netcode;

public class Sword : NetworkBehaviour
{
    [Header("Combat Stats")]
    [SerializeField] float dmg;
    [SerializeField] float cooldown;

    private float actualCooldown;
    private Server server;

    public void Start()
    {
        AskServerForOwnershipRpc();
        server = GameObject.FindGameObjectWithTag("Server").GetComponent<Server>();
        server.ServerTick += OnServerTick;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            AskServerToAttackRpc();
        }
    }
    
    private void OnServerTick()
    { 
        if (actualCooldown > 0)
        {
            actualCooldown -= Server.serverDeltaTime;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AttackRpc()
    {
        //code to run the animation of attacking
    }

    [Rpc(SendTo.Server)]
    private void AskServerToAttackRpc(RpcParams rpcParams = default)
    {
        if (OwnerClientId != rpcParams.Receive.SenderClientId)
        {
            return;
        }
        else if (actualCooldown <= 0)
        {
            AttackRpc();
            actualCooldown = cooldown;
            return;
        }
        else 
        {
            return;
        }
    }
    
    [Rpc(SendTo.Server)]
    private void AskServerForOwnershipRpc(RpcParams rpcParams = default)
    {
        if (OwnerClientId != 0)
        {
            return;
        }
        
        ulong clientId = rpcParams.Receive.SenderClientId;

        NetworkObject.ChangeOwnership(clientId);
        return;
    }
}
