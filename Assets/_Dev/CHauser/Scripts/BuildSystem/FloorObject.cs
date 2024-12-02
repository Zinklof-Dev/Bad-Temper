using System;
using System.Collections.Generic;
using UnityEngine;
using ZinklofDev.Utils.MathZ;

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
            Gizmos.DrawSphere(transform.position + point.pos, 0.15f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + point.pos, transform.position + Vectors.DirPoint(point.pos, point.eulerRotation, 0.5f, true));
            // Debug.Log(Vectors.DirPoint(point.pos, point.eulerRotation, 0.5f));
        }
    }

    public List<WallPoint> GetWallPoints()
    {
        return inspectorWallPoints;
    }
}
    
