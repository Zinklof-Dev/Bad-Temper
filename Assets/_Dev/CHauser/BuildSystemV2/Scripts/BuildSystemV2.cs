using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using ZinklofDev.ConsoleV2;

public class BuildSystemV2 : NetworkBehaviour
{
    private static bool isBuilding = false;
    private static int currentObjectID;

    [SerializeField] private GameObject[] ghostObjects;
    [SerializeField] private GameObject[] buildPrefabs;
    [SerializeField] private GameObject playerCamera;

    [SerializeField] private float playerReach;

    [SerializeField] private LayerMask placeableLayerMask;

    public override void OnNetworkSpawn()
    {
        playerCamera = GameObject.FindWithTag("MainCamera");

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!isBuilding)
            return;

        switch(currentObjectID) 
        { 
            case 0:
                PlaceFoundation();
                break;
            case 1:
                break;
        }
    }

    private void PlaceFoundation()
    {
        RaycastHit hit;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, playerReach, placeableLayerMask))
        {
            GhostObject ghostObject = ghostObjects[0].GetComponent<GhostObject>();
            if (ghostObject.isSpawnable)
            {
                ghostObject.gameObject.transform.position = hit.point;
                if(Input.GetMouseButtonDown(0))
                {
                    PlaceObjectRpc(hit.point, ghostObject.gameObject.transform.rotation, 0);
                }
            }
            else
            {
                ghostObject.gameObject.transform.position = ghostObject.defaultPosition;
            }
        }
    }

    private void PlaceWall()
    {

    }

    private void PlaceCeling()
    {

    }

    private void PlaceRamp()
    {

    }

    [Rpc(SendTo.Server)]
    private void PlaceObjectRpc(Vector3 spawnPos, Quaternion rotation, int objectID)
    {
        GameObject spawnedObject = Instantiate(buildPrefabs[objectID], spawnPos, rotation);
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);
    }

    [Command("Activates or deactivates build system.")]
    public static void Building(bool _isBuilding)
    {
        isBuilding = _isBuilding;
    }

    [Command("Changes the object you are placing in the scene.")]
    public static void BuildID(int ID)
    {
        currentObjectID = ID;
    }
}
