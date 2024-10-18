using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using ZinklofDev.Console;
using ZinklofDev.Utils.Testing;

public class WaveSystem : NetworkBehaviour
{
    public delegate void WaveSystemEventManager();
    public static event WaveSystemEventManager TestServerTick;
    static bool isOwnerStatic = false;
    static bool isServerStatic = false;
 

    static int _waveCount;

    public NetworkVariable<Int32> waveCount = new NetworkVariable<Int32>(
        value: 0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );
    
        public static void increaseWaveCount() //Cameron || we don't need an increment for this to be honest, in run time it should only ever increase by one.
        {
           // Cole | 10/18/24 | Compiler error here
           // Not how network variables work smh
           // where is singapore - Lucas

           // var wave = NetworkVariable<Int32>.waveCount;
        
            if (!isOwnerStatic) 
                return;
            if (!isServerStatic)
                return;

            _waveCount += 1;
            // Cole | 10/18/24 | and here to
            // wave.UpdateWaveCount();
    }

    public void UpdateWaveCount()
    {
        waveCount.Value = _waveCount;
    } 
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            isOwnerStatic = true;
        if (IsServer)
            isServerStatic = true;

        Shell.RegisterCommand(WAVESTART);
        Shell.RegisterCommand(INCWAVE);
        
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

    public static Command INCWAVE = new Command("0001x3700000001", "inc_wave", "This increases the wave by the amount put in", false, () =>
    {
        increaseWaveCount();
        Log.LogResponse("Increased wave, now " + _waveCount);
    });
}
