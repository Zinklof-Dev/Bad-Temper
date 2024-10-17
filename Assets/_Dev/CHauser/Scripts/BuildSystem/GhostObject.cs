using UnityEngine;

public class GhostObject : MonoBehaviour
{
    public bool isSpawnable;
    public Vector3 defaultPosition;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "BuildObjects")
            isSpawnable = false;
    }

    private void OnTriggerExit(Collider other)
    {
            isSpawnable = true;
    }

    private void Update()
    {
        if (transform.position == defaultPosition)
            isSpawnable = false;

        else
            isSpawnable = true;
    }
}
