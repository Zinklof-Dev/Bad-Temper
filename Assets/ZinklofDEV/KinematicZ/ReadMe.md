<h1>KinematicZ (pronounced Kinematics or Kinematic Zee)</h1>
This is a simple plugin meant to perform IK and likely at some point FK in unity at real time. Allowing for procedural animation (either by fully IK or sprinkled in IK with HQ anims).

<h2>Sources:</h2>
[EasyIK](https://github.com/joaen/EasyIK)

<h1>Usage</h1>
<p>The usage of the IK portion of KinematicZ is increadibly simple, It just involves placing the IK script on the end joint, and placing the previous joints in the hiararchy into the array</p>
<p>IE:</p>

```cs
// this assumes a GameObject called "rWristObject" is defined and set via inspector, and that said GameObject has the IKHandle script on it
IKHandle rWristIKHandle = rWristObject.GetComponent<IKHandle>();

rWristIKHandle.parentJoints[0] = elbow;
rWristIKHandle.parentJoints[1] = shoulder;

// this array is public and thus can be set via inspector rather than code of course :D
```
<p></p>
<p>This would then in real time create IK for the characters arm based on the wrists target position, which is accessed by getting the IKHandle script, then using the Target() function IE:</p>

```cs
// this assumes a GameObject called "rWristObject" is defined and set via inspector, and that said GameObject has the IKHandle script on it
IKHandle rWristIKHandle = rWristObject.GetComponent<IKHandle>();

rWristIKHandle.Target(new Vector3(0,0,0)); // this changes the target for the IK to the provided Vector3 as if it were an offset of the origin (by default the root bone if none is assinged in inspector)
rWristIKHandle.Target(new Vector3(0,0,0), true); // this changes the target for the IK to the provided Vector3 as if it were world space (if false instead of true it behaves like the line above)
rWristIKHandle.Move(New Vector3(0,1,0)); // this changes the target by adding the provided Vector3 to the current target position, akin to Transform.Translate()
```
