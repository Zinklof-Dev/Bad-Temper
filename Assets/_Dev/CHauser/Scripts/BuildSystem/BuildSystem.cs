using UnityEngine;
using Unity.Netcode;
using ZinklofDev.Utils.Testing;
using System;

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

    [SerializeField] private int currentObjectID;
    [SerializeField] private float playerReach;
    [SerializeField] public bool isBuilding;

    private void Awake()
    {
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {
        // Cole | Will make sure code only executes if player is building
        // Cole | Was going to reserve Object ID zero for this but thats dumb we can just use a bool that other scripts can edit 
        // Cole | I need to add commands, I'm going to either add a class to this script or make a commands script, or try to figure out how to add them to this script.
        if (!isBuilding)
            return;
        
        // Cole | Probably need to change tag, will check later in Flowers
        buildObjectsInScene = GameObject.FindGameObjectsWithTag("BuildingObject");

        // Cole | Right now I have the floor object with an ID of one, but I will change it to zero in Flowers most likley.
        switch (currentObjectID)
        {
            case 1:
                FloorPlace();
                break;
        }
    }

    private void FloorPlace()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        GhostObject ghostObject = ghostObjects[1].GetComponent<GhostObject>();

        if (Physics.Raycast(ray, out hit, playerReach, layerMask))
        {
            ghostObjects[1].transform.position = new Vector3(RoundToMultipule(hit.point.x, 5), RoundToMultipule(hit.point.y, 5), RoundToMultipule(hit.point.z, 5));
        }
        else
        {
            ghostObjects[1].transform.position = ghostObject.defaultPosition;
        }

        if(ghostObject.isSpawnable == true)
        {
            if(Input.GetMouseButtonDown(1))
            {
                if(CheckProposedPlacement(ghostObjects[1].transform.position))
                    PlaceObjectInSceneRpc(ghostObjects[1].transform.position);
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
        foreach(GameObject in buildObjectsInScene)
        {
            if(buildObjectsInScene.transform.position == proposedPosition)
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
    
}
