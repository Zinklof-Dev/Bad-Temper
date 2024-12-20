using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using TMP;

public class LoadingManager : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject _LoadingCanvas;
    [SerializeField] TextMeshProUGUI _LoadingText;
    [SerializeField] TextMeshProUGUI _TipText;
    [SerializeField] Slider _LoadingSlider;
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
    private int _Seed;

    private string[] loadingTips = {
    "This is a loading tip!",
    "Need a Dispenser here!",
    ""
    }

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
        minTimeElapsed = UnityEngine.Random.Range(28,32);
        timeElapsed = 0;
    }

    public void FinishStep(string nextStepText)
    {
        stepsComplete++;
        wantedBarValue = stepsComplete/totalSteps;
        _LoadingText.Text = text;
    }

    private void EvalateBar()
    {
        currentBarValue += (wantedBarValue - currentBarValue) * (25 * Time.deltaTime); //get 10% closer to the wanted value every evaluation;
        if (currentBarValue > 0.98f && wantedBarValue >= 1)
        {
            currentBarValue = 0.99f;
        }

        _LoadingSlider.Value = currentBarValue;
    }

    private void ChangeLoadingTip()
    {
        
    ]

    private void update()
    {
        timeElapsed += Time.deltaTime;
        EvaluateBar();

        if (wantedBarValue >= 1 && timeElapsed > minTimeElapsed)
        {
            _LoadingSlider = null;
            _LoadingText = null;
            Destroy(_LoadingCanvas);
            //code to teleport player, player script needs updated
            Destroy(this);
        }
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
