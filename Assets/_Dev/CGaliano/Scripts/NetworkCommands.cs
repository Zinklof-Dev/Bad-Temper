using System;
using System.Net;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using ZinklofDev.Console;
using ZinklofDev.ConsoleV2;

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
        Shell.RegisterCommand(ClientBackend.CHANGEUSERNAME);
    }

    LegacyCommand HOST =  new LegacyCommand("0001x8800000000", "host", "starts server", false, ()=>
    {
        host();    
    });

    LegacyCommand<string> CONNECT = new LegacyCommand<string>("0001x8800000001", "connect", "connects to server", false, (t1) =>
    {
        Connect(t1);
    });

    [Command("Starts hosting", false, "Host")]
    public static void host()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log(IPV4toHex.IPV4ToHexadecimal(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString()));
    }

    [Command("Connects to the specified join ID", false, "Connect")]
    public static void Connect(string hostID)
    {
        try
        {
            string targetIP = IPV4toHex.HexadecimalToIPV4(hostID);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(targetIP, 7777);
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }    
}
