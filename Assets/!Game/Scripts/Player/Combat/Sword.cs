using System;
using Unity;

public class Sword : NetworkBehaviour
{
    [Header("Combat Stats")]
    [SerializeField] float dmg;
    [SerializeField] float cooldown;

    private float actualCoodown;
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
            actualCooldown -= server.ServerDeltaTime;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private AttackRpc()
    {
        //code to run the animation of attacking
    }

    [Rpc(SendTo.Server)]
    private bool AskServerToAttackRpc(RpcParams rpcParams = default)
    {
        if (OwnerClientId != rpcParams.Receive.SenderClientId)
        {
            return false;
        }
        else if (actualCooldown <= 0)
        {
            AttackRpc();
            actualCooldown = cooldown;
            return true;
        }
        else 
        {
            return false;
        }
    }
    
    [Rpc(SendTo.Server)
    private bool AskServerForOwnershipRpc(RpcParams rpcParams = default)
    {
        if (OwnerClientId != 0)
        {
            return false;
        }
        
        ulong clientId = rpcParams.Receive.SenderClientId;

        ChangeOwnership(clientId);
        return true;
    }
}
