using UnityEngine;
using Unity.Netcode;
using ZinklofDev.Utils.Testing;
using System;
using ZinklofDev.Console;

public class BuildSystem : NetworkBehaviour
{
    [Header("Game Object Refrences")]

    [SerializeField] private GameObject playerCamera;
    // Cole | Place the buildable prefabs from the assets file into here
    [SerializeField] private GameObject[] placeableObjects;
    // Cole | Place the ghost objects in the scene in here
    [SerializeField] private GameObject[] ghostObjects;
    // Cole | List of all game objects with a "BuildingObjects" tag that are active in the scene
    [SerializeField] private GameObject[] buildObjectsInScene;

    [Header("Layer Masks")]

    [SerializeField] private LayerMask layerMask;

    [Header("Modifiable Variables")]

    [SerializeField] private static int currentObjectID;
    [SerializeField] private float playerReach;
    [SerializeField] public static bool isBuilding;

    private void Awake()
    {
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    public override void OnNetworkSpawn()
    {
        Shell.RegisterCommand(IS_BUILDING);
        Shell.RegisterCommand(CHANGE_BUILD_OBJECT_ID);

        base.OnNetworkSpawn();
    }

    void Update()
    {
        // Cole | Will make sure code only executes if player is building
        // Cole | Was going to reserve Object ID zero for this but thats dumb we can just use a bool that other scripts can edit 

        if (!isBuilding)
            return;
        
        buildObjectsInScene = GameObject.FindGameObjectsWithTag("BuildObjects");

        // Cole | Right now I have the floor object with an ID of one, but I will change it to zero in Flowers most likley.
        switch (currentObjectID)
        {
            case 0:
                FloorPlace();
                break;
        }
    }

    private void FloorPlace()
    {
        // FLOOR IS ID 0

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        GhostObject ghostObject = ghostObjects[0].GetComponent<GhostObject>();

        if (Physics.Raycast(ray, out hit, playerReach, layerMask))
        {
            ghostObjects[0].transform.position = new Vector3(RoundToMultipule(hit.point.x, 5), RoundToMultipule(hit.point.y, 5), RoundToMultipule(hit.point.z, 5));
        }
        else
        {
            ghostObjects[0].transform.position = ghostObject.defaultPosition;
        }

        if(ghostObject.isSpawnable == true)
        {
            if(Input.GetMouseButtonDown(1))
            {
                if(CheckProposedPlacement(ghostObjects[0].transform.position))
                    PlaceObjectInSceneRpc(ghostObjects[0].transform.position, 0);
            }
        }
    }

    private void RampPlace()
    {
        
    }

    private void WallPlace()
    {
        // Wall Code
    }

    private void FreePlace(int objectID)
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit; 

        if (Physics.Raycast(ray, out hit, playerReach, layerMask))
        {
            PlaceObjectInSceneRpc(hit.point, objectID);
        }
    }

    private bool CheckProposedPlacement(Vector3 proposedPosition)
    {
        foreach(GameObject gameObject in buildObjectsInScene)
        {
            if (gameObject.transform.position == proposedPosition)
                return false;
        }

        return true;
    }

    // Cole | Usually Mathf.Round rounds to the nearest whole number, but for the building grid system, I need to round to a multipule of certian values
    // Cole | Thank you to Bunny83 and dgoyette on Unity Discussions for the logic
    
    private static float RoundToMultipule(float inputValue, float baseNumberOfMultipule)
    {
        return Mathf.Round(inputValue / baseNumberOfMultipule) * baseNumberOfMultipule;
    }


    // Cole | Also thank you to Bunny83, this allows for the function to also take in an offset value for rounding

    private static float RoundToMultipule(float inputValue, float baseNumberOfMultipule, float offset)
    {
        return Mathf.Round((inputValue - offset) / baseNumberOfMultipule) * baseNumberOfMultipule + offset;
    }

    [Rpc(SendTo.Server)]
    private void PlaceObjectInSceneRpc(Vector3 spawnPos, int objectID)
    {
        GameObject spawnedObject = Instantiate(placeableObjects[objectID], spawnPos, transform.rotation);
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);
    }

    // Tests and Commands
    // Right now it's just tests but maybe commands coming soon idk

    public static Test RoundToMultipuleTest = new Test("BuildSystem.cs", () => 
    {
        float x = RoundToMultipule(2.6f, 2.5f);
        RoundToMultipuleTest.Expect(x, 2.5f);

        x = RoundToMultipule(69, 2.5f);
        RoundToMultipuleTest.Expect(x, 70f);

        x = RoundToMultipule(420.69f, 8);
        RoundToMultipuleTest.Expect(x, 424f);

    });

    public static Test RoundWithOffsetTest = new Test("BuildSystem.cs", () =>
    {
        float x = RoundToMultipule(0.1f, 5, 2.5f);
        RoundWithOffsetTest.Expect(x, 2.5f);

        x = RoundToMultipule(69, 6, 0.69f);
        RoundWithOffsetTest.Expect(x, 66.69f);

        x = RoundToMultipule(450, 420, 0.69f);
        RoundWithOffsetTest.Expect(x, 420.69f);
    });

    public static LegacyCommand<bool> IS_BUILDING = new LegacyCommand<bool>("0001x1500000003", "is_building", "Activates or deactivates build system.", false, (t1) =>
    {
        isBuilding = t1;
    });

    public static LegacyCommand<int> CHANGE_BUILD_OBJECT_ID = new LegacyCommand<int>("0001x1500000004", "change_build_object_id", "Changes the object you are placing in the scene", false, (t1) =>
    {
        currentObjectID = t1;
    });

}
