using System;
using Unity;

public class AiHealth : NetworkBehavior
{
    [SerializeField] maxHP;
    [SerializeField] currentHP;
    
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
        if (rpcParams.Receive.ClientId != 0)
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
