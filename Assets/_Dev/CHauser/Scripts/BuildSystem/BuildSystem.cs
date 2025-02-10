using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using ZinklofDev.Utils.Testing;
using ZinklofDev.Console;
using ZinklofDev.ConsoleV2;
using ZinklofDev.Utils.MathZ;
using Unity.VisualScripting;

[System.Serializable]
public struct SnapPoint
{
    public Vector3 position;
    public Vector3 eulerRotation;
    public Quaternion quaternionRotation { get; private set; }

    public SnapPoint(Vector3 position, Vector3 eulerRotation)
    {
        this.position = position;
        this.eulerRotation = eulerRotation;
        quaternionRotation = Quaternion.Euler(eulerRotation);
    }
}

public class BuildSystem : NetworkBehaviour
{
    #region Snap Points
    // All points are just offsets we then add to the positions of whatever base object we're building off of
    private static SnapPoint[] floorWallPoints = 
    { 
        new SnapPoint(new Vector3(1.25f, 1.25f, 0), new Vector3(0, 0, 90)), 
        new SnapPoint(new Vector3(-1.25f, 1.25f, 0), new Vector3(0, 0, 90)), 
        new SnapPoint(new Vector3(0, 1.25f, 1.25f), new Vector3(90, 0, 0)),
        new SnapPoint(new Vector3(0, 1.25f, -1.25f), new Vector3(90, 0, 0))
    };
    private static SnapPoint[] foundationWallPoints =
    {
        new SnapPoint(new Vector3(1.25f, 1.75f, 0), new Vector3(0, 0, 90)),
        new SnapPoint(new Vector3(-1.25f, 1.75f, 0), new Vector3(0, 0, 90)),
        new SnapPoint(new Vector3(0, 1.75f, 1.25f), new Vector3(90, 0, 0)),
        new SnapPoint(new Vector3(0, 1.75f, -1.25f), new Vector3(90, 0, 0))
    };
    private static SnapPoint[] foundationFoundationPoints =
    {
        new SnapPoint(new Vector3(2.5f, 0, 0), new Vector3(0,0,0)),
        new SnapPoint(new Vector3(-2.5f, 0, 0), new Vector3(0,0,0)),
        new SnapPoint(new Vector3(0, 0, 2.5f), new Vector3(0,0,0)),
        new SnapPoint(new Vector3(0, 0, -2.5f), new Vector3(0,0,0))
    };
    private static SnapPoint[] wallFloorPoints =
    {
        new SnapPoint(new Vector3(1.25f, 1.3f, 0), new Vector3(0, 0, 0)),
        new SnapPoint(new Vector3(-1.25f, 1.3f, 0), new Vector3(0, 0, 0)),
        new SnapPoint(new Vector3(0, 1.3f, 1.25f), new Vector3(0, 0, 0)),
        new SnapPoint(new Vector3(0, 1.3f, -1.25f), new Vector3(0, 0, 0))
    };
    #endregion

    [Header("Game Object Refrences (DON'T TOUCH YOURSELF)")]
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
    [SerializeField] private LayerMask foundationLayerMask;
    [SerializeField] private LayerMask terrainLayerMask;
    [SerializeField] private LayerMask wallLayerMask;

    [Header("Modifiable Variables")]
    
    [SerializeField] private float playerReach;

    private static int currentObjectID;
    public static bool isBuilding;

    public override void OnNetworkSpawn()
    {
        // Cole | All legacy commands must be registered with the shell
        ZinklofDev.Console.Shell.RegisterCommand(IS_BUILDING);
        ZinklofDev.Console.Shell.RegisterCommand(BUILD_ID);
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
                FoundationPlace();
                break;
            case 1:
                FloorPlace();
                break;
            case 2:
                RampPlace();
                break;
            case 3:
                WallPlace(); 
                break;
            default:
                FreePlace(currentObjectID);
                break;
        }
    }

    private void FoundationPlace()
    {
        RaycastHit hit;
        GhostObject ghostObject = ghostObjects[0].GetComponent<GhostObject>();

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, playerReach, terrainLayerMask))
        {
            Collider[] foundationColliders = Physics.OverlapSphere(hit.point, 2.5f, foundationLayerMask);

            if (foundationColliders.Length == 0)
            {
                ghostObject.gameObject.transform.position = hit.point;
            }
            else
            {
                Collider closestCollider = ClosestCollider(foundationColliders, hit);
                SnapPoint closestSnapPoint = FindClosestSnapPoint(foundationFoundationPoints, closestCollider.gameObject, hit);
                ghostObject.transform.position = closestCollider.transform.position + closestSnapPoint.position;
                ghostObject.rotation = closestSnapPoint.quaternionRotation;
            }
        }
        else
        {
            ghostObject.gameObject.transform.position = ghostObject.defaultPosition;
        }

        if (ghostObject.isSpawnable)
        {
            if (Input.GetMouseButtonDown(1))
            {
                PlaceObjectInSceneRPC(ghostObject.transform.position, ghostObject.gameObject.transform.rotation, 0);
            }
        }
    }

    private void FloorPlace()
    {
        // FLOOR IS ID 1
        RaycastHit hit;
        GhostObject ghostObject = ghostObjects[1].GetComponent<GhostObject>();
        GameObject criticalObject = null;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, playerReach))
        {
            Collider[] wallColliders = Physics.OverlapSphere(hit.point, 5, wallLayerMask);

            if(wallColliders.Length == 0)
            {
                ghostObject.transform.position = ghostObject.defaultPosition;
                return;
            }

            Collider closestWall = ClosestCollider(wallColliders, hit);
            criticalObject = closestWall.gameObject;

            if(closestWall.transform.rotation.eulerAngles.z == 90)
            {
                if(Vectors.SqrDist3f(hit.point, closestWall.transform.position + wallFloorPoints[0].position) <= Vectors.SqrDist3f(hit.point, closestWall.transform.position + wallFloorPoints[1].position))
                {
                    ghostObject.transform.position = closestWall.transform.position + wallFloorPoints[0].position;
                }
                if (Vectors.SqrDist3f(hit.point, closestWall.transform.position + wallFloorPoints[0].position) >= Vectors.SqrDist3f(hit.point, closestWall.transform.position + wallFloorPoints[1].position))
                {
                    ghostObject.transform.position = closestWall.transform.position + wallFloorPoints[1].position;
                }
            }
            if(closestWall.transform.rotation.eulerAngles.x == 90)
            {
                if (Vectors.SqrDist3f(hit.point, closestWall.transform.position + wallFloorPoints[2].position) <= Vectors.SqrDist3f(hit.point, closestWall.transform.position + wallFloorPoints[3].position))
                {
                    ghostObject.transform.position = closestWall.transform.position + wallFloorPoints[2].position;
                }
                if (Vectors.SqrDist3f(hit.point, closestWall.transform.position + wallFloorPoints[2].position) >= Vectors.SqrDist3f(hit.point, closestWall.transform.position + wallFloorPoints[3].position))
                {
                    ghostObject.transform.position = closestWall.transform.position + wallFloorPoints[3].position;
                }
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
                PlaceObjectInSceneRPC(ghostObjects[1].transform.position, transform.rotation, 1, true, criticalObject);
            }
        }
    }

    private void RampPlace()
    {
        // RAMP ID IS 2
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        GhostObject ghostObject = ghostObjects[2].GetComponent<GhostObject>();
        GameObject criticalObject = null;

        if (Physics.Raycast(ray, out hit, playerReach))
        {
             Collider[] floorColliders = Physics.OverlapSphere(hit.point, 5, floorLayerMask + foundationLayerMask);

             if(floorColliders.Length <= 0)
             {
                ghostObjects[2].transform.position = ghostObject.defaultPosition;
                return;
             }

            Collider closestFloor = ClosestCollider(floorColliders, hit);
            criticalObject = closestFloor.gameObject;

            if(closestFloor.gameObject.layer == 7)
                ghostObjects[2].transform.position = new Vector3(closestFloor.gameObject.transform.position.x, closestFloor.gameObject.transform.position.y + 1.25f, closestFloor.gameObject.transform.position.z);
            if (closestFloor.gameObject.layer == 8)
                ghostObjects[2].transform.position = new Vector3(closestFloor.gameObject.transform.position.x, closestFloor.gameObject.transform.position.y + 1.75f, closestFloor.gameObject.transform.position.z);

            ghostObject.rotation = Quaternion.Euler(-45f, RoundToMultipule(playerCamera.transform.eulerAngles.y, 90), 0);
        }
        else
        {
            ghostObjects[2].transform.position = ghostObject.defaultPosition;
        }

        if(ghostObject.isSpawnable == true)
        {
            if(Input.GetMouseButtonDown(1))
            {
                PlaceObjectInSceneRPC(ghostObjects[2].transform.position, ghostObject.rotation, 2, true, criticalObject);
            }
        }
    }

    private void WallPlace()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        GhostObject ghostObject = ghostObjects[3].GetComponent<GhostObject>();
        GameObject criticalObject = null;

        if (Physics.Raycast(ray, out hit, playerReach))
        {
            Collider[] floorColliders = Physics.OverlapSphere(hit.point, 2.5f, floorLayerMask + foundationLayerMask);

            if (floorColliders.Length == 0)
            {
                ghostObjects[3].transform.position = ghostObject.defaultPosition;
                return;
            }

            Collider closestCollider = ClosestCollider(floorColliders, hit);
            criticalObject = closestCollider.gameObject;

            if (closestCollider.gameObject.layer == 8) // Foundation Layer is Layer 8
            {
                SnapPoint closestSnapPoint = FindClosestSnapPoint(foundationWallPoints, closestCollider.gameObject, hit);
                ghostObject.transform.position = closestCollider.gameObject.transform.position + closestSnapPoint.position;
                ghostObject.rotation = Quaternion.Euler(closestSnapPoint.eulerRotation);
            }
            else if (closestCollider.gameObject.layer == 7) // Floor Layer is Layer 7
            {
                SnapPoint closestSnapPoint = FindClosestSnapPoint(floorWallPoints, closestCollider.gameObject, hit);
                ghostObject.transform.position = closestCollider.gameObject.transform.position + closestSnapPoint.position;
                ghostObject.rotation = Quaternion.Euler(closestSnapPoint.eulerRotation);
            }
            else
            {
                ghostObjects[3].transform.position = ghostObject.defaultPosition;
            }
        }
        else
        {
            ghostObjects[3].transform.position = ghostObject.defaultPosition;
        }

        if (ghostObject.isSpawnable == true)
        {
            if (Input.GetMouseButtonDown(1))
            {
                PlaceObjectInSceneRPC(ghostObjects[3].transform.position, ghostObject.rotation, 3, true, criticalObject);
            }
        }
    }

    private Collider ClosestCollider(Collider[] colliders, RaycastHit hit)
    {
        Collider closestCollider = colliders[0];
        int i = 0;
        foreach(Collider collider in colliders)
        {
            if (i == 0)
            {
                closestCollider = collider;
                i++;
                continue;
            }

            if (Vectors.SqrDist3f(hit.point, collider.gameObject.transform.position) < Vectors.SqrDist3f(hit.point, closestCollider.gameObject.transform.position))
            {
                closestCollider = collider;
            }

            i++;
        }
        return closestCollider;
    }

    private SnapPoint FindClosestSnapPoint(SnapPoint[] snapPoints, GameObject originObject, RaycastHit hit)
    {
        SnapPoint closestSnapPoint = snapPoints[0];
        int i = 0;
        foreach(SnapPoint snapPoint in snapPoints)
        {
            if (i == 0)
            {
                closestSnapPoint = snapPoint;
                i++;
                continue;
            }
            
            if(Vectors.SqrDist3f(hit.point, originObject.transform.position + snapPoint.position) < Vectors.SqrDist3f(hit.point, originObject.transform.position + closestSnapPoint.position))
            {
                closestSnapPoint = snapPoint;
            }

            i++;
        }

        return closestSnapPoint;
    }

    private void FreePlace(int objectID)
    {
        // FREE PLACE OBJECTS ARE ID 4+
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        GhostObject ghostObject = ghostObjects[objectID].GetComponent<GhostObject>();
        GameObject criticalObject = null;

        if (Physics.Raycast(ray, out hit, playerReach, floorLayerMask + foundationLayerMask))
        {
            criticalObject = hit.transform.gameObject;
            if (hit.transform.gameObject.layer == 8 && hit.point.y == hit.transform.position.y + 0.5f)
            {
                ghostObjects[objectID].transform.position = hit.point;
                ghostObject.rotation = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0);
            }
            if (hit.transform.gameObject.layer == 7 && hit.point.y >= hit.transform.position.y + 0.054f)
            {
                ghostObjects[objectID].transform.position = hit.point;
                ghostObject.rotation = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0);
            }
        }
        else
        {
            ghostObjects[objectID].transform.position = ghostObject.defaultPosition;
        }

        if (ghostObject.isSpawnable == true)
        {
            if (Input.GetMouseButtonDown(1))
            {
                PlaceObjectInSceneRPC(ghostObjects[objectID].transform.position, ghostObject.rotation, objectID, true, criticalObject);
            }
        }
    }

    // Cole | Usually Mathf.Round rounds to the nearest whole number, but for the building grid system, I need to round to a multipule of certian values
    // Cole | Thank you to Bunny83 and dgoyette on Unity Discussions for the logic
    
    private static float RoundToMultipule(float inputValue, float baseNumberOfMultipule)
    {
        return Mathf.Round(inputValue / baseNumberOfMultipule) * baseNumberOfMultipule;
    }

    // Cole | Also thank you to Bunny83, this allows for the function to also take in an tOffset value for rounding

    private static float RoundToMultipule(float inputValue, float baseNumberOfMultipule, float tOffset)
    {
        return Mathf.Round((inputValue - tOffset) / baseNumberOfMultipule) * baseNumberOfMultipule + tOffset;
    }

    [Rpc(SendTo.Server)]
    private void PlaceObjectInSceneRPC(Vector3 spawnPos, Quaternion rotation, int objectID, bool hasCriticalObject = false, NetworkObjectReference criticalObjectReference = new NetworkObjectReference())
    {
        GameObject spawnedObject = Instantiate(placeableObjects[objectID], spawnPos, rotation);
        if(hasCriticalObject)
        {
                criticalObjectReference.TryGet(out NetworkObject criticalObject);
                BuildObject buildObject = spawnedObject.GetComponent<BuildObject>();
                buildObject.hasCriticalObject = true;
                buildObject.criticalObject = criticalObject.gameObject;
        }
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);
    }

    #region Commands

    // V2 Commands

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
    #endregion

    #region Tests and Legacy Commands

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
    #endregion
}
