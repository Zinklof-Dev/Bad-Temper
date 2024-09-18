using TMPro;
using System;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class Player : NetworkBehaviour
{
    [Header("Player External Variables")]
    [SerializeField] public float health = 100f;
    [SerializeField] public bool playerLive = true;
    [Space(10)]
    [Header("Movement")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float sprintMult;
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravityMult;
    [SerializeField] private float terminalVelocity;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float drag;
    [Header("Camera")]
    [SerializeField] private float sensitivity;
    [SerializeField] private Vector3 cameraRotationEuler;
    [Space(20)]
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private TextMeshPro username;
    [Header("ignore me")]
    [SerializeField] NetworkVariable<FixedString32Bytes> networkUsername = new NetworkVariable<FixedString32Bytes>(
        value: "unkown",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
        );


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerCamera = GameObject.FindWithTag("MainCamera");

            Cursor.lockState = CursorLockMode.Locked;

            playerCamera.transform.position = gameObject.transform.position + new Vector3(0, 0.9f, 0);
            playerCamera.transform.parent = gameObject.transform;

        }

        networkUsername.OnValueChanged += OnNetworkUsernameValueChanged;
        username.text = networkUsername.Value.ToString();

        if (IsOwner)
        {
            ClientBackend.OnClientEndUsernameChanged += OncClientUsernameChange;

            networkUsername.Value = ClientBackend.playerUsername;
        }

        base.OnNetworkSpawn();
    }

    void OncClientUsernameChange()
    {
        networkUsername.Value = ClientBackend.playerUsername;
        Debug.Log("username change event called");
    }

    void OnNetworkUsernameValueChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        networkUsername.Value = newValue;
        username.text = newValue.Value.ToString();
        Debug.Log("username value updated???");
    }

    private void XRotation()
    {
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
        playerCamera.transform.localRotation = Quaternion.Euler(cameraRotationEuler);
    }

    private void YRotation()
    {
        float y = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;

        gameObject.transform.Rotate(0, y, 0);
    }


    private float CalculateY()
    {
        float y = 0;

        if (characterController.isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                y = jumpForce;
            }
            else
            {
                velocity.y = -0.01f;
            }
        }
        else
        {
            y = Physics.gravity.y * gravityMult * Time.deltaTime;
        }
        return y;
    }

    private Vector3 CalculateXZ()
    {
        Vector3 xz = Vector3.zero;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            xz += Input.GetAxis("Vertical") * movementSpeed * sprintMult * transform.forward * Time.deltaTime;
            xz += Input.GetAxis("Horizontal") * movementSpeed * sprintMult * transform.right * Time.deltaTime;
        }
        else
        {
            xz += Input.GetAxis("Vertical") * movementSpeed * transform.forward * Time.deltaTime;
            xz += Input.GetAxis("Horizontal") * movementSpeed * transform.right * Time.deltaTime;
        }
        return xz;
    }

    private void CalculateVelocityChanges()
    {
        velocity.y = Mathf.Clamp(velocity.y, -terminalVelocity, terminalVelocity);
        velocity.x = Mathf.Clamp(velocity.x, -terminalVelocity, terminalVelocity);
        velocity.z = Mathf.Clamp(velocity.z, -terminalVelocity, terminalVelocity);
        velocity.x = velocity.x * (1 - Time.deltaTime * drag);
        velocity.z = velocity.z * (1 - Time.deltaTime * drag);
    }

    private void CalculateMovement()
    {
        Vector3 movement = new Vector3();

        movement = CalculateXZ();
        movement.y = CalculateY();

        velocity += movement;

        CalculateVelocityChanges();

        characterController.Move(velocity);
        playerCamera.transform.position = gameObject.transform.position + new Vector3(0, 0.9f, 0);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote)) 
        {
            if (playerLive)
            {
                playerLive = false;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                playerLive = true;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        try
        {
            if (playerLive)
            {
                XRotation();
                YRotation();

                CalculateMovement();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
