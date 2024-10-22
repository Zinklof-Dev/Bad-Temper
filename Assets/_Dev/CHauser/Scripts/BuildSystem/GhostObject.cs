using UnityEngine;

public class GhostObject : MonoBehaviour
{
    public bool isSpawnable; 
    public Vector3 defaultPosition;
    public LayerMask layerMask;

    private void Update()
    {
        if (transform.position == defaultPosition)
            isSpawnable = false;
        else
        {
            if (CheckForCollision())
                isSpawnable = true;
            else
                isSpawnable = false;
        }
    }

    private bool CheckForCollision()
    {
        // Cameron | 10/22/2024 A.D 05:58 EST | Compiler error in the next line, Quaternion.Identity doesn't exist, fixed it for you. its a lowercase i :) 
        Collider[] colliders = Physics.OverlapBox(gameObject.transform.position, transform.localScale / 2, Quaternion.identity, layerMask);
        if (colliders[0] == null)
            return true;
        else
            return false; 
    }
}
