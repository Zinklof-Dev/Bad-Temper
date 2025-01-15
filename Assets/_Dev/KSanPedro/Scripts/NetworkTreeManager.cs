using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

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
    public void AskToDeleteTreeRpc(int treeID)
    {
        DeleteTreeRpc(treeID);
    }

    [Rpc(SendTo.Everyone)]
    public void DeleteTreeRpc(int treeID)
    {
        if (trees[treeID].index != treeID)
        {
            Debug.Log("Something is definitly broken, the index stored by the Tree does not match it's index in the trees array");
            return;
        }

        Destroy(trees[treeID].treeObject);
    }
}
