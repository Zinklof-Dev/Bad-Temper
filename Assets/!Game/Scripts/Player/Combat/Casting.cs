using System;
using Unity;
using UnityEngine;
using Unity.Netcode;

public class Casting : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] transform handTarget;
    [SerializeField] transform elbowTarget;
    [SerializeField] ParticleSystem healParticleSystem;
    [SerializeField] ParticleSystem fireballParticleSystem;
    [Header("IKTargets")]
    [SerializeField] vector3 handTargetTo;
    [SerializeField] vector3 elbowTargetTo;
    [SerializeField] vector3 handTargetToFireball;
    [SerializeField] vector3 elbowTargetToFireball;
    [Header("Spell Stats")]
    [SerializeField] float healCost = 100;
    [SerializeField] float healAmmount = 75;
    [SerializeField] float healCooldown = 8;
    [SerializeField] float healCastTime = 4;
    [SerializeField] float fireballCost = 75;
    [SerializeField] float fireballDamage = 75;
    [SerializeField] float fireballCooldown = 12;
    [SerializeField] float fireballCastTime = 2;

    private GameObject player;
    private GameObject cameraObj;
    private Stats stats;

    vector3 handTargetStart;
    vector3 elbowTargetStart;

    float lastFireball;
    float lastHeal;

    bool isCastingHeal;
    bool isCastingFireball;

    float timeSpentCasting;

    public bool leftIKLocked = false;

    public void Initialize(GameObject player)
    {
        this.player = player;
        cameraObj = GameObject.FindObjectByTag("MainCamera");
        stats = player.GetComponent<Stats>();
    }

    public void CastingHeal()
    {
        timeSpentCasting += Time.deltaTime;

        float lerpAmmount = MathF.Clamp(timeSpentCasting / (healCastTime / 2), 0, 1);

        handTarget.position = vector3.lerp(handTargetStart, handTargetTo + transform.position, lerpAmmount);
        eblowTarget.position = vector3.lerp(elbowTargetStart, elbowTargetTo + transform.position, lerpAmmount);

        if (timeSpentCasting / (healCastTime / 2) > 1)
        {
            // run particle system;
        }
        if (timeSpentCasting / (healCastTime / 2) > 2)
        {
            stats.Damage(-healAmmount);
            leftIKLocked = false;
            isCastingHeal = false;
            timeSpentCasting = -1;
        }
    }

    public void CastingFireball()
    {
        timeSpentCasting += Time.deltaTime;

        float lerpAmmount = MathF.Clamp(timeSpentCasting / (fireballCastTime / 2), 0, 1);

        handTarget.position = vector3.lerp(handTargetStart, handTargetToFireball + transform.position, lerpAmmount);
        eblowTarget.position = vector3.lerp(elbowTargetStart, elbowTargetToFireball + transform.position, lerpAmmount);

        if (timeSpentCasting / (fireballCastTime / 2) > 1)
        {
            // run particle system;
        }
        if (timeSpentCasting / (fireballCastTime / 2) > 2)
        {
            // summon fireball OBJ, give target, 

            leftIKLocked = false;
            isCastingHeal = false;
            timeSpentCasting = -1;
        }
    }

    public void Update()
    {
        if (player == null)
            return;

        lastFireball += Time.deltaTime;
        lastHeal += Time.deltaTime;

        if (isCastingHeal)
            CastingHeal();
        else if (isCastingFireball)
            CastingFireball();

        if (timeSpentCasting != -1)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (stats.UseMana(healCost) && lastHeal > healCooldown)
            {
                isCastingHeal = true;
                leftIKLocked = true;
                timeSpentCasting = 0;

                handTargetStart = handTarget.position;
                elbowTargetStart = elbowTarget.position;
            }
        }
    }
}