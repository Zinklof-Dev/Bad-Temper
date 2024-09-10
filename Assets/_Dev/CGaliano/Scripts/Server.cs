using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Server : NetworkBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(this);

        player1Name.OnValueChanged += (string previousValue, string newValue) =>
        {
            username1.text = newValue;
        };
    }

    public static byte getID()
    {
        if (lastID == 6)
        {
            Debug.LogWarning("too many IDs taken, kicking client");
            Application.Quit();
            return 0;
        }
        byte id = (byte)(lastID + 1);
        lastID++;
        return id;
    }

    private static byte lastID = 0;

    NetworkVariable<string> player1Name;
    NetworkVariable<string> player2Name;
    NetworkVariable<string> player3Name;
    NetworkVariable<string> player4Name;
    NetworkVariable<string> player5Name;
    NetworkVariable<string> player6Name;

    TextMeshPro username1;
    TextMeshPro username2;
    TextMeshPro username3;
    TextMeshPro username4;
    TextMeshPro username5;
    TextMeshPro username6;

    public void ChangeName(byte ID, string newName)
    {
        switch (ID)
        {
            case 1:
                player1Name.Value = newName;
                break;
            case 2:
                player2Name.Value = newName;
                break;
            case 3:
                player3Name.Value = newName;
                break;
            case 4:
                player4Name.Value = newName;
                break;
            case 5:
                player5Name.Value = newName;
                break;
            case 6:
                player6Name.Value = newName;
                break;
        }
    }

    public void SubscribeNameUpdate(byte ID, TextMeshPro nameObject)
    {
        switch (ID)
        {
            case 1:
                username1 = nameObject;
                break;
            case 2:
                username2 = nameObject;
                break;
            case 3:
                username3 = nameObject;
                break;
            case 4:
                username4 = nameObject;
                break;
            case 5:
                username5 = nameObject;
                break;
            case 6:
                username6 = nameObject;
                break;
        }
    }
}
