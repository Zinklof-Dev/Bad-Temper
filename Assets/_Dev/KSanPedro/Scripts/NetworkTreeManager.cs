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

public class NetworkTreeManager : NetworkBehaviour
{
    public List<Tree> trees;

    [Rpc(SendTo.Server)]
    public void AskToRemoveTreeRpc(int treeID)
    {
        NetworkTreeManager networkTreeManager = FindAnyObjectByType<NetworkTreeManager>();

        if (networkTreeManager.trees[treeID].treeObject == null)
        {
            Debug.Log("Tree already removed!");
            return;
        }
        RemoveTreeRpc(treeID);
    }

    [Rpc(SendTo.Everyone)]
    public void RemoveTreeRpc(int treeID)
    {
        NetworkTreeManager networkTreeManager = FindAnyObjectByType<NetworkTreeManager>();

        if (networkTreeManager.trees[treeID].index != treeID)
        {
            Debug.Log("Something is definitly broken, the index stored by the Tree does not match it's index in the trees array");
            return;
        }

        Destroy(networkTreeManager.trees[treeID].treeObject);
    }

    [Command("Command to remove a tree")]
    public static void RemoveTree(int treeID)
    {
        //AskToRemoveTreeRpc(treeID);
    }
}
