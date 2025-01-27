using System;
using UnityEngine;
using Unity.Netcode;
using ZinklofDev.ConsoleV2;
using TMPro;
using Unity.Collections;
using System.Text;
using UnityEditor.PackageManager.Requests;
using Mono.Cecil;

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
    [SerializeField] float terminalVelo;
    [Header("Input Variables")]
    [SerializeField] float jumpInputMemory; // how many seconds till the controller forgets the player qeued a jump action.
    [Header("Network Variables")]
    [SerializeField] string username;

    float timeSinceJumpInput;

    // I was going to use local versions, to cut down on potential ms cost of contacting another class, then I realized IsOwner and IsServer both come from inheritence from NetworkBehaviour... so this would be the same exact cost, but more memory.
    // private bool isOwner = false;
    // private bool isServer = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // in future fetch usernname
            // SendServerUsernameRPC(username);
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

    private void Update()
    {
        CheckNonContinuousInput();
    }

    private void FixedUpdate()
    {
        if (timeSinceJumpInput != -1)
            HandleJumpInput();
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
        Vector3 movement = Vector3.Normalize(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")));
        movement *= movementSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            movement *= sprintMult;

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