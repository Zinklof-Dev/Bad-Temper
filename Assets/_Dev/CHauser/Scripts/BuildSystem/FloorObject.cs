using System;
using System.Collections.Generic;
using UnityEngine;
using ZinklofDev.Utils.MathZ;

public class FloorObject : MonoBehaviour
{
    [SerializeField] List<SnapPoint> inspectorWallPoints = new List<SnapPoint>();

    private void OnDrawGizmos()
    {
        foreach (SnapPoint point in inspectorWallPoints)
        {

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position + point.position, 0.15f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + point.position, transform.position + Vectors.DirPoint(point.position, point.eulerRotation, 0.5f, true));
            // Debug.Log(Vectors.DirPoint(point.pos, point.eulerRotation, 0.5f));
        }
    }

    public List<SnapPoint> GetWallPoints()
    {
        return inspectorWallPoints;
    }
}
    
