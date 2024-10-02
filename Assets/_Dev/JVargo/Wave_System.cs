using System;
using Unity.Netcode;
using UnityEngine;
using ZinklofDev.Console;
using ZinklofDev.Utils.Testing;

public class Wave_System : NetworkBehaviour
{
    public NetworkVariable<Int32> waveCount = new NetworkVariable<Int32>(
        value: 0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    public void increaseWaveCount(int incremental)
    {
        if (!IsOwner) 
            return;
        if (!IsServer)
            return;

        waveCount.Value += incremental;
        Debug.Log(waveCount.Value);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    public void Update()
    {
        Debug.Log(waveCount);
    }

    public static Test WaveManagerIncramentTest = new Test("Wave_System.cs", () =>
    {
      //   int wavecount = increaseWaveCount("");

       // WaveManagerIncramentTest.Expect();
    });
    
}
