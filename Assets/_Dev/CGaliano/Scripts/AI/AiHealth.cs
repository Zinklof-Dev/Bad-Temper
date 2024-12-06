using System;
using Unity.Netcode;
using UnityEngine;

public class AiHealth : NetworkBehaviour
{
    [SerializeField] float maxHP;
    [SerializeField] float currentHP;
    
    private bool runServerCode = true;
    
    public void Start()
    {
        if (!IsServer)
        {
            return;
        }
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer)
        {
            return;
        }

        if (collision.gameObject.tag == "Weapon")
        {
            collision.gameObject.GetComponent<Sword>();
        }    
    }

    [Rpc(SendTo.ClientsAndHost)]
    void DamageAiRpc(float dmg, RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != 0)
        {
            return;
        }
        else
        {
            currentHP -= dmg;
            if (currentHP <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
