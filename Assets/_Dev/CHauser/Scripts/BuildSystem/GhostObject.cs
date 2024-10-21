using UnityEngine;

public class GhostObject : MonoBehaviour
{
    public bool isSpawnable; 
    public Vector3 defaultPosition;

    private void Update()
    {
        if (transform.position == defaultPosition)
            isSpawnable = false;
        else
            isSpawnable = true;
    }
}
