using UnityEngine;
using Unity.Netcode;

public class BuildSystem : NetworkBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject[] placeableObjects;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private int currentObjectID;
    [SerializeField] private float playerReach;

    private void Awake()
    {
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            FreePlace(currentObjectID);
    }

    private void FreePlace(int objectID)
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit; 

        Debug.Log("Start");

        if (Physics.Raycast(ray, out hit, playerReach, layerMask))
        {
            PlaceObjectInSceneRpc(hit.point, objectID);
        }
    }

    [Rpc(SendTo.Server)]
    private void PlaceObjectInSceneRpc(Vector3 spawnPos, int objectID)
    {
        GameObject spawnedObject = Instantiate(placeableObjects[objectID], spawnPos, transform.rotation);
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);
    }
}
