using System;
using Unity.Netcode;
using UnityEngine;

[AddComponentMenu("!ZinklofDev/" + "Utils/" + "LookAt")]
public class LookAt : NetworkBehaviour
{
    GameObject playerCamera = null;
    [SerializeField] GameObject overrideObj = null;
    
    public void FixedUpdate()
    {
        //if (playerCamera == null && overrideObj == null)
        //{
        //    playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
        //}

        //if (overrideObj == null)
        //{
        //    transform.LookAt(playerCamera.transform);
        //}
        //else
        //{
            transform.LookAt(overrideObj.transform.position);

            transform.localRotation = transform.localRotation * Quaternion.Euler(0, -90, 0);
        //}
    }
}
