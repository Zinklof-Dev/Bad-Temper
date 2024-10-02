using UnityEngine;

public class BuildSystem : MonoBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject[] placeableObjects;
    [SerializeField] private LayerMask layerMask;

    private void Awake()
    {
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            FreePlace(0);
    }

    private void FreePlace(int objectID)
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        Debug.Log("Start");

        if (Physics.Raycast(ray, out hit, 100, layerMask))
        {
            Instantiate(placeableObjects[objectID], hit.point, transform.rotation);
            Debug.Log("Done");
        }
    }
}
