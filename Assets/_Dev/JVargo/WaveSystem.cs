using System;
using Unity.Netcode;
using UnityEngine;
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
        base.OnNetworkSpawn();
    }
    public void Update()
    {
        Debug.Log(waveCount);
    }

    //public static Test WaveManagerIncramentTest = new Test("Wave_System.cs", () =>
    //{
    //});
    
}
