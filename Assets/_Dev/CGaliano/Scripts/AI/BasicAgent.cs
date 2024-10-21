using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class BasicAgent : NetworkBehaviour
{
    [Serializable]
    struct debugShowState
    {
        public int stateID;
        public string stateName;

        public debugShowState(int stateID, string stateName)
        {
            this.stateID = stateID;
            this.stateName = stateName;
        }
    }

    enum side
    {
        Friendly,
        Hostile,
        Neutral,
        Passive
    }

    [Header("Debug Display")]
    [SerializeField] debugShowState state;
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
    [Header("References")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Server server;

    debugShowState wander = new debugShowState(0, "Wander");
    debugShowState campfire = new debugShowState(1, "Campfire");
    debugShowState player = new debugShowState(2, "Player");
    debugShowState structure = new debugShowState(3, "Structure");
    debugShowState ilde = new debugShowState(4, "idle");
    debugShowState ai = new debugShowState(5, "ai");
    debugShowState pet = new debugShowState(6, "pet");

    private void OnValidate()
    {
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

    private void Update()
    {
        
    }



}
