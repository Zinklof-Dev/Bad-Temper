using TMPro;
using System;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using ZinklofDev.ConsoleV2;

public class Player : NetworkBehaviour
{
    [Header("Player External Variables")]
    [SerializeField] public float health = 100f;
    [SerializeField] public float maxHealth = 100f;
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
        );

    private float lastJump;
    [SerializeField] private float lastSpeed;

    private static Player playerClass = null;
    [SerializeField] private Player playerClassNonStatic = null;

    public override void OnNetworkSpawn()
    {
        //subscribe to the event for the value of the username getting changed for this GameObject.
        networkUsername.OnValueChanged += OnNetworkUsernameValueChanged;

        //if the client running this code owns the object its attached to this is true
        if (IsOwner)
        {
            //get the main camera, lock the cursor, move the camera to the player objects position with the default tOffset i always use, and make it a child of the player object on this clients end only.
            playerCamera = GameObject.FindWithTag("MainCamera");

            Cursor.lockState = CursorLockMode.Locked;

            playerCamera.transform.position = gameObject.transform.position + new Vector3(0, 0.9f, 0);
            playerCamera.transform.parent = gameObject.transform;

            //subscribe to the client backend event for changing your username.
            ClientBackend.OnClientEndUsernameChanged += OncClientUsernameChange;

            //set the current username value to the current username value in the clientbackend class, likley set elsewhere like the main menu.
            networkUsername.Value = ClientBackend.playerUsername;

            // Save reference to this playerclass, and playerobject, for command use later
            playerClass = this;
            playerClassNonStatic = playerClass;
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
            if (Input.GetKeyDown(KeyCode.Space) && lastJump > 0.25f && !ZinklofDev.ConsoleV2.Console.isOpen)
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

        if (!ZinklofDev.ConsoleV2.Console.isOpen)
        {
            xz += Input.GetAxis("Vertical") * transform.forward;
            xz += Input.GetAxis("Horizontal") * transform.right;
        }

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

        //Debug.Log(velocity.magnitude * 50 + " MPS | " + (velocity.magnitude * 50) * 2.237f + " MPH" );

        //Call the function to clamp velocity to terminal velocity and apply drag.
        CalculateVelocityChanges();

        characterController.Move(velocity * 100 * Time.deltaTime);
    }

    private void ApplyFallDamage()
    {
        if (!characterController.isGrounded)
        {
            //Debug.Log("logged lastSpeedValue");
            lastSpeed = MathF.Abs(velocity.y * 50); //50 follows the convention of every unity unit being one meter in my games, the 50 was gotten from testing this current char controler as it moved past cubes.
        }

        if (characterController.isGrounded && lastSpeed > 4) //this should only ever run the frame your character hits the ground.
        {
            //Debug.Log("entered intto the apply dmg");
            //i'll be basing this off a NASA study on ejection systems and how they affect human bodies, 12 Meters Per Second seem to be survivable but cause life changing injuries, 17 MPS is certain death

            float damage = ((lastSpeed - 4) * (lastSpeed - 4) / 140) * maxHealth; //This may seem super random, but its a cheap, basic exponential function, you take 0 damage at 4 mps, 45.7% of your health is lost at 12 mps, and 100% at roughly 15.85 mps
            health -= damage;
            lastSpeed = 0;
        }
    }

    [Command("Teleports the player to the provided position", true)]
    public static void Teleport(float x, float y, float z)
    {
        if (playerClass == null)
        {
            ZinklofDev.ConsoleV2.Console.Log("Player Commands cannot be run when no player exists (IE you're in the game scene)", "Teleport");
            return;
        }
        else if (!playerClass.IsServer)
        {
            ZinklofDev.ConsoleV2.Console.Log("You lack sufficient permission to run this command (IE server only command)", "Teleport");
            return;
        }
        else
        {
            playerClass.characterController.transform.position = new Vector3(x, y, z);
            ZinklofDev.ConsoleV2.Console.Log("New player position is: " + playerClass.gameObject.transform.position, "Teleport");
        }
    }

    // this override isn't a command as the new command system cannot construct Vector3's, at least yet, this is primarily for the times we actually need to teleport the player
    // It also lacks the server check as it doesn't need to ensure its the server (unless exploited which at that point they can likely change the variable IsServer anyways) as it can only be called by code rather than by command or user input.
    public static void Teleport(Vector3 pos)
    {
        if (playerClass = null)
        {
            Debug.LogError("Attempted to TP player that does not exist");
        }
        else
        {
            playerClass.characterController.transform.position = pos;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        lastJump += Time.deltaTime;

        //request all our movement, and look code is run, wrapped in a try catch statement for not really any real reason, this isn't calling any networked code and is pretty simple stuff so it wont fail catastrophically
        //but... you never know i guess, so past me decided a try catch was worth it.
        try
        {
            if (!ZinklofDev.ConsoleV2.Console.isOpen)
            {
                XRotation();
                YRotation();
            }

            CalculateMovement();

            //ApplyFallDamage();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
