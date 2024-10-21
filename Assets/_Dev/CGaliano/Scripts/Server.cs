using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Server : NetworkBehaviour
{
    [Header("Tick Debug Display")]
    [SerializeField] float serverTicksPerSecond;
    [SerializeField] float aiTicksPerSecond;
    [Header("Tick Settings")]
    [SerializeField] int targetServerTicksPerSecond;
    [SerializeField] int targetAiTicksPerSecond;
    [SerializeField] int minServerTicksPerSecond;
    [SerializeField] int minAiTicksPerSecond;
    [Space(5)]
    [SerializeField] bool logTicks;

    List<Action> ActionQueue = new List<Action>();

    public delegate void ServerEventManger();
    public event ServerEventManger ServerTick;
    public event ServerEventManger AITick;

    float timeSinceLastServerTick;
    float timeSinceLastAiTick;

    float timeBetweenServerTicks;
    float timeBetweenAITicks;

    //deltaTimes for the ticks
    static public float ServerDeltaTime;
    public static float AIDeltaTime;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        timeBetweenServerTicks = 1 / targetServerTicksPerSecond;
        timeBetweenAITicks = 1 / targetAiTicksPerSecond;
    }

    /// <summary>
    /// This function queues an action to be run by the server next server tick. you probably wont have a reason to use this with the server tick event.
    /// however, if you need it, please know that it will loose any references you have and thus is severely limited, if you really need to queue
    /// actions to be done on a server tick, it may not be as performant but making a queue within your own script is probably best for you.
    /// </summary>
    /// <param name="action">the action to be invoked next server tick</param>
    public void QueueServerAction(Action action)
    {
        //this should allow scripts runnning more often than server ticks to request the server does something next tick and still get all of them/it done
        //might go pretty unused
        ActionQueue.Add(action);
    }

    private void InternalServerTick()
    {
        ServerDeltaTime = timeSinceLastServerTick;

        if (logTicks)
        {
            Debug.Log("Server Tick");
        }

        //Run through the action Queue
        try
        {
            if (ActionQueue.Count < 0)
            {
                foreach (Action a in ActionQueue)
                {
                    a.Invoke();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        //Should always be at the end to ensure the server does all its stuff before other scripts run with the server tick.
        ServerTick?.Invoke();
    }

    private void InternalAITick()
    {
        AIDeltaTime = timeSinceLastAiTick;

        if (logTicks)
        {
            Debug.Log("AI Tick");
        }

        //should always be at the end to ensure the server does all other required stuff before other scripts run with the AI tick
        AITick?.Invoke();
    }

    private void ChangeTickRate()
    {
        float deltAsFPS = 1 / Time.deltaTime;

        float percentage = deltAsFPS / 60;

        serverTicksPerSecond = Mathf.Clamp((targetServerTicksPerSecond * percentage), minServerTicksPerSecond, targetServerTicksPerSecond);
        aiTicksPerSecond = Mathf.Clamp((targetAiTicksPerSecond * percentage), minAiTicksPerSecond, targetAiTicksPerSecond);

        timeBetweenServerTicks = 1 / serverTicksPerSecond;
        timeBetweenAITicks = 1 / aiTicksPerSecond;
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        timeSinceLastAiTick += Time.deltaTime;
        timeBetweenServerTicks += Time.deltaTime;

        if (timeSinceLastServerTick > timeBetweenServerTicks)
        {
            InternalServerTick();
        }
        if (timeSinceLastAiTick > timeBetweenAITicks)
        {
            InternalAITick();
        }

        ChangeTickRate();
    }
}
