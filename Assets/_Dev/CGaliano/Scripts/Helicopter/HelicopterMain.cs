using UnityEngine;

public class HelicopterMain : MonoBehaviour
{
    public Transform mainRotor;
    public Transform[] rotorPoints;
    public Transform tailRotor;
    public float liftForce;
    public float tiltAmmount;
    public float yawForce;
    public float dragFactor;
    public float throttleIncreaseFactor;

    public Rigidbody rb;

    private void Start()
    {
        rb.centerOfMass = Vector3.zero;
    }

    void OnDrawGizmos()
    {
        if (rotorPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform t in rotorPoints)
            {
                // Draw force direction
                Gizmos.DrawLine(t.position, t.position + t.up);
            }
        }

        if (tailRotor)
        {
            Gizmos.color = Color.red;
            // Draw tail rotor force
            Gizmos.DrawLine(tailRotor.position, tailRotor.position + tailRotor.up);
        }
    }

    void ApplyMainRotorForce()
    {
        float throttle = Mathf.Clamp(Input.GetAxis("Helicopter Throttle"), 0, 1);

        foreach (Transform t in rotorPoints)
        {
            Vector3 forceDir = mainRotor.rotation * Vector3.up;
            Vector3 force = forceDir * liftForce * throttle * Time.fixedDeltaTime;

            rb.AddForceAtPosition(force, t.position, ForceMode.Force);
        }
    }

    void ApplyTilt()
    {
        float pitch = Input.GetAxis("Helicopter Pitch") * tiltAmmount;
        float roll = Input.GetAxis("Helicopter Roll") * tiltAmmount;

        mainRotor.localRotation = Quaternion.Euler(pitch, 0, roll);
    }

    void ApplyYawForce()
    {
        Vector3 force = tailRotor.right * Input.GetAxis("Helicopter Yaw") * yawForce * Time.fixedDeltaTime;

        rb.AddForceAtPosition(force, tailRotor.position, ForceMode.Force);
    }

    void ApplyDrag()
    {
        // Simulate air drag
        rb.linearVelocity *= (1 - dragFactor * Time.fixedDeltaTime);
        rb.angularVelocity *= (1 - dragFactor * Time.fixedDeltaTime);
    }

    private void FixedUpdate()
    {
        ApplyMainRotorForce();
        ApplyYawForce();
        ApplyTilt();
        ApplyDrag();
    }
}
