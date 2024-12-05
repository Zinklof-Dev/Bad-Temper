using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayersManager : NetworkBehaviour
{
    [SerializeField] ulong[] slots = new ulong[6];
    [SerializeField] FixedString32Bytes[] usernames = new FixedString32Bytes[6];
    [SerializeField] ulong clientId;

    private void Start()
    {
        if (IsServer)
        {
            for(int i = 0; i <6; i++)
            {
                slots[i] = (ulong)0;
            }
            for(int i = 0; i <6; ++i)
            {
                usernames[i] = (FixedString32Bytes)"NoPlAyEr";
            }
        }

        if (IsOwner)
        {
            AskForIdRpc();
        }
    }

    [Rpc(SendTo.Server)]
    void AskForIdRpc(ServerRpcParams rpcParams = default)
    {
        clientId = rpcParams.Receive.SenderClientId;

        foreach (var slot in slots)
        {
            if (slots[slot] != 0)
            {
                slots[slot] = clientId;
            }
        }

        ReturnIdtoClientRpc(clientId, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.Server)]
    void GiveServerUsernameRpc(FixedString32Bytes username, RpcParams rpcParams)
    {
        clientId = rpcParams.Receive.SenderClientId;
        bool nameSaved = false;


        for (int i = 0; i < 6; i++)
        {
            if (slots[i] == clientId)
            {
                usernames[i] = username;
                nameSaved = true;
            }

            if (nameSaved == false)
            {
                Debug.LogError("failed to save username, canceling RPC");
                return;
            }
            else
            {

            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SendUsernameAcrossNetworkRpc(FixedString32Bytes username, int index)
    {
        //need logic
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnIdtoClientRpc(ulong returnedId, RpcParams rpcParams = default) 
    {
        clientId = returnedId;
    }
}
