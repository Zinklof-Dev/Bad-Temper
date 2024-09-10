using System;
using System.Net;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using ZinklofDev.Console;

public class NetworkCommands : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    private void Awake()
    {
        Shell.RegisterCommand(HOST);
        Shell.RegisterCommand(CONNECT);
    }

    Command HOST =  new Command("0001x8800000000", "host", "starts server", false, ()=>
    {
        host();    
    });

    Command<string> CONNECT = new Command<string>("0001x8800000001", "connect", "connects to server", false, (t1) =>
    {
        Connect(t1);
    });

    public static void host()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log(IPV4toHex.IPV4ToHexadecimal(IPV4toHex.IPV4ToHexadecimal(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString())));
    }

    public static void Connect(string hostID)
    {
        try
        {
            string targetIP = IPV4toHex.HexadecimalToIPV4(hostID);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(targetIP, 7777);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }    
}
