using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Tree
{
    public Matrix4x4 matrix;
    public int treeType;

    public Tree(Matrix4x4 matrix, int treeType)
    {
       this.matrix = matrix;
       this.treeType = treeType;
    }
}

public class TreeManager : MonoBehaviour
{
    [SerializeField] public List<Tree> treeList = new List<Tree>();
    [SerializeField] Mesh[] meshes;
    [SerializeField] Material material;

    public void AddTree(Matrix4x4 matrix, int treeType)
    {
        Tree tree = new Tree(matrix, treeType);
        treeList.Add(tree);
    }

    private void Update()
    {
        foreach (Tree tree in treeList) 
        {
            RenderParams renderParams = new RenderParams(material);
            Graphics.RenderMesh(renderParams, meshes[tree.treeType], 0, tree.matrix);
        }
    }
}
