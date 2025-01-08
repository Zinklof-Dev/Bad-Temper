using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;

public class Campfire : NetworkBehaviour
{
    [SerializeField] private Vector2[] positions;
    private static Vector2[] _positions;
    private static GameObject _gameObject;


    // Refrences to Game Objects and variables that are used client side
    // Can be effected by Network Variables, but aren't synced across the network

    [Header("Client Side Refrences")]
    //[SerializeField] private GameObject campfirePrefab;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Server server;
    [SerializeField] private float campfireHealthRefrence;

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
        _positions = positions;
        _gameObject = gameObject;

        // Server Only- first write to the synced campfireHealth network variable

        if (IsServer)
            campfireHealth.Value = maxHealth;

        // Does all of the client side spawning and assigning refrences to needed refrences

        healTimer = healTime;
        //Instantiate(campfirePrefab);
        healthBar = GameObject.FindGameObjectWithTag("CampfireHealthBar");
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
        // Makes sure we aren't executing the code and getting a million error messages from Unity before the health bar gets refrenced when Network Spawn happens
        if (healthBar == null)
            return;

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

    private static async Task<Vector3> GetTriangleNormal(int index, Mesh mesh)
    {
        int vertex0Index = mesh.triangles[index * 3]; // First vertex index
        int vertex1Index = mesh.triangles[index * 3 + 1]; // Second vertex index
        int vertex2Index = mesh.triangles[index * 3 + 2]; // Third vertex index

        Vector3 v0 = mesh.vertices[vertex0Index];
        Vector3 v1 = mesh.vertices[vertex1Index];
        Vector3 v2 = mesh.vertices[vertex2Index];
        
        (Debug.Log("Vertex Positions: " + v0 + v1 + v2);

        return Vector3.Cross(v1 - v0, v2 - v0);
    }

    public static async Task Initialize(int seed, GameObject loadingManagerObject)
    {
        System.Random random = new System.Random(seed * 69 / 420 + 69);

        MeshFilter terrainMeshFilter = loadingManagerObject.GetComponent<MeshFilter>();
        UnityEngine.Mesh mesh = terrainMeshFilter.sharedMesh;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;


        RaycastHit hit;
        int i = 0;
        int fails = 0;

        while (i < _positions.Length)
        {

            if (Physics.Raycast(new Vector3(_positions[i].x, 9999, _positions[i].y), Vector3.down, out hit))
            {
                //mesh = hit.transform.gameObject.GetComponent<Mesh>();

                if (hit.point.y <= 0)
                {
                    i++;
                    continue;
                }

                Vector3 triangleNormal = await GetTriangleNormal(hit.triangleIndex, mesh);
                Debug.Log("Triangle Normal: " + triangleNormal);

                if(Mathf.Abs(triangleNormal.x) > 0.5f || Mathf.Abs(triangleNormal.x) > 0.5 || Mathf.Abs(triangleNormal.x) > 0.5)
                {
                    i++;
                    Debug.Log("Failed because of incline");
                    continue;
                }

                _gameObject.transform.position = hit.point;
                return;
            }
        }

        while (fails < 30)
        {
            if (Physics.Raycast(new Vector3(random.Next(0, 1000), 9999, random.Next(0, 1000)), Vector3.down, out hit))
            {
                if (hit.point.y <= 0)
                {
                    fails++;
                    continue;
                }

                Vector3 triangleNormal = await GetTriangleNormal(hit.triangleIndex, mesh);
                Debug.Log("Triangle Normal: " + triangleNormal);

                if (Mathf.Abs(triangleNormal.x) > 0.5f || Mathf.Abs(triangleNormal.x) > 0.5 || Mathf.Abs(triangleNormal.x) > 0.5)
                {
                    i++;
                    Debug.Log("Failed because of incline");
                    continue;
                }

                _gameObject.transform.position = hit.point;
                return;
            }
        }

        Debug.Log("Critical Campfire Failure");
    }
}
