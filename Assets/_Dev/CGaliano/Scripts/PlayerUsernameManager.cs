using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerUsernameManager : NetworkBehaviour
{

    [SerializeField] string playerName = "player";
    [SerializeField] private TextMeshPro usernameObject;
    byte playerID;
    Server serverObject;

    private void Awake()
    {
        if (!IsOwner)
        {
            return;
        }

        serverObject = GameObject.FindGameObjectWithTag("ServerObject").GetComponent<Server>();

        playerID = Server.getID();
        serverObject.SubscribeNameUpdate(playerID, usernameObject);

        serverObject.ChangeName(playerID, playerName);
    }

    private void ChangeName(string newName)
    {
        playerName = newName;
        serverObject.ChangeName(playerID, playerName);
    }

    private void UpdateName()
    {
        serverObject.ChangeName(playerID, playerName);
    }
}
