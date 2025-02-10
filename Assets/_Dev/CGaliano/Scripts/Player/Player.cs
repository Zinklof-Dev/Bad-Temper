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
    [Header("Settings Overriden")]
    [SerializeField] float sensitivity;
    
    Vector2 playerLookXY;
    bool TPV = false; // third person view, likely debug only

    Vector3 linearVelocity;
    float lastJumpInput;
    
    private void CameraHandeler()
    {
      if (!TPV)
      {
        firstPerson();
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

    public void DFT() // Drag - Friction - Terminal Velocity
    {
      // Terminal Velocity
      linearVelocity.x = Mathf.Clamp(linearVelocity.x, -terminalVelocity, terminalVelocity);
      linearVelocity.y = Mathf.Clamp(linearVelocity.y, -terminalVelocity, terminalVelocity);
      linearVelocity.z = Mathf.Clamp(linearVelocity.z, -terminalVelocity, terminalVelocity);
    
      // Drag
      linearVelocity = linearVelocity - ((linearVelocity * drag) * Time.feltaTime);

      // Friction (Drag again, but only if grounded)
      if (cc.isGrounded)
        linearVelocity = linearVelocity - ((linearVelocity * friction) * Time.deltaTime);
    }

    private void Update()
    {
      CameraHandeler();
      MovementHandeler();
      DFT();
      cc.Move(linearVelocity);
    }
  }
}
*/
