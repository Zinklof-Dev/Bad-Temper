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
    static bool isOwnerStatic = false;
    static bool isServerStatic = false;
    static bool isDay = false;
    static float timeOfDay;
    static string time;
    
    bool isDivisible;
    bool hasDayBeenSet = false;
    static int day = 0;
    static bool hasDayStarted = false;
    

    static bool waveChanged = false;

    public static int _waveCount;

    public NetworkVariable<Int32> waveCount = new NetworkVariable<Int32>(
        value: 0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );
    
    public static void WaveStart() //Cameron || we don't need an increment for this to be honest, in run time it should only ever increase by one.
    {
        waveChanged = true;
        // Cole | 10/18/24 | Compiler error here --->
        // Not how network variables work smh

        // where is singapore - Lucas

        // Cameron | I thought i removed lucas's access to the repository when he left the team? (i also fixed these all being misaligned by one space)

        // var wave = NetworkVariable<Int32>.waveCount; <---

        if (!isOwnerStatic)
            return;
        if (!isServerStatic)
            return;

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
        hasDayStarted = true;
    }

    public void Update()
    {
        if (waveChanged && IsServer)
        {
            waveCount.Value = _waveCount;
            waveChanged = false;
        }
        timeOfDay += Time.deltaTime;
        int mins = Mathf.FloorToInt(timeOfDay / 60);
        int secs = Mathf.FloorToInt(timeOfDay % 60);
        

        if (hasDayStarted)
        {
            

            time = string.Format("{0:00} : {1:00}", mins, secs);

            if (secs % 5 == 0)
            {

                day++;
                if(!hasDayBeenSet)
                ChangeDay();
                
            }
            else
                hasDayBeenSet = false;
            Debug.Log(day);
        }         
    }

    public void ChangeDay()
    {
        if (isDay)
            isDay = false;
        else
            isDay = true;   
        hasDayBeenSet = true;
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
