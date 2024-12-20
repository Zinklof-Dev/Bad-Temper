using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using TMP;

public class LoadingManager : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] Canvas _LoadingCanvas;
    [SerializeField] TextMeshPro _LoadingText;
    [SerializeFIeld] Slider _LoadingSlider;
    [Header("Script References")]
    [SerializeField] TreeGeneration _TreeGen;
    [SerializeField] TerrainGeneration _TerrainGen;

    private float currentBarValue;
    private float wantedBarValue;
    private int totalSteps;
    private int stepsComplete;

    private float timeElapsed;
    private float minTimeElapsed;

    private bool _ServerHasSeed = false;
    int _Seed;

    public override void OnNetworkSpawn()
    {
        
    
        if (IsServer)
        {
            _Seed = UnityEngine.Random.Range(0, 99999);
            _ServerHasSeed = true;
            StartWorldGeneration();
        }
        else
        {
            AskForSeedRpc();
        }

        base.OnNetworkSpawn();
    }

    private void PreLoadChecklist()
    {
        minTimeElapsed = UnityEngine.Randim.Range(28,32);
        timeElapsed = 0;
    }

    public void FinishStep(string nextStepText)
    {
        stepsComplete++;
        _LoadingText.Text = text;
    }

        private async void EvalateBar()
    {
        currentBarValue += (wantedBarValue - currentBarValue) * 0.1f; //get 10% closer to the wanted value every evaluation;
    }

    private async void StartWorldGeneration()
    {
        await _TerrainGen.Initialize(_Seed);
        await _TreeGen.Initialize(_Seed);
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
