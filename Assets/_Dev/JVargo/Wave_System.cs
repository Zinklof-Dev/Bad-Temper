using System;
using Unity.Netcode;
using UnityEngine;
using ZinklofDev.Console;

public class Wave_System : NetworkBehaviour
{
    public NetworkVariable<Int32> waveCount = new NetworkVariable<Int32>(
        value: 0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {



        base.OnNetworkSpawn();
    }
}
