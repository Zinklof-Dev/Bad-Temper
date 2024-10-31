using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct WallPoint
{
    public Vector3 pos;
    public Vector3 eulerRotation;
    public Quaternion quaternionRotation { get; private set; }

    public WallPoint(Vector3 pos, Vector3 eulerRotation)
    {
        this.pos = pos;
        this.eulerRotation = eulerRotation;

        quaternionRotation = Quaternion.Euler(eulerRotation);
    }
}

public class FloorObject : MonoBehaviour
{
    [SerializeField] List<WallPoint> inspectorWallPoints = new List<WallPoint>();

    private void OnDrawGizmos()
    {
        foreach (WallPoint point in inspectorWallPoints)
        {

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(point.pos, 0.15f);
            Gizmos.color = Color.red;
            // Gizmos.DrawLine(point.pos, point.pos * point.eulerRotation);
        }
    }

    public List<WallPoint> GetWallPoints()
    {
        return inspectorWallPoints;
    }
}
    
