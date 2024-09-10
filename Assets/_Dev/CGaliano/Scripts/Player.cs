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
    [Space(20)]
    [Header("References")]
    [SerializeField] private CharacterController characterController;

    private void CalculateMovement()
    {
        Vector3 movement = new Vector3();
        
        movement.z = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        movement.x = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;

        if (characterController.isGrounded)
        {
            movement.y = -0.01f;
        }
        else
        {
            movement.y = Physics.gravity.y * gravityMult * Time.deltaTime;
        }

        velocity += movement;

        velocity.y = Mathf.Clamp(velocity.y, -terminalVelocity, terminalVelocity);
        velocity.x = Mathf.Clamp(velocity.x, -terminalVelocity, terminalVelocity);
        velocity.z = Mathf.Clamp(velocity.z, -terminalVelocity, terminalVelocity);
        velocity = velocity * (1 - Time.deltaTime * drag);

        characterController.Move(velocity);
    }

    private void Update()
    {
        CalculateMovement();

    }
}
