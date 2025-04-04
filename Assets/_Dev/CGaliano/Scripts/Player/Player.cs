/*
Abreviations commonly found throughout my work, and their meaning:

rot = rotation
mem = memory
tpv = third person view 
tpp = third person perspective (used less often)
fpv = first person view
fpp = first person perspective (used less often)
IK = inverse kinematics
cmd = command

other uncommon abreviations will have their meaning in a comment next to them or on a neighboring line.
*/
using Unity;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Unity.Netcode;
using ZinklofDev.ConsoleV2;
using ZinklofDev.Utils.MathZ;

namespace BadTemper
{
    public class Player : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] CharacterController cc;
        [SerializeField] GameObject cameraObject;
        [SerializeField] Transform[] jointReferences; // 0 should be the head / skull joint | 1 should be neck | 2 should be top spine joint | 3 should be mid spine joint
        [Header("Movement Varaibles")]
        [SerializeField] float moveSpeed;
        [SerializeField] float sprintMult;
        [SerializeField] float inAirMult;
        [SerializeField] float jumpForce;
        [SerializeField] float jumpMem; // how long we remember the player hit spacebar for, makes jumping feel cleaner and smoother.
        [Header("Physics")]
        [SerializeField] float drag;
        [SerializeField] float waterDrag;
        [SerializeField] float friction;
        [SerializeField] Vector3 terminalVelocity;
        [SerializeField] float gravityMult;
        [SerializeField] float waterLevel; // universal since the game only has an ocean and no lakes (as of now) so we don't need any complex system to check for water (would probably just set up a Volume system for that if ever needed)
        [Header("Leg IK")]
        [SerializeField] Transform[] footTargets; // 0 is left | 1 is right
        [SerializeField] float maxDist;
        [SerializeField] float overShoot;
        [SerializeField] AnimationCurve stepHeightCurve;
        [SerializeField] float stepTime;
        [SerializeField] Vector3[] homePositions; // this is an offset from root | 0 is left | 1 is right
        [SerializeField] Transform pelvis; // used to add some weight and exagerated momentum when falling and jumping
        [SerializeField] float TargetOffset; // since the IK system uses the ankle as the final joint and not the foot we need to have a y offset to keep the foot above ground.
        [Header("Perspective")]
        [SerializeField] Vector3 TPVCamOffset;
        [SerializeField] float TPVCamLerpT;
        [Header("Settings Overriden")]
        [SerializeField] float sensitivity;
        [Header("Debug")]
        [SerializeField] bool gizmos;
        [SerializeField] bool ikGizmos;
        [SerializeField] bool physicsGizmos;
        [SerializeField] bool calcRealVelocity;
        [SerializeField] float gizmosSize;
        [SerializeField] private Vector3 realVelocity; //calculates velocity mathmatically since the linearvelocity variable is delta timed and thus doesn't give correct results when viewed in inspector

        Vector2 playerLookXY;
        public bool TPV = false; // third person view, likely debug only

        public Vector3 linearVelocity;
        [SerializeField] float lastJumpInput;

        private Quaternion midSpineStartRot;
        private Quaternion topSpineStartRot;

        static BadTemper.Player playerRef;

        private float[] stepProgress;
        private int currentlySteppingLeg = -1;
        private Vector3 currentLegTargetPos;
        private Vector3 currentLegStartPos;

        private void OnDrawGizmos()
        {
            // expensive stuff, only used in editor with a toggle though so it wont really matter once the game is compiled and a build is made.
        
            if (!gizmos)
                return;

            if (ikGizmos)
            {
                for (int i = 0; i < footTargets.Length; i++)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(footTargets[i].position + (transform.up * TargetOffset), gizmosSize);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(HPtoWorld(homePositions[i]), gizmosSize);
                    Gizmos.DrawLine(HPtoWorld(homePositions[i]), footTargets[i].position + (transform.up * TargetOffset));
                    Gizmos.DrawWireSphere(HPtoWorld(homePositions[i]), maxDist);

                    if (currentlySteppingLeg != -1)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawSphere(currentLegTargetPos, gizmosSize);
                        Gizmos.DrawLine(currentLegTargetPos, footTargets[currentlySteppingLeg].position + (transform.up * TargetOffset));
                    }
                }
            }
            if (physicsGizmos)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(transform.position + new Vector3(0,1,0), transform.position + new Vector3(0,1,0) + linearVelocity);

                // Figure out what the friction would be
                Vector3 frictionV = new Vector3(0,0,0);
                frictionV.x = (linearVelocity.x - ((linearVelocity.x * friction) * Time.deltaTime)) - linearVelocity.x;
                frictionV.z = (linearVelocity.z - ((linearVelocity.z * friction) * Time.deltaTime)) - linearVelocity.y;
                    
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position + new Vector3(0,1,0), transform.position + new Vector3(0,1,0) - frictionV);

                // Figure out what the air resistance would be
                Vector3 dragV = (linearVelocity - ((linearVelocity * drag) * Time.deltaTime)) - linearVelocity;

                Gizmos.color = Color.yellow;
                if (dragV.y < -0.1f && dragV.y > 0.1f)
                    Gizmos.DrawLine(transform.position + new Vector3(0,1,0), transform.position + new Vector3(0,1,0) - dragV);
                else
                    Gizmos.DrawLine(transform.position + new Vector3(0,1,0), transform.position + new Vector3(0,1,0) - dragV);

                // Water Drag
                Vector3 waterDragV = (linearVelocity - ((linearVelocity * waterDrag) * Time.deltaTime)) - linearVelocity;
                
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position + new Vector3(0,1,0), transform.position + new Vector3(0,1,0) - waterDragV);
            }
        }

        private void Start() // change to on network spawn later
        {
            playerRef = this;

            cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
            cameraObject.transform.SetParent(jointReferences[0]);
            cameraObject.transform.localPosition = Vector3.zero;

            midSpineStartRot = jointReferences[3].localRotation;
            topSpineStartRot =  jointReferences[2].localRotation;

            stepProgress = new float[footTargets.Length];
        }
    
        private void CameraHandeler()
        {
            playerLookXY.y += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            playerLookXY.x = Mathf.Clamp(playerLookXY.x + (Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime), -90, 90);
    
            if (!TPV)
            {
                FPB();
            }
            else // this is not the most efficient method but TPV will likely only be a debug tool. if ever implimented properly will make more efficent code.
            {
                //Vector3 cameraCurrentPos = cameraObject.transform.localPosition;

                Vector3 cameraTargetPos = new Vector3(0, 0, 0) + (cameraObject.transform.forward * TPVCamOffset.z) + (cameraObject.transform.right * TPVCamOffset.x) + (cameraObject.transform.up * TPVCamOffset.y);
        
                //cameraObject.transform.localPosition = new Vector3(0,0,0);
                FPB();
                //cameraObject.transform.localPosition = cameraCurrentPos;
                cameraObject.transform.localPosition = Vector3.Lerp(cameraObject.transform.localPosition, TPVCamOffset, TPVCamLerpT);
            }
        }

        private void FirstPerson()
        {
            // OLD SYSTEM
            // playerLookXY.y += Input.GetAxis("MouseY") * sensitivity * Time.deltaTime; // this and the following line were moved to the CameraHandeler function to make the inputs more universal
            // playerLookXY.x = Mathf.Clamp(PlayerLookXY.x + (Input.GetAxis("MouseX") * sensitivity * Time.deltaTime), -90, 90);

            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, playerLookXY.y, transform.rotation.z));
            cameraObject.transform.localRotation = Quaternion.Euler(new Vector3(playerLookXY.x, cameraObject.transform.localRotation.y, cameraObject.transform.localRotation.z));
        }

        private void FPB() // stands for "First Person Bones" redesign of the look system to dynamically use the joints of the player to make their movement more fluid-esc | Could have used IK for this tbh... meh... actually isn't this just really basic IK?
        {
            // this system will use the WANTED X value in playerLookXY to rotate the spine and neck joints in order to look ~90 down or ~90 up.

            // Plan: when looking 90 degrees down, neck should account for ~95% of rotation, when looking 90 degrees up, neck should account for ~75% (human neck can bend further forward than it can backwards)
            float x = playerLookXY.x;

            float neckRot = 0;
            float spineTopRot = 0;
            float spineMidRot = 0;

            if (x < 0)
            {
                neckRot = x * 0.50f;
                spineTopRot = x * 0.25f;
                spineMidRot = x * 0.25f;
            }
            else if (x > 0) // else if as we want nothing to happen if x does just straight up equal zero
            {
                neckRot = x * 0.75f;
                spineTopRot = x * 0.10f;
                spineMidRot = x * 0.15f;
            }

            jointReferences[1].localRotation = Quaternion.Euler(new Vector3(0, -90.5f, neckRot));
            //jointReferences[2].localRotation = Quaternion.Euler(new Vector3(spineTopRot, jointReferences[2].localRotation.y, jointReferences[2].localRotation.z));
            //jointReferences[3].localRotation = Quaternion.Euler(new Vector3(spineMidRot, jointReferences[3].localRotation.y, jointReferences[3].localRotation.z));

            jointReferences[2].localRotation = topSpineStartRot * Quaternion.Euler(-spineTopRot, 0, 0);
            jointReferences[3].localRotation =  midSpineStartRot * Quaternion.Euler(0, -spineMidRot, 0);

            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, playerLookXY.y, transform.rotation.z));
        }

        private void IKHandeler()
        {
            // neat little resource, explains about what I thought i'd have to do going into this, but also clarifies what I wasn't quite sure on: https://weaverdev.io/projects/bonehead-procedural-animation/
            
            // Check which legs should be moved;
            for (int i = 0; i < footTargets.Length; i++)
            {
                footTargets[i].rotation = transform.rotation;

                if (currentlySteppingLeg != -1) // if currently in process of moving a leg, don't check for others, only one leg may leave the grond at any given moment
                        break;

                if (Vectors.SqrDist3(footTargets[i].position, homePositions[i] + transform.position) > (maxDist * maxDist))
                {
                    currentlySteppingLeg = i; // save which leg needs to move
                    stepProgress[i] -= Time.deltaTime; // this just ensures the foot doesn't start to move this frame but rather next frame
                    currentLegStartPos = footTargets[i].position; // save our starting position

                    // evaluate how far we need to move
                    Vector3 difference = HPtoWorld(homePositions[i]) - footTargets[i].position;

                    // calc mult for overshoot
                    // this prevents a funny issue where if the leg is off from the home position to the right by even a little bit, evey sequential step will make it overshoot more and more and more until your legs are flopping side to side. by making the overshoot smaller the closer we are to the home position we ensure that it ends up making its way to basically the home position after a few steps if its drastically off to the right.
                    float multX = Mathf.Clamp(Mathf.Abs(difference.x) / maxDist, 0, 1);
                    float multZ = Mathf.Clamp(Mathf.Abs(difference.z) / maxDist, 0, 1);

                    // add overshoot
                    difference.x = (difference.x >= 0) ? difference.x + (overShoot * multX) : difference.x - (overShoot * multX);
                    difference.z = (difference.z >= 0) ? difference.z + (overShoot * multZ) : difference.z - (overShoot * multZ);

                    // save new position
                    currentLegTargetPos = footTargets[i].position + difference;
                }
            }

            if (currentlySteppingLeg == -1)
                return;

            stepProgress[currentlySteppingLeg] += Time.deltaTime; // if first run, should evaluate to zero
            float progress = stepProgress[currentlySteppingLeg] / stepTime;

            Vector3 evaluatedPosition = Vector3.Lerp(currentLegStartPos, currentLegTargetPos, progress);
            evaluatedPosition.y = stepHeightCurve.Evaluate(progress) + TargetOffset;

            footTargets[currentlySteppingLeg].position = evaluatedPosition;
            footTargets[currentlySteppingLeg].rotation = transform.rotation;

            if (stepProgress[currentlySteppingLeg] >= stepTime)
            {
                stepProgress[currentlySteppingLeg] = 0;
                currentlySteppingLeg = -1;
            }
        }

        private Vector3 HPtoWorld(Vector3 offset) // Home Position to World
        {
            return (offset.x * transform.right) + (offset.y * transform.up) + (offset.z * transform.forward) + transform.position;
        }

        private void MovementHandeler()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                lastJumpInput = jumpMem;
            else if (lastJumpInput != -1)
                lastJumpInput -= Time.deltaTime;
            if (lastJumpInput < 0)
                lastJumpInput = -1;

            float mm = 0; // stands for "movement multiplier"

            if (Input.GetKey(KeyCode.LeftShift))
                mm = moveSpeed * sprintMult * Time.deltaTime;
            else
                mm = moveSpeed * Time.deltaTime;

            if (!cc.isGrounded)
                mm *= inAirMult;

            Vector3 addedMovement = ((Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward)) * mm;

            if (transform.position.y <= waterLevel && lastJumpInput != -1)
            {                 
                addedMovement.y = jumpForce; // water drag will make this cause less force than a reg jump later
                lastJumpInput = -1;
            }
            else if (cc.isGrounded && lastJumpInput != -1)
            {
                addedMovement.y = jumpForce;
                lastJumpInput = -1;
            }               
            else if (cc.isGrounded)
                addedMovement.y = -0.01f;
            else
                addedMovement += new Vector3(0, -9.81f, 0) * gravityMult * Time.deltaTime; // if gravity mult is 1 this will compute to 9.81 m/s^2 of acceleration

            linearVelocity += addedMovement;
        }

        public void WAFT() // Water Drag - Air Resistance - Friction - Terminal Velocity
        {
            // Terminal Velocity
            linearVelocity.x = Mathf.Clamp(linearVelocity.x, -terminalVelocity.x * Time.deltaTime, terminalVelocity.x * Time.deltaTime);
            linearVelocity.y = Mathf.Clamp(linearVelocity.y, -terminalVelocity.z * Time.deltaTime, terminalVelocity.z * Time.deltaTime);
            linearVelocity.z = Mathf.Clamp(linearVelocity.z, -terminalVelocity.y * Time.deltaTime, terminalVelocity.y * Time.deltaTime);
    
            // Air Resistance (Drag)
            linearVelocity = linearVelocity - ((linearVelocity * drag) * Time.deltaTime);

            // Friction (Drag again, but only if grounded, and not affecting Y aka jump velocity)
            if (cc.isGrounded)
            {
                linearVelocity.x = linearVelocity.x - ((linearVelocity.x * friction) * Time.deltaTime);
                linearVelocity.z = linearVelocity.z - ((linearVelocity.z * friction) * Time.deltaTime);
            }

            // Water Drag (Drag again, but only if under water)
            if (transform.position.y <= waterLevel)
                linearVelocity = linearVelocity - ((linearVelocity * waterDrag) * Time.deltaTime);
        }

        private void Update()
        {
            CameraHandeler();
            MovementHandeler();
            WAFT();
            cc.Move(linearVelocity);
            IKHandeler();

            if (calcRealVelocity)
            {
                realVelocity = (1 / Time.deltaTime) * linearVelocity; // basically just undoing the delta time calculations.
            }
        }

        //////// COMMANDS ////////

        [Command("Toggles Third Person View, really only a debug tool, not optimized for gameplay.")]
        static private void ToggleThirdPerson()
        {
            if (playerRef = null)
            {
                Debug.LogError("No Player");
                return;
            }
      
            playerRef.TPV = !playerRef.TPV;
        }

        static private void TP(float x, float y, float z) // likely to not work currently like the old one wasn't working.
        {
            if (playerRef = null)
            {
                Debug.LogError("No Player");
                return;
            }
      
            playerRef.transform.position = new Vector3(x,y,z);
        }

        static public void AddVelocity(float x, float y, float z)
        {
                if (playerRef = null)
                {
                Debug.LogError("No Player");
                return;
                }

                playerRef.linearVelocity += new Vector3(x, y, z);
        }
    }
}
