using System;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class PlayerV2 : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] GameObject _playerCamera;
    [SerializeField] TextMeshPro _usernameObject;
    [SerializeField] Rigidbody rb;
    [Space(10)]
    [Header("Movement Physics")]
    [SerializeField] float movementSpeed;
    [SerializeField] float sprintMult;
    [SerializeField] float jumpForce;
    [SerializeField] float NotGroundedMult;
    [Header("Input Variables")]
    [SerializeField] float jumpInputMemory; // how many seconds till the controller forgets the player qeued a jump action.
    [SerializeField] float sensitivity;
    [Header("Network Variables")]
    [SerializeField] string username;

    float timeSinceJumpInput;
    bool isGrounded;
    private Vector3 cameraRotationEuler;
         
    // I was going to use local versions, to cut down on potential ms cost of contacting another class, then I realized IsOwner and IsServer both come from inheritence from NetworkBehaviour... so this would be the same exact cost, but more memory.
    // private bool isOwner = false;
    // private bool isServer = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // in future fetch usernname
            // SendServerUsernameRPC(username);
            rb = gameObject.GetComponent<Rigidbody>();
            _playerCamera = GameObject.FindWithTag("MainCamera");
            _playerCamera.transform.SetParent(gameObject.transform, false);
            _playerCamera.transform.localPosition = new Vector3(0, 0.95f, 0);
        }
        else if (IsServer)
        {
        }
        else
        {
            // RequestUsernameRPC(OwnerClientId);
        }

        base.OnNetworkSpawn();
    }

    private void XRotation()
    {
        //simple method taking the mouses up/down movement, turning it into a float, and applying that to a perminant Vector3 that is then turned into a quaternion by unity since i depreciated my utils math library
        //then the cameras internal quaternion used for its rotation is changed to the value of that generated quaternion. Is this the most performant route? no, but i don't wanna code in 4d, and i've used
        //this exact method for years now, and i still hit 700 fps on my home rig and 120 on the school desktops. without this entire script its like... 2-3 FPS on the home rig, not even one on the school desktop
        //since the school desktop is bottle necked on its GPU and has an actually pretty good CPU.

        float x = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        cameraRotationEuler += new Vector3(-x, 0, 0);
        if (cameraRotationEuler.x > 90)
        {
            cameraRotationEuler.x = 90;
        }
        if (cameraRotationEuler.x < -90)
        {
            cameraRotationEuler.x = -90;
        }
        _playerCamera.transform.localRotation = Quaternion.Euler(cameraRotationEuler);
    }

    private void YRotation()
    {
        //this is a pretty simple section of code that just gets the mouses left and right movement and applies it to making the player object to move left and right
        //we don't make the camera itself move left and right because we parent it to the player so it will adjust with the player object turning. allows transform.forward and the likes to work correctly
        //and it would be much less performant to make sure the camera doesn't turn oddly when its not looking level with the horizon...
        //if you don't get what i mean by that... you're blessed by not working in this line of work.

        float y = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;

        gameObject.transform.Rotate(0, y, 0);
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        CheckNonContinuousInput();
        XRotation();
        YRotation();
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        else
        {
            isGrounded = Physics.Raycast(transform.position, -transform.up, 1.05f);
        }
        
        if (!ZinklofDev.ConsoleV2.Console.isOpen)
        {
            if (timeSinceJumpInput != -1 && isGrounded)
                HandleJumpInput();
            HandleMovement();
        }
    }

    private void CheckNonContinuousInput()
    {
        if (timeSinceJumpInput != -1)
        {
            timeSinceJumpInput += Time.deltaTime;
            if (timeSinceJumpInput > jumpInputMemory)
                timeSinceJumpInput = -1;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            timeSinceJumpInput = 0;
        }
    }

    private void HandleMovement()
    {
        Vector3 movement = (transform.right * Input.GetAxis("Horizontal")) + (transform.forward * Input.GetAxis("Vertical"));
        movement = Vector3.Normalize(movement);
        movement *= movementSpeed;
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded)
            movement *= sprintMult;

        if (!isGrounded)
            movement *= NotGroundedMult;

        rb.AddForce(movement, ForceMode.Acceleration);
    }

    private void HandleJumpInput()
    {
        rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        timeSinceJumpInput = -1;
    }

    private void UpdateUsernameObject()
    {
        throw new NotImplementedException();
    }

    [Rpc(SendTo.Server)]
    public void SendServerUsernameRPC(FixedString64Bytes username)
    {
        SendUSernameToAllClientsRPC(username); // Because clients cannot send RPCs to clients.
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SendUSernameToAllClientsRPC(FixedString64Bytes username)
    {
        this.username = username.ToString();
        UpdateUsernameObject();
    }
}