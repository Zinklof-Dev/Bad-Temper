using UnityEngine;

public class GhostObject : MonoBehaviour
{
    public bool isSpawnable;
    public float raduisOfCheck;
    public Vector3 defaultPosition;
    bool done = false;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, raduisOfCheck);
    }

    private void Update()
    {
        if (transform.position == defaultPosition)
            isSpawnable = false;

        Collider[] thingsInBounds = Physics.OverlapSphere(transform.position, raduisOfCheck); 

        foreach(Collider collider in thingsInBounds)
        {
            if(!done)
            {
                isSpawnable = true;
            }

            if(collider.gameObject.tag == "BuildingObjects")
            {
                isSpawnable = false;
                done = true;
            }
        }
    }
}
