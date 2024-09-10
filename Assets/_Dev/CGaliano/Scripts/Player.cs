using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player External Variables")]
    [SerializeField] public float health = 100f;
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
    [SerializeField] private Camera playerCamera;

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
            xz.z = Input.GetAxis("Vertical") * movementSpeed * sprintMult * Time.deltaTime;
            xz.x = Input.GetAxis("Horizontal") * movementSpeed * sprintMult * Time.deltaTime;
        }
        else
        {
            xz.z = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
            xz.x = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
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
    }

    private void Update()
    {
        try
        {
            XRotation();
            YRotation();
            Cursor.lockState = CursorLockMode.Confined;

            CalculateMovement();
        }
        catch
        {

        }
    }
}
