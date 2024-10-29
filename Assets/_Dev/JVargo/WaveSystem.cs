using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using ZinklofDev.Console;

public class WaveSystem : NetworkBehaviour
{
    public delegate void WaveSystemEventManager();
    public static event WaveSystemEventManager TestServerTick; // Cameron | I love how this keeps chucking a warning at me in editor because its still unused KEK
    [SerializeField] Server server;
    static bool isOwnerStatic = false;
    static bool isServerStatic = false;
    static bool isDay = false;
    static string time;
    [SerializeField] float secs; // Luigi | I did this because i wanted it to be pronounced like you know what.
    [SerializeField] float dayLength;
    static bool waveChanged = false;
    

    public static int _waveCount;

    public NetworkVariable<Int32> waveCount = new NetworkVariable<Int32>(
        value: 0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

        // Cole | You were checking if the client was a host before the network was even created, I fixed it for you by rearranging your Start function to execute in the OnNetworkSpawn function.

        public override void OnNetworkSpawn()
    {
        if (IsOwner)
            isOwnerStatic = true;
            
        if (IsServer)
        {
            isServerStatic = true;
            server = GameObject.FindGameObjectWithTag("Server").GetComponent<Server>();
            server.ServerTick += ServerUpdate;
        }

        waveCount.OnValueChanged += OnWaveChangeVariableChange;

        Shell.RegisterCommand(WAVESTART);
        Shell.RegisterCommand(CURRENTTIME);
        Shell.RegisterCommand(ENDWAVE);
        base.OnNetworkSpawn();
    }

    void OnWaveChangeVariableChange(Int32 previousValue, Int32 newValue)
    {
        // Cole | Clients can't write to this variable, should probably create a local Int32 that the client can use. We will write to this variable only on the host.
        waveCount.Value = newValue;
    }

    public static void WaveStart() //Cameron || we don't need an increment for this to be honest, in run time it should only ever increase by one.
    {
        if (!isOwnerStatic)
            return;
        if (!isServerStatic)
            return;

        isDay = false;
        waveChanged = true;
        // Cole | 10/18/24 | Compiler error here --->
        // Not how network variables work smh

        // where is singapore - Lucas

        // Cameron | I thought i removed lucas's access to the repository when he left the team? (i also fixed these all being misaligned by one space)

        // var wave = NetworkVariable<Int32>.waveCount; <---

        _waveCount += 1;

        // Cole | 10/18/24 | and here to --->

        // Cameron | Wrong kind of too ya idiot.

        // wave.UpdateWaveCount(); <---
    }

    public void ServerUpdate()
    {
        if (waveChanged && IsServer)
        {
            waveCount.Value = _waveCount;
            waveChanged = false;
        }

        if (isDay)
        {
            secs += Server.serverDeltaTime;
            if (secs >= dayLength)
            {
                WaveStart();
                secs = 0;
            }
        }
        
    }


    /*public void Update() //Cameron || no no use update funvtion.
    {
        if (waveChanged && IsServer)
        {
            waveCount.Value = _waveCount;
            waveChanged = false;
        }
        ; //Time.deltaTime is ALWAYS the ammount of seconds the last frame took. -camoron

        if (secs > 5)
        {
            //logik hr
            ChangeDay();
            secs = 0;
        }
    }*/

    public static void EndWave()
    {
        Debug.LogWarning("EndWave(); not implimented yet");
        isDay = true;
    }

    

    public static LegacyCommand WAVESTART = new LegacyCommand("0001x3700000000", "wave.force_start", "This starts the wave", true, () =>
    {
        WaveStart();
        Log.LogResponse("Force Started wave, now " + _waveCount);
    });

    public static LegacyCommand CURRENTTIME = new LegacyCommand("0001x3700000001", "current_time", "this will tell us the current time of day", true, () =>
        Log.LogResponse(time)
    );

    public static LegacyCommand ENDWAVE = new LegacyCommand("0001x3700000002", "wave.force_end", "This will force end the wave and turn it to day", true, () =>
        EndWave()
    );
}
