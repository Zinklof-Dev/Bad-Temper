using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using ZinklofDev.ConsoleV2;

public class Campfire : NetworkBehaviour
{
    private static GameObject _gameObject;
    public static Campfire campfire;
    public static Vector3 _position;

    // Variables to go through coords of placement
    static int incremental = 10;
    static int distanceToTravel = 10;
    static int distanceTraveled = 0;
    static int direction = 0;
    static int x = 0;
    static int y = 0;

    // Refrences to Game Objects and variables that are used client side
    // Can be effected by Network Variables, but aren't synced across the network

    [Header("Client Side Refrences")]
    //[SerializeField] private GameObject campfirePrefab;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Server server;
    [SerializeField] private float campfireHealthRefrence;
    private SpriteRenderer healthBarSpriteRenderer;

    [Space(10)]

    // These are the variables for all of the math that happens on the Host, Clients don't need them, so they don't use them

    [Header("Variables Only Needed With Host")]
    [SerializeField] private float healSpeed = 1f;
    [SerializeField] private float healAmount = 1;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float healTime = 10;
    private float healTimer;

    [Space(10)]

    // Network variable that allows for the value to be synced across the network

    [Header("Variables Synced Across Network")]
    [SerializeField] private NetworkVariable<float> campfireHealth = new NetworkVariable<float>(0);

    public override void OnNetworkSpawn()
    {
        campfire = this;
        _gameObject = gameObject;

        // Server Only- first write to the synced campfireHealth network variable

        if (IsServer)
            campfireHealth.Value = maxHealth;

        // Does all of the client side spawning and assigning refrences to needed refrences

        healTimer = healTime;
        //Instantiate(campfirePrefab);
        healthBar = GameObject.FindGameObjectWithTag("CampfireHealthBar");
        healthBarSpriteRenderer = healthBar.GetComponent<SpriteRenderer>();
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
        server = GameObject.FindGameObjectWithTag("Server").GetComponent<Server>();
        campfireHealthRefrence = campfireHealth.Value;

        // Network variables have an event called "OnValueChanged" that allows the client to update their refrence to the Network variable
        campfireHealth.OnValueChanged += UpdateCampfireHealthValue;

        // Makes it so that the function executes every server tick
        server.ServerTick += HealCampfire;

        // Allows for base OnNetworkSpawn() function to do it's job, adds stuff back we overrided

        base.OnNetworkSpawn();
    }

    private void UpdateCampfireHealthValue(float oldValue, float newValue)
    {
        // Allows client to get a refrence of the synced campfireHealth variable whenever it changes. Also makes it so that health bar updates only when the synced value updates

        campfireHealthRefrence = newValue;
        UpdateHealthBar();
    }

    private void Update()
    {
        _position = transform.position;

        // Makes sure we aren't executing the code and getting a million error messages from Unity before the health bar gets refrenced when Network Spawn happens
        if (healthBar != null)
            HealthBarLookAtCamera();
    }

    private void HealCampfire()
    { 
        if(campfireHealth.Value > maxHealth)
            campfireHealth.Value = maxHealth;

        if(campfireHealth.Value <  maxHealth)
        {
            if (healTimer <= 0)
            {
                campfireHealth.Value += healAmount;
                //Debug.Log(OwnerClientId + "; " + campfireHealth.Value);
                healTimer = healTime;
            }
            else
            {
                healTimer -= healSpeed * Server.serverDeltaTime;
            }
        }
    }

    private void HealthBarLookAtCamera()
    {
        healthBar.transform.parent.LookAt(playerCamera.transform);
    }

    private void UpdateHealthBar()
    {
        float percentage = campfireHealthRefrence / maxHealth;
        healthBar.transform.localScale = new Vector3(percentage * .96f, 0.65f, 1);
        if(percentage > 0.75f)
        {
            healthBarSpriteRenderer.color = Color.green;
        }
        else if(percentage > 0.5f)
        {
            healthBarSpriteRenderer.color = Color.yellow;
        }
        else if(percentage > 0.25f)
        {
            healthBarSpriteRenderer.color = new Color(1, 0.5f, 0); // Orange
        }
        else
        {
            healthBarSpriteRenderer.color = Color.red;
        }
    }

    /* private void CheckIfEndGame()
    {
        if (campfireHealth.Value == 0)
        {

        }
    } */

    private void DamageCampfire(float damage)
    {
        campfireHealth.Value -= damage;
    }

    #region Campfire Placement Code

    public static async Task Initialize(int seed, GameObject loadingManagerObject)
    {
        System.Random random = new System.Random(seed * 69 / 420 + 69);
        MeshFilter terrainMeshFilter = loadingManagerObject.GetComponent<MeshFilter>();
        RaycastHit hit;


        for (int i = 0; i < 9800 /*Dialed this in to be the perfect number of iterations to cheeck every single point on the terrain that is 10 units apart (500 units out is excluded) */; i++)
        {
            if (Physics.Raycast(new Vector3(x, 9999, y), Vector3.down, out hit))
            {
                if (hit.point.y <= 0)
                {
                    await GetNextCoordinates();
                    continue;
                }

                Vector3 trianglePosition = await GetTrianglePosition(hit.triangleIndex, terrainMeshFilter.sharedMesh);

                if (trianglePosition.y <= 0)
                {
                    await GetNextCoordinates();
                    continue;
                }

                Quaternion triangleQuaternionRotation = await GetTriangleQuaternionRotation(hit.triangleIndex, terrainMeshFilter.sharedMesh);

                if ((triangleQuaternionRotation.eulerAngles.x > 20 && triangleQuaternionRotation.eulerAngles.x < 340) || (triangleQuaternionRotation.eulerAngles.z > 20 && triangleQuaternionRotation.eulerAngles.z < 340))
                {
                    await GetNextCoordinates();
                    Debug.Log("Failed because of incline");
                    continue;
                }

                _gameObject.transform.position = trianglePosition;
                _gameObject.transform.rotation = Quaternion.Euler(-triangleQuaternionRotation.eulerAngles.x, 0, -triangleQuaternionRotation.eulerAngles.z);

                return;
            }
        }

        // Debug.Log("Campfire position: \n X: " + x + " Y: " + y);

        for (int fails = 0; fails < 30; fails++)
        {
            if (Physics.Raycast(new Vector3(random.Next(0, 1000), 9999, random.Next(0, 1000)), Vector3.down, out hit))
            {
                if (hit.transform.gameObject.GetComponent<MeshFilter>() == null)
                {
                    continue;
                }

                Vector3 trianglePosition = await GetTrianglePosition(hit.triangleIndex, terrainMeshFilter.sharedMesh);

                if (trianglePosition.y <= 0)
                {
                    continue;
                }

                Quaternion triangleQuaternionRotation = await GetTriangleQuaternionRotation(hit.triangleIndex, terrainMeshFilter.sharedMesh);

                if ((triangleQuaternionRotation.eulerAngles.x > 20 && triangleQuaternionRotation.eulerAngles.x < 340) || (triangleQuaternionRotation.eulerAngles.z > 20 && triangleQuaternionRotation.eulerAngles.z < 340))
                {
                    Debug.Log("Failed because of incline");
                    continue;
                }

                _gameObject.transform.position = trianglePosition;
                _gameObject.transform.rotation = Quaternion.Euler(-triangleQuaternionRotation.eulerAngles.x, 0, -triangleQuaternionRotation.eulerAngles.z);

                return;
            }
        }

        Debug.Log("Critical Campfire Failure, Campfire will be placed at 0,0");

        // If all else fails, the campfire will just be put at (0,0). If no mesh is collided with, than the y value is automatically 0.
        if (Physics.Raycast(new Vector3(0, 999, 0), Vector3.down, out hit))
        {
            if (hit.transform.gameObject.GetComponent<MeshFilter>() == null)
                return;
            Vector3 trianglePosition = await GetTrianglePosition(hit.triangleIndex, terrainMeshFilter.sharedMesh);
            Quaternion triangleQuaternionRotation = await GetTriangleQuaternionRotation(hit.triangleIndex, terrainMeshFilter.sharedMesh);
            _gameObject.transform.position = trianglePosition;
            _gameObject.transform.rotation = Quaternion.Euler(-triangleQuaternionRotation.eulerAngles.x, 0, -triangleQuaternionRotation.eulerAngles.z);
        }
    }

    private static async Task<Quaternion> GetTriangleQuaternionRotation(int index, Mesh mesh)
    {
        Vector3 v0 = mesh.vertices[mesh.triangles[index * 3]];
        Vector3 v1 = mesh.vertices[mesh.triangles[index * 3 + 1]];
        Vector3 v2 = mesh.vertices[mesh.triangles[index * 3 + 2]];
        Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
        Quaternion quaternionRotation = Quaternion.FromToRotation(normal, Vector3.up);

        //Debug.Log("Triangle Positions: " + v0 + v1 + v2+ "\nRotation of triangle: " +  quaternionRotation.eulerAngles);

        return quaternionRotation;
    }

    private static async Task<Vector3> GetTrianglePosition(int index, Mesh mesh)
    {
        Vector3 v0 = mesh.vertices[mesh.triangles[index * 3]];
        Vector3 v1 = mesh.vertices[mesh.triangles[index * 3 + 1]];
        Vector3 v2 = mesh.vertices[mesh.triangles[index * 3 + 2]];

        return new Vector3((v0.x + v1.x + v2.x) / 3, (v0.y + v1.y + v2.y) / 3, (v0.z + v1.z + v2.z) / 3);
    }

    public static async Task GetNextCoordinates()
    {
        // Right
        if (direction == 0)
        {
            x += incremental;
            distanceTraveled += incremental;

            if (distanceTraveled == distanceToTravel)
            {
                direction++;
                distanceTraveled = 0;
            }
        }
        // Up
        else if (direction == 1)
        {
            y += incremental;
            distanceTraveled += incremental;

            if (distanceTraveled == distanceToTravel)
            {
                direction++;
                distanceTraveled = 0;
                distanceToTravel += incremental;
            }
        }
        // Left
        else if (direction == 2)
        {
            x -= incremental;
            distanceTraveled += incremental;

            if (distanceTraveled == distanceToTravel)
            {
                direction++;
                distanceTraveled = 0;
            }
        }
        // down
        else if (direction == 3)
        {
            y -= incremental;
            distanceTraveled += incremental;

            if (distanceTraveled == distanceToTravel)
            {
                direction = 0;
                distanceTraveled = 0;
                distanceToTravel += incremental;
            }
        }
    }

    #endregion

    [Command("Changes the health of the campfire")]
    public static void ChangeHealth(float health)
    {
        if(!campfire.IsServer)
        {
            Console.Log("Only Server Can Execute This Command!", "ChangeHealth");
            return;
        }

        campfire.campfireHealth.Value = health;
        Console.Log("New Health: " + campfire.campfireHealth.Value, "ChangeHealth");
    }
}
