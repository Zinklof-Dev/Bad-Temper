using System;
using UnityEngine;
using Unity.Netcode;

public class Sword : NetworkBehaviour
{
    [Header("Combat Stats")]
    [SerializeField] float dmg = 35;
    [SerializeField] float cooldown = 4;
    [SerializeField] float stamUsage = 20;

    [SerializeField] Animator animator;

    public Stats stats;

    private float actualCooldown;

    public void Initialize(Stats stats)
    {
        AskForOwnerRPC();

        this.stats = stats;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Stats hitStats = other.gameObject.GetComponent<Stats>();

            hitStats.Damage(dmg);
        }
    }

    public void Update()
    {
        if (!IsOwner) return;

        actualCooldown += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Mouse0) && actualCooldown > cooldown)
        {
            if (stats.UseStamina(stamUsage))
            {
                actualCooldown = 0;
                AskToAttackRPC();
                animator.SetTrigger("Swing");
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
        if (IsOwner)
        {
            return;
        }
        animator.SetTrigger("Swing");
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
