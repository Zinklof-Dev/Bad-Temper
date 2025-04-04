using System;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using ZinklofDev.Utils.MathZ;

public class SpooderAnimation : MonoBehaviour
{
    [SerializeField] private Transform LB_Target_IK;
    [SerializeField] private Transform LF_Target_IK;
    [SerializeField] private Transform RB_Target_IK;
    [SerializeField] private Transform RF_Target_IK;

    [SerializeField] private Transform LB_Target_Offset;
    [SerializeField] private Transform LF_Target_Offset;
    [SerializeField] private Transform RB_Target_Offset;
    [SerializeField] private Transform RF_Target_Offset;

    [SerializeField] private Transform body;
    [SerializeField] private Vector3 previousBodyPosition;

    [SerializeField] private Vector3 LB_Target_IK_Start;
    [SerializeField] private Vector3 LF_Target_IK_Start;
    [SerializeField] private Vector3 RB_Target_IK_Start;
    [SerializeField] private Vector3 RF_Target_IK_Start;

    [SerializeField] private bool isMovingLB;
    [SerializeField] private bool isMovingLF;
    [SerializeField] private bool isMovingRB;
    [SerializeField] private bool isMovingRF;

    [SerializeField] private bool group1Moved;
    [SerializeField] private bool group2Moved;

    [SerializeField] private float timeElapsed1;
    [SerializeField] private float timeElapsed2;

    [SerializeField] private float moveDistance;
    [SerializeField] private float moveSpeed;

    [SerializeField] private MeshFilter meshFilter;

    [SerializeField] private LayerMask ignoreRaycast;

    private void Start()
    {
        LB_Target_IK_Start = LB_Target_IK.position;
        LF_Target_IK_Start = LF_Target_IK.position;
        RB_Target_IK_Start = RB_Target_IK.position;
        RF_Target_IK_Start = RF_Target_IK.position;
        meshFilter = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        SyncOffset(LF_Target_Offset, 1.184392f, 1.411505f);
        SyncOffset(RB_Target_Offset, -1.184392f, -1.411505f);
        SyncOffset(LB_Target_Offset, -1.184392f, 1.411505f);
        SyncOffset(RF_Target_Offset, 1.184392f, -1.411505f);

        VerticalMovment(RF_Target_Offset);
        VerticalMovment(RB_Target_Offset);
        VerticalMovment(LB_Target_Offset);
        VerticalMovment(LF_Target_Offset);

        if (group2Moved)
        {
            AnimateLeg(LF_Target_IK, LF_Target_Offset, ref isMovingLF, ref LF_Target_IK_Start, ref timeElapsed1);
            AnimateLeg(RB_Target_IK, RB_Target_Offset, ref isMovingRB, ref RB_Target_IK_Start, ref timeElapsed1);
        }
        if(group2Moved && !isMovingLF && !isMovingRB) 
        { 
            group2Moved = false;
            group1Moved = true;
        }

        if (group1Moved)
        {
            AnimateLeg(LB_Target_IK, LB_Target_Offset, ref isMovingLB, ref LB_Target_IK_Start, ref timeElapsed2);
            AnimateLeg(RF_Target_IK, RF_Target_Offset, ref isMovingRF, ref RF_Target_IK_Start, ref timeElapsed2);
        }
        if (group1Moved && !isMovingLB && !isMovingRF)
        {
            group2Moved = true;
            group1Moved = false;
        }

        previousBodyPosition = body.position;
    }

    void AnimateLeg(Transform IK, Transform Offset, ref bool isMoving, ref Vector3 start, ref float timeElapsed)
    {
        if (isMoving)
        {
            // timeElapsed += Time.deltaTime; Unused, need to test further in Unity
            
            float speedMultiplier = (Vectors.SqrDist3f(start, Offset.position) * Vectors.SqrDist3f(start, Offset.position)) / Time.deltaTime;

           /* if (timeElapsed > 0.5f) // Ditto as 83
            {
                isMoving = false;
            }*/

            IK.position = Vector3.Lerp(IK.position, Offset.position, moveSpeed * speedMultiplier); // Need to test if Slerp or Lerp is better, it seems like Slerp gives better results but costs more based just on a google search

            if (Vectors.SqrDist3f(IK.position, Offset.position) < 0.1f)
            {
                isMoving = false;
            }
            return;
        }

        if (Vectors.SqrDist2f(new Vector2(IK.position.x, IK.position.z), new Vector2(Offset.position.x, Offset.position.z)) > Mathf.Sqrt(moveDistance))
        {
            isMoving = true;
            start = IK.position;
            timeElapsed = 0;
        }
    }

    void VerticalMovment(Transform Offset)
    {
        RaycastHit hit;
        if(Physics.Raycast(new Vector3(Offset.position.x, Offset.position.y + 1, Offset.position.z), Vector3.down, out hit, float.PositiveInfinity, ignoreRaycast))
        {
            Offset.position = new Vector3(Offset.position.x, hit.point.y, Offset.position.z);
        }

        body.position = new Vector3(body.position.x, (LB_Target_IK.position.y + RB_Target_IK.position.y + LF_Target_IK.position.y + RF_Target_IK.position.y /* + LB_Target_Offset.position.y + RB_Target_Offset.position.y + LF_Target_Offset.position.y + RF_Target_Offset.position.y*/) / /*8*/ 4, body.position.z);
        Quaternion triangleQuaternionRotation = GetTriangleQuaternionRotation(hit.triangleIndex, meshFilter.sharedMesh);

        body.rotation = Quaternion.Euler(-triangleQuaternionRotation.eulerAngles.x, 0, -triangleQuaternionRotation.eulerAngles.z);
    }

    Quaternion GetTriangleQuaternionRotation(int triangleIndex, Mesh mesh)
    {

        Vector3 v0 = mesh.vertices[mesh.triangles[triangleIndex * 3]];
        Vector3 v1 = mesh.vertices[mesh.triangles[triangleIndex * 3 + 1]];
        Vector3 v2 = mesh.vertices[mesh.triangles[triangleIndex * 3 + 2]];
        Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
        Quaternion quaternionRotation = Quaternion.FromToRotation(normal, Vector3.up);

        //Debug.Log("Triangle Positions: " + v0 + v1 + v2+ "\nRotation of triangle: " +  quaternionRotation.eulerAngles);

        return quaternionRotation;
    }

    void SyncOffset(Transform offset, float x, float y)
    {
        offset.position = new Vector3(body.position.x + x, transform.position.y, body.position.z + y);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(LB_Target_IK.position, 0.15f);
        Gizmos.DrawSphere(LF_Target_IK.position, 0.15f);
        Gizmos.DrawSphere(RB_Target_IK.position, 0.15f);
        Gizmos.DrawSphere(RF_Target_IK.position, 0.15f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(LB_Target_Offset.position, 0.15f);
        Gizmos.DrawSphere(LF_Target_Offset.position, 0.15f);
        Gizmos.DrawSphere(RB_Target_Offset.position, 0.15f);
        Gizmos.DrawSphere(RF_Target_Offset.position, 0.15f);
    }
}
