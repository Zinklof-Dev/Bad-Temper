using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using ZinklofDev.Utils.Testing;
using ZinklofDev.Console;
using ZinklofDev.Utils.MathZ;

public class BuildSystem : NetworkBehaviour
{
    [Header("Game Object Refrences")]
    // Cole | Don't touch yourself, refrence gets assigned in the OnNetworkSpawn function
    [SerializeField] private GameObject playerCamera;
    // Cole | Place the buildable prefabs from the assets file into here
    [SerializeField] private GameObject[] placeableObjects;
    // Cole | Place the ghost objects in the scene in here
    // Cole | I have the ghost objects as prefabs right now just so I can have them persist beyond being scene dependent, but when we put this in the actual game scene we'll unpack them and put them here.
    [SerializeField] private GameObject[] ghostObjects;

    [Header("Layer Masks")]

    [SerializeField] private LayerMask layerMask;
    [SerializeField] private LayerMask floorLayerMask;

    [Header("Modifiable Variables")]
    
    [SerializeField] private float playerReach;

    private static int currentObjectID;
    public static bool isBuilding;

    public override void OnNetworkSpawn()
    {
        // Cole | All commands must be registered with the shell
        Shell.RegisterCommand(IS_BUILDING);
        Shell.RegisterCommand(BUILD_ID);
        // Cole | Assigns the player camera refrence
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
        // Cole | Allows for the function to execute what it needs to do because of the ovveride.
        base.OnNetworkSpawn();
    }

    void Update()
    {
        // Cole | Will make sure code only executes if player is building
        // Cole | Will also make sure that all ghost objects are at default position if player is not building
        // Cole | Was going to reserve Object ID zero for this but thats dumb we can just use a bool that other scripts can edit 

        if (!isBuilding)
        {
            foreach(GameObject ghost in ghostObjects)
            {
                GhostObject ghostObject = ghost.GetComponent<GhostObject>();
                ghost.transform.position = ghostObject.defaultPosition;
            }
            
            return;
        }

        // Cole | Makes sure that the ghost object is at it's default position if  that object ID is not selected
        // Cole | Fixes bug that if you change your object ID while the ghost object is not at it's default position, the ghost object would stay in the open
        
        int i = 0;
        foreach(GameObject ghost in ghostObjects)
        {
            if(i != currentObjectID)
            {
                GhostObject ghostObject = ghost.GetComponent<GhostObject>();
                ghost.transform.position = ghostObject.defaultPosition;
            }

            i++;
        }
        
        switch (currentObjectID)
        {
            case 0:
                FloorPlace();
                break;
            case 1:
                RampPlace();
                break;
            case 2:
                WallPlace(); 
                break;
            default:
                FreePlace(currentObjectID);
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
            ghostObjects[0].transform.position = new Vector3(RoundToMultipule(hit.point.x, 2.5f), RoundToMultipule(hit.point.y, 2.5f), RoundToMultipule(hit.point.z, 2.5f));
        }
        else
        {
            ghostObjects[0].transform.position = ghostObject.defaultPosition;
        }

        if(ghostObject.isSpawnable == true)
        {
            if(Input.GetMouseButtonDown(1))
            {
                PlaceObjectInSceneRpc(ghostObjects[0].transform.position, transform.rotation, 0);
            }
        }
    }

    private void RampPlace()
    {
        // RAMP ID IS 1
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        GhostObject ghostObject = ghostObjects[1].GetComponent<GhostObject>();

        if (Physics.Raycast(ray, out hit, playerReach, layerMask))
        {
             Collider[] floorColliders = Physics.OverlapSphere(hit.point, 5, floorLayerMask);
             if(CheckColliderArray(floorColliders))
            {
                Collider closestFloor = ClosestCollider(floorColliders, hit);
                ghostObjects[1].transform.position = new Vector3(closestFloor.gameObject.transform.position.x, closestFloor.gameObject.transform.position.y + 1.25f, closestFloor.gameObject.transform.position.z);
                ghostObject.rotation = Quaternion.Euler(-45f, RoundToMultipule(playerCamera.transform.eulerAngles.y, 90), 0);
            }
        }
        else
        {
            ghostObjects[1].transform.position = ghostObject.defaultPosition;
        }

        if(ghostObject.isSpawnable == true)
        {
            if(Input.GetMouseButtonDown(1))
            {
                PlaceObjectInSceneRpc(ghostObjects[1].transform.position, ghostObject.rotation, 1);
            }
        }
    }

    private void WallPlace()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        GhostObject ghostObject = ghostObjects[2].GetComponent<GhostObject>();

        if (Physics.Raycast(ray, out hit, playerReach, layerMask))
        {
            Collider[] floorColliders = Physics.OverlapSphere(hit.point, 5, floorLayerMask);
            if(CheckColliderArray(floorColliders))
            {
                Collider closestFloor = ClosestCollider(floorColliders, hit);
                FloorObject floorObject = closestFloor.gameObject.GetComponent<FloorObject>();
                List<WallPoint> wallPoints = floorObject.GetWallPoints();

                if (CheckWallPointList(wallPoints))
                {
                    WallPoint closestWallPoint = FindClosestWallPoint(wallPoints, closestFloor.gameObject, hit);
                    ghostObjects[2].transform.position = closestFloor.gameObject.transform.position + closestWallPoint.pos;
                    ghostObject.rotation = Quaternion.Euler(closestWallPoint.eulerRotation);
                }
            }
        }
        else
        {
            ghostObjects[2].transform.position = ghostObject.defaultPosition;
        }

        if (ghostObject.isSpawnable == true)
        {
            if (Input.GetMouseButtonDown(1))
            {
                PlaceObjectInSceneRpc(ghostObjects[2].transform.position, ghostObject.rotation, 2);
            }
        }
    }

    private bool CheckColliderArray(Collider[] colliders)
    {
        bool isNotNull = false;
        foreach(Collider collider in colliders)
        {
            isNotNull = true;
        }

        return isNotNull;
    }

    private Collider ClosestCollider(Collider[] colliders, RaycastHit hit)
    {
        Collider closestCollider = colliders[1];
        int i = 0;
        foreach(Collider collider in colliders)
        {
            if(i == 0)
                closestCollider = collider;
            else
            {
                if(Vectors.SqrDist3f(hit.point, collider.gameObject.transform.position) < Vectors.SqrDist3f(hit.point, closestCollider.gameObject.transform.position))
                {
                     closestCollider = collider;
                }
            }
            i++;
        }
        return closestCollider;
    }

    private WallPoint FindClosestWallPoint(List<WallPoint> wallPoints, GameObject closestFloor, RaycastHit hit)
    {
        WallPoint closestWallPoint = wallPoints[0];
        int i = 0;
        foreach(WallPoint wallPoint in wallPoints)
        {
            if(i == 0)
                closestWallPoint = wallPoint;
            else
            {
                if(Vectors.SqrDist3f(hit.point, closestFloor.transform.position + wallPoint.pos) < Vectors.SqrDist3f(hit.point, closestFloor.transform.position + closestWallPoint.pos))
                {
                    closestWallPoint = wallPoint;
                }
            }
            i++;
        }

        return closestWallPoint;
    }

    private bool CheckWallPointList(List<WallPoint> wallPoints)
    {
        bool isNotNull = false;
        foreach (WallPoint wallPoint in wallPoints)
        {
            isNotNull = true;
        }
        return isNotNull;
    }

    private void FreePlace(int objectID)
    {
        // FREE PLACE OBJECTS ARE ID 3+
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        GhostObject ghostObject = ghostObjects[objectID].GetComponent<GhostObject>();

        if (Physics.Raycast(ray, out hit, playerReach, layerMask))
        {
            ghostObjects[objectID].transform.position = hit.point;
            ghostObject.rotation = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0);
        }
        else
        {
            ghostObjects[objectID].transform.position = ghostObject.defaultPosition;
        }

        if (ghostObject.isSpawnable == true)
        {
            if (Input.GetMouseButtonDown(1))
            {
                PlaceObjectInSceneRpc(ghostObjects[objectID].transform.position, ghostObject.rotation, objectID);
            }
        }
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
    private void PlaceObjectInSceneRpc(Vector3 spawnPos, Quaternion rotation, int objectID)
    {
        GameObject spawnedObject = Instantiate(placeableObjects[objectID], spawnPos, rotation);
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);
    }

    // Tests and (Legacy) Commands

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

    public static LegacyCommand<int> BUILD_ID = new LegacyCommand<int>("0001x1500000004", "build_id", "Changes the object you are placing in the scene", false, (t1) =>
    {
        currentObjectID = t1;
    });

}
