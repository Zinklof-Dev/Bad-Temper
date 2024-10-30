using System;
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
    [SerializeField] List<WallPoint> inpsectorWallPoints = new List<WallPoint>;

    private void OnDrawGizmos()
    {
        foreach(WallPoint point in inspectorWallPoints)
        {
            
            Gizmos.color = Color.Blue;
            Gizmos.DrawSphere(point.pos, 0.25f);
            //Gizmos.DrawLine(point.pos, )
            //this is commented because i ran out of time, you'll need to use the euler rotation to find another point infront of the wallpoint in the direction your quaternion rotation points
            //Then you'll have a debug view of the points and where they are facing which is mostly for QOL and later usage.
        }
    }

    public List<WallPoint> GetWallPoints();
    {
        List<WallPoint> tempWallPointList = new List<WallPoints>;
        
        //this is simple logic, you can tell why we use this func
        
        return tempWallPointList;
    }
}
    
