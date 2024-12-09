using System.Security.Cryptography.X509Certificates;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayersManager : NetworkBehaviour
{
    [SerializeField] ulong[] slots = new ulong[6];
    [SerializeField] FixedString32Bytes[] usernames = new FixedString32Bytes[6];
    [SerializeField] GameObject[] playerObjects = new GameObject[6];
    [SerializeField] ulong clientId;
    [SerializeField] DataPersistanceManager dataPersistanceManager;

    FixedString32Bytes localUsername;



    public override void OnNetworkSpawn()
    {
        localUsername = dataPersistanceManager.GetData().username;

        Debug.Log("start OnNetworkSpawn");
        for(int i = 0; i <6; i++)
        {
            Debug.Log("for loop itteration " + i);
            slots[i] = (ulong)99999999;
            usernames[i] = (FixedString32Bytes)"NoPlAyEr";
        }

        AskForIdRpc();
        GiveServerUsernameRpc(localUsername);

        base.OnNetworkSpawn();
    }

    void ReconfigurePlayerScreen() // run locally on each client after they receive a new name. this will update all names on hud that are active, and activate any that dont have the keyword for no player. this isn't the best solution but its a solution for now. the biggest issue is the no player keyword that a player connecting with their name being that wont be shown on the main menu.
    {
        for(int i = 0; i < 6; i++)
        {
            GameObject go = playerObjects[i]; // fetch object from the array

            /*if (go is null) // null case, shouldn't happen but you never know
            {
                go = GameObject.Find("PlayerName" + i); // find the object
                
                if (go is null) // if that failed skip this itteration of the loop
                {
                    Debug.LogError("couldn't find player HUD object, error not fatal, user experience may be harmed though");
                    continue;
                }
                else // otherwise save the object for later use
                playerObjects[i] = go;
            }*/

            // skipped null case just to see what happens, having issue with the null case being triggered even when it has a reference to the gameobject, maybe to do with it being inactive?? 

            if (usernames[i] != "NoPlAyEr") // if the slot doesn't have the no player keyword, then enable it
            {
                go.SetActive(true);
            }

            else // otherwise set it false (to be sure)
            {
                go.SetActive(false); //redundant call but just making absolute sure.
                continue;
            }

            Debug.Log("go transformCount: " + go.transform.childCount);

            GameObject goChild = go.transform.GetChild(go.transform.childCount-1).gameObject;
            
            TextMeshProUGUI tmp = goChild.GetComponent<TextMeshProUGUI>(); // fetch the tmp component

            // add null case soon
            
            tmp.text = usernames[i].ToString(); // change tmp text to username text
        }
    }

    [Rpc(SendTo.Server)]
    void AskForIdRpc(RpcParams rpcParams = default)
    {
        clientId = rpcParams.Receive.SenderClientId;

        Debug.Log(clientId);

        for(int i = 0; i < 6; i++)
        {
            if (slots[i] == 99999999)
            {
                slots[i] = clientId;
                break;
            }
        }

        ReturnIdtoClientRpc(clientId, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.Server)]
    void GiveServerUsernameRpc(FixedString32Bytes username, RpcParams rpcParams = default)
    {
        clientId = rpcParams.Receive.SenderClientId;
        bool nameSaved = false;
        int finalIndex = 0;

        for (int i = 0; i < 6; i++)
        {
            if (slots[i] == clientId)
            {
                usernames[i] = username;
                nameSaved = true;
                finalIndex = i;
                break;
            }
        }

        if (nameSaved == false)
        {
            Debug.LogError("failed to save username, canceling RPC");
            return;
        }
        else
        {
            SendUsernamesAcrossNetworkRpc(usernames);
            return;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SendUsernamesAcrossNetworkRpc(FixedString32Bytes[] newUsernames, RpcParams rpcParams = default)
    {
        for (int i = 0; i < 6; i++)
        {
            Debug.Log("name[" + i + "]: " + newUsernames[i]);
        }

        usernames = newUsernames;
        ReconfigurePlayerScreen();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnIdtoClientRpc(ulong returnedId, RpcParams rpcParams = default) 
    {
        clientId = returnedId;
    }
}
