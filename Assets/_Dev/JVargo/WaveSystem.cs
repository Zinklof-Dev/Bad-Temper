using System;
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
    float secs; // Luigi | I did this because i wanted it to be pronounced like you know what.

    static int day = 0;
    static bool waveChanged = false;
    

    public static int _waveCount;

    public NetworkVariable<Int32> waveCount = new NetworkVariable<Int32>(
        value: 0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    private void Start()
    {
        server = GameObject.FindGameObjectWithTag("Server").GetComponent<Server>();
        server.ServerTick += ServerUpdate;
    }

    public static void WaveStart() //Cameron || we don't need an increment for this to be honest, in run time it should only ever increase by one.
    {
        if (!isOwnerStatic)
            return;
        if (!isServerStatic)
            return;

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



        StartDay();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            isOwnerStatic = true;
        if (IsServer)
            isServerStatic = true;

        Shell.RegisterCommand(WAVESTART);
        Shell.RegisterCommand(CURRENTTIME);
        base.OnNetworkSpawn();
    }

    public static void StartDay()
    {
        isDay = true;
        Log.LogResponse("It is daytime " + isDay);
        //hasDayStarted = true;
    }

    public void ServerUpdate()
    {

    }

    public void Update()
    {
        if (waveChanged && IsServer)
        {
            waveCount.Value = _waveCount;
            waveChanged = false;
        }
        secs += Time.deltaTime; //Time.deltaTime is ALWAYS the ammount of seconds the last frame took. -camoron

        if (secs > 5)
        {
            //logik hr
            ChangeDay();
            secs = 0;
        }
    }

    public void ChangeDay()
    {
        if (isDay)
            isDay = false;
        else
            isDay = true;   
    }

    public static void EndWave()
    {
        Debug.LogWarning("EndWave(); not implimented yet");
    }

    public static LegacyCommand WAVESTART = new LegacyCommand("0001x3700000000", "wave.force_start", "This starts the wave", false, () =>
    {
        EndWave();
        WaveStart();
        Log.LogResponse("Force Started wave, now " + _waveCount);
    });

    public static LegacyCommand CURRENTTIME = new LegacyCommand("0001x3700000001", "current_time", "this will tell us the current time of day", false, () =>
        Log.LogResponse(time)
    );
}
