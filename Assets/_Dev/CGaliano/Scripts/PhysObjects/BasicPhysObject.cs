/*using System;
using Unity;

public class BasicPhysObject : NetworkBehavior
{
  [SerializedField] bool pickedUp;
  [SerializedField] Vector3 Offset;
  [SerializedField] float rotSens; // rotation sensitivity;

  [SerializedField] bool serverSide; // toggle whether this logic should be server side or not, if server side it MUST be syncronized over network, non server side doesn't have to be synced but objects may end up in different places if not.

  Quaternion rot; // used when being held so player can rotate the object.

  private GameObject playerCamera;
  private RigidBody rb;

  private void Start()
  {
    rb = gameObject.GetComponent<RigidBody>();

    if (!IsServer && serverSide)
    {
      //use func that deletes RB, this way only the server does physics math then syncs that over network.
    }
  }

  private void Update()
  {
    if (pickedUp && IsOwner)
    {
      transform.position = Vector3.Lerp(transform.position, playerCamera.transform.position + Offset, 0.65f);
      if (Input.GetKey(Keycode.G))
      {
        // uh oh quaternion math time | Cameron
      
        float y = Input.GetAxis("MouseY") * rotSens * Time.deltaTime;
        float x = Input.GetAxis("MouseX") * rotSens * Time.deltaTime;

        rot *= Quaternion.Euler(x, y, 0); // will this work? we shall find out! quaternions hurt my brain :D | Cameron
      }
      transform.rotation = rot;
    }
  }
  
  [Rpc(SendTo.Server)]
  public void RequestToPickupRPC(long clientID)
  {
    ChangeOwnership(clientID);
    TogglePickedUpRPC();
  }

  [Rpc(SendTo.Server)]
  public void RequestToDropRPC()
  {
    ChangeOwnership((long)0)
    TogglePickedUpRPC();
  }

  [Rpc(SendTo.ClientsAndHost)]
  public void TogglePickedUpRPC()
  {
    pickedUp = !pickedUp;
  }
}*/
