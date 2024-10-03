using System;
using Unity.Netcode;
using UnityEngine;

[AddComponentMenu("!ZinklofDev/" + "Utils/" + "LookAt")]
public class LookAt : NetworkBehaviour
{
    GameObject playerCamera = null;
    
    public void FixedUpdate()
    {
        if (playerCamera == null)
        {
            playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        transform.LookAt(playerCamera.transform);
    }
}
