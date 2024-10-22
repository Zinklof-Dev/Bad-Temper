using NUnit.Framework;
using System;
using UnityEngine;

public class GhostObject : MonoBehaviour
{
    public bool isSpawnable; 
    public Vector3 defaultPosition;
    public LayerMask layerMask;
    public Material material;


    private void Update()
    {
        if (transform.position == defaultPosition)
            isSpawnable = false;
        else
        {
            if (CheckForCollision())
                isSpawnable = true;
            else
                isSpawnable = false;
        }

        if (isSpawnable)
            material.color = new Color(0, 1, 0, 0.4f);
        else 
            material.color = new Color(1, 0, 0, 0.4f);
    }

    private void OnApplicationQuit()
    {
        material.color = new Color(1, 1, 1, 0.4f);
    }

    private bool CheckForCollision()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2.01f, Quaternion.identity, layerMask);
        bool collidersFound = false;

        foreach (Collider collider in colliders)
        {
            collidersFound = true;
        }

        if (!collidersFound)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
