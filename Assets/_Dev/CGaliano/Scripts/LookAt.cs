using System;
using UnityEngine;

[AddComponentMenu("!ZinklofDev/" + "Utils")]
public class LookAt : MonoBehaviour
{
    GameObject playerCamera = null;

    public void Awake()
    {
        try 
        {
            playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        if (playerCamera = null);
        {
            Debug.LogError("LookAt.cs, can't find game object with tag (MainCamera)");
            Destroy(this)
        }
    }
    
    public void FixedUpdate()
    {
        transform.LookAt(playerCamera);
    }
}
