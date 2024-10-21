using UnityEngine;

public class GhostObject : MonoBehaviour
{
    public bool isSpawnable; 
    public Vector3 defaultPosition;
    public LayerMask layerMask;

    private void Update()
    {
        if (CheckForCollision())
            isSpawnable = true;
        else
            isSpawnable = false;

        if (transform.position == defaultPosition)
            isSpawnable = false;
        else
            isSpawnable = true;
    }

    private bool CheckForCollision()
    {
        Collider[] colliders = Physics.OverlapBox(gameObject.transform.position, transform.localScale / 2, Quaternion.Identity, layerMask);
        if (colliders[0] == null)
            return true;
        else
            return false; 
    }
}
