/*using Unity;
using System;

namespace ZinklofDev.KinematicZ
{
  class IKHandle : Monobehavior
  {
    [Header("References")
    [Tooltip("if null/none will autoset to the GameObject this script is attached to.")] public Transform mainJoint;
    [Tooltip("Work backwards, first item should be the parent, second should be the parent of the parent, etc. etc. (up to shoulder/hip joints which should be the last item)")] public transform[] parrentJoints;
    [Header("IK Positioning")]
    [SerializeField] private Vector3 target;
    [SerializeField][Tooltip("In a three joint system (typical leg/arm/finger jointing) this is used to control and direct the rotation of the knee/elbow/middle joint in the right direction")] private Vector3 poleTarget;
    [Header("Performance")]
    [Tooltip("Controls whether the joint positions are updated in real time or only on function call")] public bool realtime;
    [Tooltip("How often the joint should try to update its positon, 0 = every frame")] public int fps;
    [Tooltip("How many iterations should the algorithm run through in attempt to reach the target?")] public int iterations;
    [Tooltip("How close does the main joint need to be to the target be before the algorithm stops (assuming it doesn't run out of itterations)")] public float tolerance;
    [Header("Debug")]
    [SerializeField][Tooltip("Enables display/rendering of gizmos representing joints, bones, target, etc etc. (can get slow)")] private bool debugView;
    [SerialieField][Slider(0.05f,1.0f)][Tooltip("Changes the size of the gizmos")] private float gizmoSize = 0.05f; 

    private OnDrawGizmos()
    {
      if (!debugView)
        return;

      
  }
}

// as of current this system is on pause till i can confirm if EASYIK still works in newer unity versions (AKA im too lazy to solve IK myself.
*/