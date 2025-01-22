using UnityEngine;

public class GhostObject : MonoBehaviour
{
    public bool isSpawnable; 
    public Vector3 defaultPosition;
    public Quaternion rotation;

    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Material material;
    [SerializeField] private Vector3 size;


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

            if (isSpawnable)
                material.color = new Color(0, 1, 0, 0.4f);
            else
                material.color = new Color(1, 0, 0, 0.4f);
        }

        transform.rotation = rotation;
    }

    private void OnApplicationQuit()
    {
        material.color = new Color(1, 1, 1, 0.4f);
    }

    private bool CheckForCollision()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(size.x / 2.01f, size.y / 2, size.z / 2.01f), rotation, layerMask);
        bool collidersFound = false;

        foreach (Collider collider in colliders)
        {
            collidersFound = true;
        }

        if (!collidersFound)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
