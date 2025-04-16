using System;
using UnityEngine;
using Unity.Netcode;

public class Sword : NetworkBehaviour
{
    [Header("Combat Stats")]
    [SerializeField] float dmg = 35;
    [SerializeField] float cooldown = 2;
    [SerializeField] float stamUsage = 20;

    public Stats stats;

    private float actualCooldown;

    public void Initialize(Stats stats)
    {
        AskForOwnerRPC();

        this.stats = stats;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Player")
        {
            Stats hitStats = collision.collider.gameObject.GetComponent<Stats>();

            hitStats.Damage(dmg);
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (stats.UseStamina(stamUsage))
            {

            }
        }
    }

    [Rpc(SendTo.Server)]
    public void AskToAttackRPC()
    {
       MakeAttackRPC(); 
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void MakeAttackRPC()
    {
        //Code To animate
    }

    [Rpc(SendTo.Server)]
    public void AskForOwnerRPC(RpcParams rpcParams = default)
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
