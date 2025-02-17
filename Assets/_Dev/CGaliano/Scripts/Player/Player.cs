/*using Unity;
using System;

Namespace BadTemper
{
  public class Player : NetworkBehavior
  {
    [Header("References")]
    [SerializeField] CharacterControler cc;
    [SerializeField] GameObject cameraObject;
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
    }
    
    private void CameraHandeler()
    {
      if (!TPV)
      {
        firstPerson();
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

    private void firstPerson()
    {
      playerLookXY.y += Input.GetAxis("MouseY") * sensitivity * Time.deltaTime;
      playerLookXY.x = Mathf.Clamp(PlayerLookXY.x + (Input.GetAxis("MouseX") * sensitivity * Time.deltaTime), -90, 90);

      transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, playerLookXY.y, transform.rotation.z));
      cameraObject.transform.localRotation = Quaternion.Euler(new Vector3(playerLookXY.x, cameraObject.transform.localRotation.y, cameraObject.transform.localRotation.z));
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
    
      float pms = 0;
    
      if (Input.GetKey(KeyCode.LeftShift)
        pms = moveSpeed * SprintMult * Time.deltaTime;
      else
        pms = moveSpeed * Time.deltaTime;

      if (!cc.isGrounded)
        pms *= inAirMult;

      Vector3 addedMovement = ((Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward)) * pms;

      if (cc.isGrounded && lastJumpInput != -1)
        addedMovement.y = jumpForce;
      else if (cc.isGrounded)
        addedMovement.y = -0.01f;
      else
        addedMovement = Physics.Gravity;

      linearVelocity += addedMovement;
    }

    public void AFT() // Air Resistance - Friction - Terminal Velocity
    {
      // Terminal Velocity
      linearVelocity.x = Mathf.Clamp(linearVelocity.x, -terminalVelocity, terminalVelocity);
      linearVelocity.y = Mathf.Clamp(linearVelocity.y, -terminalVelocity, terminalVelocity);
      linearVelocity.z = Mathf.Clamp(linearVelocity.z, -terminalVelocity, terminalVelocity);
    
      // Air Resistance (Drag)
      linearVelocity = linearVelocity - ((linearVelocity * drag) * Time.feltaTime);

      // Friction (Drag again, but only if grounded)
      if (cc.isGrounded)
        linearVelocity = linearVelocity - ((linearVelocity * friction) * Time.deltaTime);
    }

    private void Update()
    {
      CameraHandeler();
      MovementHandeler();
      AFT();
      cc.Move(linearVelocity);
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
