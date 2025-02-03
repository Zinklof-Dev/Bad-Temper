using System;
using System.Net;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using ZinklofDev.Console;
using ZinklofDev.ConsoleV2;

public class NetworkCommands : NetworkBehavior
{
    private static NetworkCommands netCmd;

    public int PingStartEpoch;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        netCmd = this;
    }

    private void Awake()
    {
        ZinklofDev.Console.Shell.RegisterCommand(HOST);
        ZinklofDev.Console.Shell.RegisterCommand(CONNECT);
        ZinklofDev.Console.Shell.RegisterCommand(ClientBackend.CHANGEUSERNAME);
    }

    public void HandlePingOperation()
    {
        SendPingRPC();
    }
    
    (Rpc(SendTo.Server)
    private void SendPingRPC()
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        int epochTime = DateTime.UtcNow - epoch;

        ReturnPingRPC(epochTime);
    }

    private void ReturnPingRPC(DateTime PingSent, DateTime PingRecieved)
    {
        int returnMS = new TimeSpan(PingRecieved - DateTime.UtcNow).totalMiliseconds;
        int sendMS = new TimeSpan(PingSent - PingRecieved).totalMiliseconds;

        ConsoleV2.Console.Log("Send: " + sendMS + "ms | Return: " + returnMS + "ms | Total: " + (sendMS + returnMS) + "ms", "Ping");
    }

    LegacyCommand HOST =  new LegacyCommand("0001x8800000000", "host", "starts server", false, ()=>
    {
        host();    
    });

    LegacyCommand<string> CONNECT = new LegacyCommand<string>("0001x8800000001", "connect", "connects to server", false, (t1) =>
    {
        Connect(t1);
    });

    [Command("Starts hosting", false, "Network.Host")]
    public static void host()
    {
        ConsoleV2.Console.Log("Hosting network...", "Host");
        NetworkManager.Singleton.StartHost();
        Debug.Log(IPV4toHex.IPV4ToHexadecimal(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString()));

        if (NetworkManager.IsNetworkActive)
        {
            ConsoleV2.Console.Log("Network is live!", "Host");
        }
        else
        {
            ConsoleV2.Console.Log("Something went wrong, Network is not alive :(", "Host");
        }
    }

    [Command("Connects to the specified join ID", false, "Network.Connect")]
    public static void Connect(string hostID)
    {
        try
        {
            ConsoleV2.Console.Log("Attempting to join network...", "Connect");
            string targetIP = IPV4toHex.HexadecimalToIPV4(hostID);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(targetIP, 7777);
            NetworkManager.Singleton.StartClient();

            if (NetworkManager.IsNetworkActive)
            {
                ConsoleV2.Console.Log("Connected!?", "Connect");
            }
            else
            {
                ConsoleV2.Console.Log("Not connected!?", "Connect");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public static void Disconnect()
    {
        try
        {
            ConsoleV2.Console.Log("Disconnecting...", "Disconnect");
            NetworkManager.Singleton.DisconnectClient();
            ConsoleV2.Console.Log("Disconnected!", "Disconnect");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    [Command("Sends a ping and spits the send + re-receive Time back into the console", "Network.Ping")
    public static void Ping()
    {
        if (!NetworkManager.isNetworkActive)
        {
            ConsoleV2.Console.Log("No network to ping!", "Ping");
        }
    
        netCmd.HandlePingOperation();
        ConsoleV2.Console.Log("Ping started!", "Ping");
    }
}
