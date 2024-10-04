using Unity.Netcode;
using UnityEngine;

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
            PlaceObjectInSceneServerRpc(hit.point, objectID);
        }
    }

    [ServerRpc]
    private void PlaceObjectInSceneServerRpc(Vector3 spawnPos, int objectID)
    {
        GameObject spawnedObject = Instantiate(placeableObjects[objectID], spawnPos, transform.rotation);
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);
    }
}
