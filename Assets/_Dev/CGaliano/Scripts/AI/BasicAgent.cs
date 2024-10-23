using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using ZinklofDev.Utils.MathZ;

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

    [Header("Debug Display")]
    [SerializeField] debugShowState state = new debugShowState(0, "idle");
    [SerializeField] GameObject target;
    [SerializeField] bool VerboseLogging;
    [Header("Agent Settings")]
    [SerializeField] side team = BasicAgent.side.Hostile;
    [Space(5)]
    [SerializeField] bool attacksCampfire = true;
    [SerializeField] bool attacksPlayer = true;
    [SerializeField] bool attacksStructures = true;
    [SerializeField] bool attacksHostileAI = false;
    [SerializeField] bool attacksFriendyAI = true;
    [SerializeField] bool attacksNeutralAI = false;
    [SerializeField] bool attacksPassiveAI = false;
    [Space(5)]
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

    private Vector3 memoryPOS = Vector3.zero;

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
    }

    private void Start()
    {
        if (VerboseLogging)
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

        server.AITick += AIUpdate; AVL("subscribing AIUpdate() to AITick event");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerEngagementDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDisengangeDistance);
        Gizmos.color = new Color(0.058f, 1, 1);
        if (target != null)
        Gizmos.DrawLine(transform.position, target.transform.position);
    }

    private void AIUpdate()
    {
        AVL("AI update function");
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
    }

    private void AVL(string input) //Agent Verbose Log, avoiding ambiguity with the verbose logging function from the test manager
    {
        if (!VerboseLogging) return;
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

        if (Vectors.SqrDist3f(target.transform.position, transform.position) < Numbers.Sqr(playerDisengangeDistance))
        {
            AVL("setting desitination to player");
            agent.SetDestination(target.transform.position);
        }
        else
        {
            AVL("best player is too far!");
        }

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
        agent.destination = memoryPOS;
        if (Vectors.SqrDist3f(agent.destination, transform.position) >= Numbers.Sqr(0.5f))
        {
            AVL("still not at end of idle path");
            return;
        }
        else
        {
            AVL("reached end of path for idle state");
            CheckForStateChanges();
        }
    }

    private void WanderState()
    {
        AVL("wander state");
        //checking if we need to exit our wander state
        CheckForStateChanges();

        Vector3 wanderPoint = Vector3.zero;
        int fails = 0;

        if (Vectors.SqrDist3f(agent.destination, transform.position) <= Numbers.Sqr(0.5f))
        {
            AVL("finding new wander position");
            while (wanderPoint != Vector3.zero && fails <= 10)
            {
                float x = UnityEngine.Random.Range(-5, 5);
                float z = UnityEngine.Random.Range(-5, 5);

                //Saving private Y
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(transform.position.x + x, 500, transform.position.z + z), new Vector3(0, -90, 0), out hit, 1000))
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

        agent.destination = wanderPoint;
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
            if (target != null)
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
}
