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
    [SerializeField] private float gravity = Physics.gravity.y;
    [SerializeField] private Vector3 velocity;
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
            movement.y = gravity * Time.deltaTime;
        }
    }

    private void Update()
    {
    }
}
