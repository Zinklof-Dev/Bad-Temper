using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class LoadingManager : NetworkBehaviour
{
    [SerializeField] string _LoadingString;

    [SerializeField] TreeGeneration _TreeGen;
    [SerializeField] TerrainGeneration _TerrainGen;

    private bool _ServerHasSeed = false;
    int _Seed = 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _ServerHasSeed = true;
            StartWorldGeneration();
        }
        else
        {
            AskForSeedRpc();
        }

        base.OnNetworkSpawn();
    }

    private async void StartWorldGeneration()
    {
        await _TerrainGen.Initialize(_Seed);
        await _TreeGen.Initialize(_Seed);
    }

    public void UpdateString(string text)
    {
        _LoadingString = text;
        //code to update GUI
    }

    private async void AskAgain()
    {
        await Task.Delay(1000); // wait 500 ms, aka 0.5 secconds
        AskForSeedRpc(); // ask again
    }

    [Rpc(SendTo.Server)]
    private void AskForSeedRpc(RpcParams rpcParams = default)
    {
        ulong clientID = rpcParams.Receive.SenderClientId; // get client ID
        if (!_ServerHasSeed) // if the server doesn't yet have the seed then deny the clients request
        {
            DenySeedRequestRpc(RpcTarget.Single(clientID, RpcTargetUse.Temp));
            return;
        }
        else // otherwise provide the seed
            SendSeedRpc(_Seed, RpcTarget.Single(clientID, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SendSeedRpc(int seed, RpcParams rpcParams = default)
    {
        this._Seed = seed;
        StartWorldGeneration();
        
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void DenySeedRequestRpc(RpcParams rpcParams = default) // the server has denied our request. so lets wait and ask again
    {
        AskAgain();
    }
}
