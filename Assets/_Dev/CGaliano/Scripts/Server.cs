using JetBrains.Annotations;
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
    [SerializeField] float targetedSTPS;
    [SerializeField] float targetedATPS;
    [SerializeField] float actualSTPS;
    [SerializeField] float actualATPS;
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
    static public float serverDeltaTime;
    public static float aiDeltaTime;

    //to debug the actual TPS
    int stps;
    int atps;
    float secondTracker; //will use time.delta time to tell when a second has passed

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

    //this was not accurate enough for my use case, it also doesn't seem to save much compute time over my current solution (both report <1ms)
    public static bool TestingModulusRateLimiting(int freq)
    {
        if (Time.frameCount % freq == 0)
        {
            return true;
        }
        return false;
    }

    private void InternalServerTick()
    {
        serverDeltaTime = timeSinceLastServerTick;

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
        aiDeltaTime = timeSinceLastAiTick;

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

        targetedSTPS = Mathf.Clamp((targetServerTicksPerSecond * percentage), minServerTicksPerSecond, targetServerTicksPerSecond);
        targetedATPS = Mathf.Clamp((targetAiTicksPerSecond * percentage), minAiTicksPerSecond, targetAiTicksPerSecond);

        timeBetweenServerTicks = 1 / targetedSTPS;
        timeBetweenAITicks = 1 / targetedATPS;
    }

    private void Update()
    {
        //DateTime start = DateTime.Now;
        if (!IsServer)
        {
            return;
        }

        timeSinceLastAiTick += Time.deltaTime;
        timeSinceLastServerTick += Time.deltaTime;
        secondTracker += Time.deltaTime;

        if (secondTracker > 1f)
        {
            actualSTPS = secondTracker / serverDeltaTime;
            actualATPS = secondTracker / aiDeltaTime;

            atps = 0; stps = 0; secondTracker = 0;
        }

        if (timeSinceLastServerTick > timeBetweenServerTicks)
        {
            InternalServerTick();
            stps++;
            timeSinceLastServerTick = 0;
        }
        if (timeSinceLastAiTick > timeBetweenAITicks)
        {
            InternalAITick();
            atps++;
            timeSinceLastAiTick = 0;
        }

        /*if (TestingModulusRateLimiting(targetServerTicksPerSecond))
        {
            InternalServerTick();
            stps++;
            timeSinceLastServerTick = 0;
        }
        if (TestingModulusRateLimiting(targetAiTicksPerSecond))
        {
            InternalAITick();
            atps++;
            timeSinceLastAiTick = 0;
        }*/

        ChangeTickRate();
        //TimeSpan cost = DateTime.Now - start;
        //Debug.Log(cost.TotalMilliseconds);
    }
}
