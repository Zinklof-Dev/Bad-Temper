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

    private float lastJump;

    public override void OnNetworkSpawn()
    {
        //subscribe to the event for the value of the username getting changed for this GameObject.
        networkUsername.OnValueChanged += OnNetworkUsernameValueChanged;

        //if the client running this code owns the object its attached to this is true
        if (IsOwner)
        {
            //get the main camera, lock the cursor, move the camera to the player objects position with the default offset i always use, and make it a child of the player object on this clients end only.
            playerCamera = GameObject.FindWithTag("MainCamera");

            Cursor.lockState = CursorLockMode.Locked;

            playerCamera.transform.position = gameObject.transform.position + new Vector3(0, 0.9f, 0);
            playerCamera.transform.parent = gameObject.transform;


            //subscribe to the client backend event for changing your username.
            ClientBackend.OnClientEndUsernameChanged += OncClientUsernameChange;

            //set the current username value to the current username value in the clientbackend class, likley set elsewhere like the main menu.
            networkUsername.Value = ClientBackend.playerUsername;
        }
        //now let unity do its usual stuff
        base.OnNetworkSpawn();
    }

    void OncClientUsernameChange()
    {
        //when the client changes their username, using the method in the clientbackend, change the value of the network variable which will then sync to everyone else without us calling an RPC
        networkUsername.Value = ClientBackend.playerUsername;
        //Debug.Log("username change event called");
    }

    void OnNetworkUsernameValueChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        //unity calls this event when the network variable value is changed, we ackowledge that change by changing our local value to the new one, then change the TMP object
        networkUsername.Value = newValue;
        username.text = newValue.Value.ToString();
        //Debug.Log("username value updated???");
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
        playerCamera.transform.localRotation = Quaternion.Euler(cameraRotationEuler);
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


    private float CalculateY()
    {
        //temp float for local math
        float y = 0;

        //iirc this bool is true when a raycast sent down from the player hits something, not the guy who made the default unity character controller though
        if (characterController.isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space) && lastJump > 0.25f)
            {
                //Debug.Log("playerJumping");
                y = jumpForce;
                lastJump = 0;
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
        //Temp Vector to calculate the X and Z axis movements, not the best choice for memory mangement, two floats, or a vector2 would be better, but this is more performant than doing math to get
        //tranform.forward and transform.right to work kindly with regular floats or a Vector2
        Vector3 xz = Vector3.zero;

        xz += Input.GetAxis("Vertical") * transform.forward;
        xz += Input.GetAxis("Horizontal") * transform.right;

        xz = xz.normalized;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            //if pressing shift, apply sprint mult to movement code
            xz *= movementSpeed * sprintMult * Time.deltaTime;
        }
        else
        {
            //else do same code but without the sprint mult
            xz *= movementSpeed * Time.deltaTime;
        }

        if (!characterController.isGrounded)
        {
            xz = xz * 0.05f;
        }
        return xz;
    }

    private void CalculateVelocityChanges()
    {
        //clamping the velocity so you don't have some bug send the player into the eather realm
        velocity.y = Mathf.Clamp(velocity.y, -terminalVelocity, terminalVelocity);
        velocity.x = Mathf.Clamp(velocity.x, -terminalVelocity, terminalVelocity);
        velocity.z = Mathf.Clamp(velocity.z, -terminalVelocity, terminalVelocity);


        /*
         * I actually hate you kerbus, this function is NOT safe from variable frame rates...
         * to be fair kerbus is not at fault i am for not doing the math and realizing what this code does and just kinda using it and being happy it works.
         * For the potential SkillsUSA person reading this later this year or next year;
         * I have used this exact method for all of my character controllers over the years, originally getting from Unity Awnsers back when i had zero clue what I was doing.
         * For the first time, this year, I didn't use V sync for some reason... and realized there was less drag... can't believe that took me three years to fix this...
         * velocity.x = velocity.x * (1 - Time.deltaTime * drag);
         * velocity.z = velocity.z * (1 - Time.deltaTime * drag);
         */

        //NEW VARIABLE FRAMERATE SAFE*** Method to apply drag
        if (characterController.isGrounded)
        {
            velocity.z = velocity.z - ((velocity.z * drag) * Time.deltaTime);
            velocity.x = velocity.x - ((velocity.x * drag) * Time.deltaTime);
        }
        else
        {
            velocity.z = velocity.z - ((velocity.z * (drag * 0.05f)) * Time.deltaTime);
            velocity.x = velocity.x - ((velocity.x * (drag * 0.05f)) * Time.deltaTime);
        }
    }

    private void CalculateMovement()
    {
        //Create temp varible, C# garbage collection will deal with later.
        Vector3 movement;

        //Give this temp Vector values
        movement = CalculateXZ();
        movement.y = CalculateY();

        //Apply this temporary Vector to the velocity Vector
        velocity += movement;

        Debug.Log((velocity.magnitude * 2.237f) + " MPH");

        //Call the function to clamp velocity to terminal velocity and apply drag.
        CalculateVelocityChanges();

        characterController.Move(velocity);
    }

    private void Update()
    {
        if (!IsOwner) return;

        lastJump += Time.deltaTime;
        
        //locks or unlocks the player cursor, movement, and ability to look if the tilde key is pressed, this allows the player to type, and use thier mouse when they open the console without the player moving everywhere
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

        //request all our movement, and look code is run, wrapped in a try catch statement for not really any real reason, this isn't calling any networked code and is pretty simple stuff so it wont fail catastrophically
        //but... you never know i guess, so past me decided a try catch was worth it.
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
