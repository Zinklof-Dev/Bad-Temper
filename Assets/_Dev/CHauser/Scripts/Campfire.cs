using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;

public class Campfire : NetworkBehaviour
{
    public static Vector3 _campfirePosition;

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

        _campfirePosition = transform.position;

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
                Debug.Log(OwnerClientId + "; " + campfireHealth.Value);
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

    public static async Task Initialize()
    {
        System.Random random = new System.Random(69);
        RaycastHit hit;
        int i = 0;
        int fails = 0;

        while (i < _positions.Length)
        {

            if (Physics.Raycast(new Vector3(_positions[i].x, 9999, _positions[i].y), Vector3.down, out hit))
            {
                if (hit.point.y <= 0)
                {
                    i++;
                    continue;
                }

                //_gameObject.transform.position = hit.point;
                _campfirePosition = hit.point;
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

                //_gameObject.transform.position = hit.point;
                _campfirePosition = hit.point;
                return;
            }
        }

        Debug.Log("Critical Campfire Failure");
    }
    public async static Task SpawnCampfire(Vector3 position)
    {
        _gameObject.transform.position = position;
    }
}
