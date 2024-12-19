using System;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

//Welcome to the world of data oriented programming instead of unities gameobject oriented programming :D

public class TreeManager : MonoBehaviour
{
    [SerializeField] Mesh[] meshes;
    [SerializeField] Material material;

    List<Matrix4x4> _Matrices = new List<Matrix4x4>();
    List<int> _MeshIndexes;

    ComputeBuffer _ComputeBuffer;

    private readonly uint[] _args = { 0, 0, 0, 0, 0 };
    private ComputeBuffer _argsBuffer;

    private bool ready = false;

    public void AddTree(Matrix4x4 matrix, int treeType)
    {
        _Matrices.Add(matrix);
        //add treemeshindex later when that gets used
    }

    public void TreesDone()
    {
        UpdateBuffer();
        ready = true;
    }

    private void Update()
    {
        if (!ready) return;

        RenderParams renderParams = new RenderParams(material);
        Graphics.DrawMeshInstancedIndirect(meshes[0], 0, material, new Bounds(Vector3.zero, Vector3.one * 1000), _argsBuffer);
    }

    private void OnDisable()
    {
        _ComputeBuffer?.Release();
        _ComputeBuffer = null;
        _argsBuffer?.Release();
        _argsBuffer = null;
    }

    public void UpdateBuffer()
    {
        int count = _Matrices.Count;

        _ComputeBuffer?.Release();
        _ComputeBuffer = new ComputeBuffer(count, 16);
        _argsBuffer?.Release();
        _argsBuffer = null;
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        var positions = new Vector4[count];

        for (int i = 0; i < count; i++)
        {
            Matrix4x4 matrix = _Matrices[i];

            positions[i] = new Vector4(matrix[0,3], matrix[1,2], matrix[2,3], 1);
        }

        _ComputeBuffer.SetData(positions);
        material.SetBuffer("position_buffer", _ComputeBuffer);

        _args[0] = meshes[0].GetIndexCount(0);
        _args[1] = (uint)count;
        _args[2] = meshes[0].GetIndexStart(0);
        _args[3] = meshes[0].GetBaseVertex(0);

        _argsBuffer.SetData(_args);
    }
}
