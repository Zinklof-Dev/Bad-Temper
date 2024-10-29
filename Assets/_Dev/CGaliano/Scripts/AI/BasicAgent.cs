using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using ZinklofDev.Utils.MathZ;
using ZinklofDev.Utils.Testing;

public class BasicAgent : NetworkBehaviour
{
    [Serializable]
    struct debugShowState
    {
        public byte stateID;
        public string stateName;

        public debugShowState(byte stateID, string stateName)
        {
            this.stateID = stateID;
            this.stateName = stateName;
        }
    }

    enum side
    {
        Friendly,
        Hostile,
        //Neutral,
        Passive,
        Custom
    }

    [Header("Debug Display (DONT TOUCH YOURSELF)")]
    [SerializeField] debugShowState state = new debugShowState(0, "idle");
    [SerializeField] GameObject target;
    [SerializeField] Vector3 destination = Vector3.zero;
    [SerializeField] Vector3 memoryPOS = Vector3.zero;
    [Header("Debug Settings")]
    [SerializeField] bool verboseLogging = false;
    [SerializeField] bool drawGizmos = false;
    [Space(5)]
    [Header("Agent Settings")]
    [SerializeField] side team = BasicAgent.side.Hostile;
    [SerializeField] AnimationCurve jumpCurve = new AnimationCurve();
    [Space(5)]
    [SerializeField] bool attacksCampfire = true;
    [SerializeField] bool attacksPlayer = true;
    [SerializeField] bool attacksStructures = true;
    [SerializeField] bool attacksHostileAI = false;
    [SerializeField] bool attacksFriendyAI = true;
    [SerializeField] bool attacksNeutralAI = false;
    [SerializeField] bool attacksPassiveAI = false;
    [Space(5)]
    [SerializeField] bool handleAgentDrifting;
    [Space(15)]
    [Header("Wander Settings")]
    [SerializeField] float wanderDistance = 25;
    [SerializeField] bool wanderAroundLastPlayerPOS = true;
    [SerializeField] bool setDefaultWanderPointToSpawn = true;
    [SerializeField] Vector3 defaultWanderPoint;
    [Header("Player Engagement Settings")]
    [SerializeField] float playerMaxPriority;
    [SerializeField] float playerMinPriority;
    [Space(5)]
    [SerializeField] float playerEngagementDistance;
    [SerializeField] float playerDisengangeDistance;
    [SerializeField] float playerStoppingDistance;
    [Space(5)]
    [SerializeField] bool playerOverrideMovespeed = false;
    [SerializeField] float playerMovespeed;
    [Space(15)]
    [Header("References (set by themselves, don't touch)")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Server server;
    [SerializeField] List<GameObject> Players = new List<GameObject>();

    debugShowState wander = new debugShowState(0, "Wander");
    debugShowState campfire = new debugShowState(1, "Campfire");
    debugShowState player = new debugShowState(2, "Player");
    debugShowState structure = new debugShowState(3, "Structure");
    debugShowState idle = new debugShowState(4, "idle");
    debugShowState ai = new debugShowState(5, "ai");
    debugShowState pet = new debugShowState(6, "pet");

    private int AIN; //Agent Identification Number

    private void OnValidate()
    {
        if (team == side.Hostile)
        {
            attacksCampfire = true;
            attacksPlayer = true;
            attacksStructures = true;
            attacksHostileAI = false;
            attacksFriendyAI = true;
            attacksNeutralAI = false;
            attacksPassiveAI = false;
        }
        if (team == side.Friendly)
        {
            attacksCampfire = false;
            attacksPlayer = false;
            attacksStructures = false;
            attacksHostileAI = true;
            attacksFriendyAI = false;
            attacksNeutralAI = false;
            attacksPassiveAI = false;
        }
        if (team == side.Passive)
        {
            attacksCampfire = false;
            attacksPlayer = false;
            attacksStructures = false;
            attacksHostileAI = false;
            attacksFriendyAI = false;
            attacksNeutralAI = false;
            attacksPassiveAI = false;
        }

        if (handleAgentDrifting)
        {
            Debug.LogWarning("Handle Agent Drifting is a patchwork solution to prevent faster agents from drifitng without having to mess with NV agent settings yourself, this will force lock the agent acceleration to max, solving the issue but making it so your AI reach top speed immedietly.");
        }
    }

    private void Awake()
    {
        if (verboseLogging)
        AIN = UnityEngine.Random.Range(0, 999999);
        AVL("Hi i am an AI Agent from the BasicAgent class! i have assigned my ID as " + AIN + ", please note that this ID is not stored anywhere and two agents can share the same ID");

        agent = gameObject.GetComponent<NavMeshAgent>(); AVL("Getting my own NV agent class");
        server = GameObject.FindGameObjectWithTag("Server").GetComponent<Server>(); AVL("Finding server object");
        GameObject[] PlayerObjects = GameObject.FindGameObjectsWithTag("Player"); AVL("Creating array of all current players");
        foreach (GameObject obj in PlayerObjects)
        {
            AVL("turning array into list");
            Players.Add(obj);
        }

        agent.autoTraverseOffMeshLink = false;
        agent.acceleration = 999f;
        agent.destination = transform.position;
        server.AITick += AIUpdate; AVL("subscribing AIUpdate() to AITick event");

        if (setDefaultWanderPointToSpawn)
        {
            defaultWanderPoint = transform.position;
        }
        memoryPOS = defaultWanderPoint;
    }

    IEnumerator Start()
    {
        if (!IsServer)
        {
            yield return null;
        }
        while (true)
        {
            if (agent.isOnOffMeshLink)
            {
                yield return StartCoroutine(Curve(agent, 0.5f));
                agent.CompleteOffMeshLink();
            }
            yield return null;
        }
    }

    IEnumerator Curve(NavMeshAgent agent, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = jumpCurve.Evaluate(normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos)
        {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerEngagementDistance);
        //if (agent != null)
        //Gizmos.DrawSphere(agent.destination, 0.25f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, playerDisengangeDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(memoryPOS, 0.20f);
        Gizmos.color = new Color(0.058f, 1, 1);
        if (agent != null)
        {
            Gizmos.DrawLine(transform.position, agent.destination);
            Gizmos.DrawSphere(agent.destination, 0.25f);
        }
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), Numbers.Sqr(.5f));
        Gizmos.color = new Color(0f, 0f, 1f, 0.25f);
        Gizmos.DrawCube(defaultWanderPoint, new Vector3(wanderDistance * 2, 1000, wanderDistance * 2));
        Gizmos.DrawSphere(defaultWanderPoint, 0.25f);
        Gizmos.DrawLine(transform.position, defaultWanderPoint);
    }

    private void AIUpdate()
    {
        AVL("AI update function");

        UnreachablePathSaver();

        switch (state.stateID)
        {
            case 0:
                WanderState(); break;
            case 1:
                break;
            case 2:
                PlayerState(); break;
            case 3:
                break; 
            case 4:
                IdleState(); break; 
            case 5: 
                break; 
            case 6: 
                break;
        }

        destination = agent.destination;
    }

    private void AVL(string input) //Agent Verbose Log, avoiding ambiguity with the verbose logging function from the test manager
    {
        if (!verboseLogging) return;
        else Debug.Log("[" + AIN + "] " + input);
    }

    private void ChangeState(byte ID)
    {
        AVL("Entered into ChangeState Function");
        switch (ID)
        {
            case 0:
                state = wander; 
                AVL("ChangeState case 0 true, changing state to wander"); 
                break;
            case 1:
                state = campfire;
                AVL("ChangeState case 1 true, changing state to campfire");
                break;
            case 2:
                state = player;
                AVL("ChangeState case 2 true, changing state to player");
                agent.stoppingDistance = playerStoppingDistance; 
                if (playerOverrideMovespeed) 
                    agent.speed = playerMovespeed; 
                break;
            case 3:
                state = structure;
                AVL("ChangeState case 3 true, changing state to structure"); 
                break;
            case 4:
                state = idle;
                AVL("ChangeState case 4 true, changing state to idle"); 
                break;
            case 5:
                state = ai;
                AVL("ChangeState case 5 true, changing state to ai"); 
                break;
            case 6:
                state = pet;
                AVL("ChangeState case 6 true, changing state to pet"); 
                break;
        }
    }

    private void UnreachablePathSaver()
    {
        if (agent.pathPending && agent.isPathStale)
        {
            AVL("Agent Could not reach path, reseting to wander and clearing last wander path attempt");
            agent.destination = transform.position;
            ChangeState(0);
        }
    }

    private void PlayerState()
    {
        AVL("player state");
        if (target == null)
        {
            FindNewPlayer();
            AVL("null target in player state, trying to find player");
        }
        if (target == null)
        {
            AVL("could not find a player, changing to wander state");
            ChangeState(0);
            return;
        }

        AVL("setting desitination to player");
        agent.destination = target.transform.position;

        if (Vectors.SqrDist3f(target.transform.position, transform.position) > Numbers.Sqr(playerDisengangeDistance))
        {
            ChangeState(4); AVL("player got too far, disengaging and going to idle state");
        }

        AVL("Saving player current pos to memory");
        memoryPOS = target.transform.position;
    }

    private void IdleState()
    {
        AVL("idle state");
        if (agent.destination == memoryPOS)
        {
            agent.destination = memoryPOS;
        }
        AVL(agent.destination + " VS " + new Vector3(transform.position.x, transform.position.y - 1, transform.position.z));
        if (Vectors.SqrDist3f(agent.destination, new Vector3(transform.position.x, transform.position.y - 1, transform.position.z)) >= Numbers.Sqr(0.5f))
        {
            AVL("still not at end of idle path");
            if (verboseLogging)// this might seem redundant given the AVL function already does this, but its to stop the math from even happening in the first place
            {
                AVL("" + Vectors.SqrDist3f(agent.destination, new Vector3(transform.position.x, transform.position.y - 1, transform.position.z)) + " is not within " + Numbers.Sqr(0.5f) + " units");
            }
            return;
        }
        else
        {
            AVL("reached end of path for idle state");
            ChangeState(0);
        }
    }

    private void WanderState()
    {
        AVL("wander state");
        //checking if we need to exit our wander state
        if(CheckForStateChanges())
        {
            return;
        }
        if (wanderAroundLastPlayerPOS && defaultWanderPoint != memoryPOS)
        {
            defaultWanderPoint = memoryPOS;
        }

        Vector3 wanderPoint = Vector3.zero;
        int fails = 0;

        if (Vectors.SqrDist3f(agent.destination, new Vector3(transform.position.x, transform.position.y - 1, transform.position.z)) <= Numbers.Sqr(0.5f))
        {
            AVL("finding new wander position");
            while (wanderPoint == Vector3.zero && fails <= 10)
            {
                float x = UnityEngine.Random.Range(-wanderDistance, wanderDistance);
                float z = UnityEngine.Random.Range(-wanderDistance, wanderDistance);

                //Saving private Y
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(defaultWanderPoint.x + x, 500, defaultWanderPoint.z + z), new Vector3(0, -90, 0), out hit, 1000))
                {
                    AVL("found point, trying to path to point now");
                    wanderPoint = hit.point;
                }
                AVL("failed to find point, trying again");
                fails++;
                if (fails == 10)
                {
                    AVL("failed too many times, stopping finding new wander position till next ai tick");
                }
            }
        }

        if(wanderPoint != Vector3.zero)
        {
            agent.destination = wanderPoint;
        }
    }

    private bool CheckForStateChanges()
    {
        AVL("Checking for state changes");
        if (attacksCampfire)
        {
            AVL("decided to try to change to campfire");
            ChangeState(1);
            return true;
        }
        else if (attacksPlayer)
        {
            AVL("decided to try to change to player");
            target = null;
            FindNewPlayer();
            if (target != null && Vectors.SqrDist3f(target.transform.position, transform.position) <= Numbers.Sqr(playerEngagementDistance))
            {
                ChangeState(2);
                return true;
            }
        }
        AVL("found no reason to change states");
        return false;
    }

    private void FindNewPlayer()
    {
        AVL("Entered find new player");

        //should probably down the line find a way to only run this when we know the player count has changed... maybe... -cameron
        //yes will for sure do this, realizing how expensive this is to run every tick KEK.
        GameObject[] PlayerObjects = GameObject.FindGameObjectsWithTag("Player"); AVL("Creating new array of all current players");
        foreach (GameObject obj in PlayerObjects)
        {
            Players.Clear();
            AVL("turning new array into list");
            Players.Add(obj);
        }

        GameObject bestTarget = null;
        float lastSqrDist = 99999f;
        foreach (GameObject obj in Players)
        {
            AVL("find new player foreach loop");
            float tempSqrdist = Vectors.SqrDist3f(transform.position, obj.transform.position);

            if (tempSqrdist < lastSqrDist && tempSqrdist < Numbers.Sqr(playerEngagementDistance))
            {
                AVL("Found a new best target");
                bestTarget = obj;
                lastSqrDist = tempSqrdist;
            }
        }
        target = bestTarget;
        AVL("Finished find new player");
    }

    static public Test distanceFunction = new Test("BasicAgent.cs", ()=>
    {
        float x = 0;

        x = Vectors.SqrDist3f(new Vector3(0,0,0), new Vector3(0,0,0));

        distanceFunction.Expect(x, 0f);

        x = Vectors.SqrDist3f(new Vector3(3,0,2), new Vector3(3,1,2));
        
        distanceFunction.Expect(x, 1f);

        x = Vectors.SqrDist3f(new Vector3(100, 67, 8), new Vector3(0, 0, 0));

        distanceFunction.Expect(x, 14553f);
    });
}
