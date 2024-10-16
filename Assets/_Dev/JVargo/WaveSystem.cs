using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using ZinklofDev.Console;
using ZinklofDev.Utils.Testing;

public class WaveSystem : NetworkBehaviour
{
    public delegate void WaveSystemEventManager();
    public static event WaveSystemEventManager TestServerTick;
    

    public NetworkVariable<Int32> waveCount = new NetworkVariable<Int32>(
        value: 0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    public void increaseWaveCount()
    {
        if (!IsOwner) 
            return;
        if (!IsServer)
            return;

        waveCount.Value += 1;
        Debug.Log(waveCount.Value);
    }

    public override void OnNetworkSpawn()
    {
        Shell.RegisterCommand(WAVESTART);

        base.OnNetworkSpawn();
    }
    public void Update()
    {
        //Debug.Log(waveCount);
    }

    public static void WaveStart()
    {
        Debug.Log("Placeholder");
    }

    public void WaveEnd()
    {

    }



    public static Command WAVESTART = new Command("0001x3700000000", "wave_start", "This starts the wave", false, () =>
    {
        WaveStart();
    });
}
