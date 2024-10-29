using System;
using Unity.Netcode;
using UnityEngine;
using ZinklofDev.Console;

public class WaveSystem : NetworkBehaviour
{
    // Unused
    public delegate void WaveSystemEventManager();
    public static event WaveSystemEventManager TestServerTick; // Cameron | I love how this keeps chucking a warning at me in editor because its still unused KEK

    [Header("Client Side Refrences")]
    [SerializeField] public bool clientRefIsDay;
    [SerializeField] public Int32 clientRefWaveCount;

    [Header("Only Needed With Server")]
    [SerializeField] Server server;
    [SerializeField] float secs; // Luigi | I did this because i wanted it to be pronounced like you know what.
    [SerializeField] float dayLength;

    [Header("Network Varibles")]
    public NetworkVariable<Int32> waveCount = new NetworkVariable<Int32>(
        value: 0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    public NetworkVariable <bool> isDay = new NetworkVariable<bool>(
        value: false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    static bool waveChanged = false;
    static bool isOwnerStatic = false;
    static bool isServerStatic = false;
    static bool _isDay = false;
    static string time;
    static Int32 _waveCount;

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

        waveCount.OnValueChanged += OnWaveCountChange;
        isDay.OnValueChanged += OnIsDayChange;


        Shell.RegisterCommand(WAVESTART);
        Shell.RegisterCommand(CURRENTTIME);
        Shell.RegisterCommand(ENDWAVE);
        base.OnNetworkSpawn();
    }

    void OnWaveCountChange(Int32 previousValue, Int32 newValue)
    {
        clientRefWaveCount = newValue;
    }

    void OnIsDayChange(bool previousValue, bool newValue)
    {
        clientRefIsDay = newValue;
    }

    public static void WaveStart() //Cameron || we don't need an increment for this to be honest, in run time it should only ever increase by one.
    {
        if (!isOwnerStatic)
            return;
        if (!isServerStatic)
            return;

        _isDay = false;
        waveChanged = true;

        _waveCount += 1;
    }

    public void ServerUpdate()
    {
        if (waveChanged)
        {
            waveCount.Value = _waveCount;
            waveChanged = false;
            EndWave();
        }

        if (!_isDay)
        {
            secs += Server.serverDeltaTime;
            if (secs >= dayLength)
            {
                WaveStart();
                secs = 0;
                waveChanged = true;
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
        _isDay = true;
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
