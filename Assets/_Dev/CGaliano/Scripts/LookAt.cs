using System;
using Unity.Netcode;
using UnityEngine;

[AddComponentMenu("!ZinklofDev/" + "Utils/" + "LookAt")]
public class LookAt : NetworkBehaviour
{
    GameObject playerCamera = null;

    public override void OnNetworkSpawn()
    {
        try
        {
            playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        if (playerCamera = null)
        {
            Debug.LogError("LookAt.cs, can't find game object with tag (MainCamera)");
            Destroy(this);
        }
        base.OnNetworkSpawn();
    }
    
    public void FixedUpdate()
    {
        transform.LookAt(playerCamera.transform);
    }
}
