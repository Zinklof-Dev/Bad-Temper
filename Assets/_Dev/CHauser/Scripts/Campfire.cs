using UnityEngine;
using Unity.Netcode;

public class Campfire : NetworkBehaviour
{
    // Refrences to Game Objects and variables that are used client side
    // Can be effected by Network Variables, but aren't synced across the network

    [Header("Client Side Refrences")]
    [SerializeField] private GameObject campfirePrefab;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject camera;
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
        // Server Only- first write to the synced campfireHealth network variable

        if (IsServer)
            campfireHealth.Value = maxHealth;

        // Does all of the client side spawning and assigning refrences to needed refrences

        healTimer = healTime;
        Instantiate(campfirePrefab);
        healthBar = GameObject.FindGameObjectWithTag("CampfireHealthBar");
        camera = GameObject.FindGameObjectWithTag("MainCamera");
        campfireHealthRefrence = campfireHealth.Value;
        campfireHealth.OnValueChanged += UpdateCampfireHealthValue;

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
        if (healthBar == null)
        {
            return;
        }

        HealthBarLookAtCamera();

        if (!IsOwner)
            return;
        if (!IsServer) 
            return;

        HealCastle();
    }

    private void HealCastle()
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
                healTimer -= healSpeed * Time.deltaTime;
            }
        }
    }

    private void HealthBarLookAtCamera()
    {
        healthBar.transform.parent.LookAt(camera.transform);
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

    private void DamageCastle(float damage)
    {
        campfireHealth.Value -= damage;
    }
}
