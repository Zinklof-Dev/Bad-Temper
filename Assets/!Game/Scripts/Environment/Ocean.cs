using UnityEngine;

public class Ocean : MonoBehaviour
{
    [SerializeField] GameObject playerCamera;

    private void FixedUpdate()
    {
        float x = RoundToMultipule(playerCamera.transform.position.x, 30);
        float z = RoundToMultipule(playerCamera.transform.position.z, 30);

        this.transform.position = new Vector3(x, 0, z);
    }

    private static float RoundToMultipule(float inputValue, float baseNumberOfMultipule)
    {
        return Mathf.Round(inputValue / baseNumberOfMultipule) * baseNumberOfMultipule;
    }
}
