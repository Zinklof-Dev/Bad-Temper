using Unity;
using Unity.Netcode;
using System;
using UnityEngine;

public class StartGame : NetworkBehaviour
{
    [SerializeField] string[] sceneNames;
    
    public void ForceLoadScene(int type)
    {
        Debug.LogWarning("Game is being started from ForceLoadScene, may cause problems if not all players are truly ready.");
        
        if (type < 0 || type > sceneNames.Length-1)
        {
            Debug.LogError("StartGame.cs was told to load a scene type outside of its bounds");
            return;
        }
        //else
            //NetworkManager.SceneManager.LoadScene(sceneNames[type]);
    }
}