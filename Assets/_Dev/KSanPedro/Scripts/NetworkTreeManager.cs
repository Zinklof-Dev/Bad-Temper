using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using ZinklofDev.ConsoleV2;

[System.Serializable]
public struct Tree
{
    public int health;
    public int index;
    public GameObject treeObject;
    public Tree(GameObject treeObject, int index)
    {
        this.treeObject = treeObject;
        this.index = index;
        health = 100;
    }
}

[System.Serializable]
public struct Rock
{
    public int health;
    public int index;
    public GameObject rockObject;
    public Rock(GameObject rockObject, int index)
    {
        this.rockObject = rockObject;
        this.index = index;
        health = 100;
    }
}

public class NetworkTreeManager : NetworkBehaviour
{
    public List<Tree> trees;

    [Rpc(SendTo.Server)]
    public void AskToRemoveTreeRpc(int treeID)
    { 
        if (trees[treeID].treeObject == null)
        {
            Debug.Log("Tree already removed!");
            return;
        }
        RemoveTreeRpc(treeID);
    }

    [Rpc(SendTo.Everyone)]
    public void RemoveTreeRpc(int treeID)
    {
        if (trees[treeID].index != treeID)
        {
            Debug.Log("Something is definitly broken, the index stored by the Tree does not match it's index in the trees array");
            return;
        }

        Destroy(trees[treeID].treeObject);
    }

    [Command("Command to remove a tree")]
    public static void RemoveTree(int treeID)
    {
        NetworkTreeManager networkTreeManager = FindAnyObjectByType<NetworkTreeManager>();
        networkTreeManager.AskToRemoveTreeRpc(treeID);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public List<Rock> rocks;


    [Rpc(SendTo.Server)]
    public void AskToRemoveRockRpc(int rockID)
    {
        if (rocks[rockID].rockObject == null)
        {
            Debug.Log("Rock already removed!");
            return;
        }
        RemoveRockRpc(rockID);
    }

    [Rpc(SendTo.Everyone)]
    public void RemoveRockRpc(int rockID)
    {
        if (rocks[rockID].index != rockID)
        {
            Debug.Log("Something is definitly broken, the index stored by the Rock does not match it's index in the rocks array");
            return;
        }

        Destroy(rocks[rockID].rockObject);
    }

    [Command("Command to remove a tree")]
    public static void RemoveRock(int rockID)
    {
        NetworkTreeManager networkTreeManager = FindAnyObjectByType<NetworkTreeManager>();
        networkTreeManager.AskToRemoveRockRpc(rockID);
    }
}
