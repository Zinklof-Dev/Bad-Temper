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

    [SerializeField] private float moveDistance;
    [SerializeField] private float moveSpeed;

    private void Update()
    {
        CheckLegDistance(LB_Target_IK, LB_Target_Offset);
        CheckLegDistance(LF_Target_IK, LF_Target_Offset);
        CheckLegDistance(RB_Target_IK, RB_Target_Offset);
        CheckLegDistance(RF_Target_IK, RF_Target_Offset);
    }

    void CheckLegDistance(Transform IK, Transform Offset)
    {
        if(Vectors.SqrDist3f(IK.position, Offset.position) > Mathf.Sqrt(moveDistance))
        {
            IK.position = Offset.position;
        }
    }
}
