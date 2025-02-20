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
/*using Unity;
using System;

Namespace BadTemper
{
  public class Player : NetworkBehavior
  {
    [Header("References")]
    [SerializeField] CharacterControler cc;
    [SerializeField] GameObject cameraObject;
    [SerializeField] transform[] jointReferences; // 0 should be the head / skull joint | 1 should be neck | 2 should be top spine joint | 3 should be mid spine joint
    [Header("Movement Varaibles")]
    [SerializeField] float moveSpeed;
    [SerializeField] float sprintMult;
    [SerializeField] float inAirMult;
    [SerializeField] float jumpForce;
    [SerializeField] float jumpMem; // how long we remember the player hit spacebar for, makes jumping feel cleaner and smoother.
    [Header("Physics")
    [SerializeField] float drag;
    [SerializeField] float waterDrag;
    [SerializeField] float friction;
    [SerializeField] float terminalVelocity;
    [SerializeField] float waterLevel; // universal since the game only has an ocean and no lakes (as of now) so we don't need any complex system to check for water (would probably just set up a Volume system for that if ever needed)
    [Header("Perspective")]
    [SerializeField] Vector3 TPVCamOffset
    [SerializeField] float TPVCamLerpT;
    [Header("Settings Overriden")]
    [SerializeField] float sensitivity;
    
    Vector2 playerLookXY;
    public bool TPV = false; // third person view, likely debug only

    public Vector3 linearVelocity;
    float lastJumpInput;

    static BadTemper.Player playerRef;

    private void Start() // change to on network spawn later
    {
      playerRef = this;

      cameraObject = GameObject.FindByTag("Main Camera");
      cameraObject.transform.SetParent(jointReferences[0])
    }
    
    private void CameraHandeler()
    {
      playerLookXY.y += Input.GetAxis("MouseY") * sensitivity * Time.deltaTime;
      playerLookXY.x = Mathf.Clamp(PlayerLookXY.x + (Input.GetAxis("MouseX") * sensitivity * Time.deltaTime), -90, 90);
    
      if (!TPV)
      {
        FPB();
      }
      else // this is not the most efficient method but TPV will likely only be a debug tool. if ever implimented properly will make more efficent code.
      {
        Vector3 cameraCurrentPos = cameraObject.transform.localPosition;

        Vector3 cameraTargetPos = new Vector3(0,0,0) + (cameraObject.transform.forward * TPVCamOffset.Z) + (cameraObject.transform.right * TPVCamOffset.X) + (cameraObject.transform.up * TPVCamOffset.Y)
        
        cameraObject.transform.localPosition = new Vector3(0,0,0);
        firstPerson();
        cameraObject.transform.localPosition = cameraCurrentPos;
        cameraObject.transorm.localPosition = Vector3.Lerp(camerObject.transform.localPosition, cameraTargetPos, TPVCamLerpT);
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
      float x = playerLookXY.x
      
      if (x < 0)
      {
        float neckRot = x * 0.95f;
        float spineTopRot = x * 0.025f;
        float SpineMidRot = x * 0.025f;
      }
      else if (x > 0)
      {
        float neckRot = x * 0.75f;
        float spineTopRot = x * 0.10f;
        float SpineMidRot = x * 0.15f;
      }
      else
      {
        float neckRot = 0;
        float spineTopRot = 0;
        float SpineMidRot = 0;
      }

      jointReferences[1].localRotation = Quaternion.Euler(new Vector3(neckRot, jointReferences[1].localRotation.y, jointReferences[1].localRotation.z));
      jointReferences[2].localRotation = Quaternion.Euler(new Vector3(spineTopRot, jointReferences[2].localRotation.y, jointReferences[2].localRotation.z));
      jointReferences[3].localRotation = Quaternion.Euler(new Vector3(spineMidRot, jointReferences[3].localRotation.y, jointReferences[3].localRotation.z));
    }

    private void IKHandeler() // NOTE 2/20/2025 CURRENT MODEL DOES NOT HAVE ANY IK JOINTS! WILL ADD TO THE MODEL SOON!
    {
      // neat little resource, explains about what I thought i'd have to do going into this, but also clarifies what I wasn't quite sure on: https://weaverdev.io/projects/bonehead-procedural-animation/
    
      // only planning to do IK for legs, meaning the only joints not controlled by code are the arm hierarchies. which will allow for more fluid animation for attacks and such rather than wonky procedural IK.

      // Once IK is set up on the model this should be as simple as moving a foot joints world POS to the direction of the velocity when it gets too far away, and ensuring to alternate between legs.
      // could go a little more complex and have a system to ensure that the foot joints don't enter a "bad area" (like the left foot going to the right side of the character)
    }

    private void MovementHandeler()
    {
      if (Input.GetKey(KeyCode.Space))
      {
        lastJumpInput = jumpMem;
      }
      else if (lastJumpInput != -1)
      {
        lastJumpInput -= Time.deltaTime;
        if (lastJumpInput < 0)
          lastJumpInput = -1;
      }
    
      float mm = 0; // stands for "movement multiplier"
    
      if (Input.GetKey(KeyCode.LeftShift)
        mm = moveSpeed * SprintMult * Time.deltaTime;
      else
        mm = moveSpeed * Time.deltaTime;

      if (!cc.isGrounded)
        mm *= inAirMult;

      Vector3 addedMovement = ((Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward)) * pms;

      if (transform.position.y <= waterHeight && lastJumpInput != -1)
        addedMovement.y = jumpForce; // water drag will make this cause less force than a reg jump later
      else if (cc.isGrounded && lastJumpInput != -1)
        addedMovement.y = jumpForce;
      else if (cc.isGrounded)
        addedMovement.y = -0.01f;
      else
        addedMovement = Physics.Gravity;

      linearVelocity += addedMovement;
    }

    public void WAFT() // Water Drag - Air Resistance - Friction - Terminal Velocity
    {
      // Terminal Velocity
      linearVelocity.x = Mathf.Clamp(linearVelocity.x, -terminalVelocity, terminalVelocity);
      linearVelocity.y = Mathf.Clamp(linearVelocity.y, -terminalVelocity, terminalVelocity);
      linearVelocity.z = Mathf.Clamp(linearVelocity.z, -terminalVelocity, terminalVelocity);
    
      // Air Resistance (Drag)
      linearVelocity = linearVelocity - ((linearVelocity * drag) * Time.feltaTime);

      // Friction (Drag again, but only if grounded, and not affecting Y aka jump velocity)
      if (cc.isGrounded)
      {
        linearVelocity.x = linearVelocity.x - ((linearVelocity.x * friction) * Time.deltaTime);
        linearVelocity.z = linearVelocity.z - ((linearVelocity.z * friction) * Time.deltaTime);
      }

      // Water Drag (Drag again, but only if under water)
      if (transform.position.y <= waterHeight)
        linearVelocity = linearVelocity - ((linearVelocity * waterDrag) * Time.deltaTime);
    }

    private void Update()
    {
      CameraHandeler();
      MovementHandeler();
      WAFT();
      cc.Move(linearVelocity);
      IK();
    }

    //////// COMMANDS ////////

    [Command("Toggles Third Person View, really only a debug tool, not optimized for gameplay.")]
    static private void ToggleThirdPerson()
    {
      if (playerRef = null || playerRef == null)
      {
        Debug.LogError("No Player");
        return;
      }
      
      playerRef.TPV = !TPV;
    }

    static private void TP(float x, float y, float z) // likely to not work currently like the old one wasn't working.
    {
      if (playerRef = null || playerRef == null)
      {
        Debug.LogError("No Player");
        return;
      }
      
      playerRef.transform.position = new Vector3(x,y,z);
    }

    static public void AddVelocity(float x, float y, float z)
    {
      if (playerRef = null || playerRef == null)
      {
        Debug.LogError("No Player");
        return;
      }
      
      playerRef.linearVelocity += new Vector3(x,y,z)
    }
  }
}
*/
