using UnityEngine;

public class FloorObject : MonoBehaviour
{
    [SerializeField] private Vector3[] wallSpots;
    private float offset = 0.5f;

    private void Start()
    {
        wallSpots[0] = new Vector3(transform.position.x + offset, transform.position.y, transform.position.z);
        wallSpots[1] = new Vector3(transform.position.x - offset, transform.position.y, transform.position.z);
        wallSpots[2] = new Vector3(transform.position.x, transform.position.y, transform.position.z + offset);
        wallSpots[3] = new Vector3(transform.position.x, transform.position.y, transform.position.z + offset);
    }
}