using System;
using System.Net;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using ZinklofDev.Console;
using ZinklofDev.ConsoleV2;

public class NetworkCommands : NetworkBehaviour
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
        SendPingRPC(DateTime.UtcNow);
    }
    
    [Rpc(SendTo.Server)]
    private void SendPingRPC(DateTime pingSent)
    {
        DateTime pingRecieved = DateTime.UtcNow;

        ReturnPingRPC(pingSent, pingRecieved);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ReturnPingRPC(DateTime PingSent, DateTime PingRecieved, RpcParams rpcParams = default)
    {
        double returnMS = (PingSent - DateTime.Now).TotalMilliseconds;
        double sendMS = (PingSent - PingRecieved).TotalMilliseconds;

        ZinklofDev.ConsoleV2.Console.Log("Send: " + sendMS + "ms | Return: " + returnMS + "ms | Total: " + (sendMS + returnMS) + "ms", "Ping");
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
        ZinklofDev.ConsoleV2.Console.Log("Hosting network...", "Host");
        NetworkManager.Singleton.StartHost();
        Debug.Log(IPV4toHex.IPV4ToHexadecimal(Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString()));

        if (netCmd.NetworkManager.isActiveAndEnabled)
        {
            ZinklofDev.ConsoleV2.Console.Log("Network is live!", "Host");
        }
        else
        {
            ZinklofDev.ConsoleV2.Console.Log("Something went wrong, Network is not alive :(", "Host");
        }
    }

    [Command("Connects to the specified join ID", false, "Network.Connect")]
    public static void Connect(string hostID)
    {
        try
        {
            ZinklofDev.ConsoleV2.Console.Log("Attempting to join network...", "Connect");
            string targetIP = IPV4toHex.HexadecimalToIPV4(hostID);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(targetIP, 7777);
            NetworkManager.Singleton.StartClient();

            if (netCmd.NetworkManager.isActiveAndEnabled)
            {
                ZinklofDev.ConsoleV2.Console.Log("Connected!?", "Connect");
            }
            else
            {
                ZinklofDev.ConsoleV2.Console.Log("Not connected!?", "Connect");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    [Command("Kicks a Client", false, "Network.Kick")]
    public static void Kick(ulong id)
    {
        try
        {
            ZinklofDev.ConsoleV2.Console.Log("Kicking...", "Kick");
            NetworkManager.Singleton.DisconnectClient(id);
            ZinklofDev.ConsoleV2.Console.Log("Kicked!", "Kick");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    [Command("Sends a ping and spits the send + re-receive Time back into the console", false, "Network.Ping")]
    public static void Ping()
    {
        if (!netCmd.NetworkManager.isActiveAndEnabled)
        {
            ZinklofDev.ConsoleV2.Console.Log("No network to ping!", "Ping");
        }
    
        netCmd.HandlePingOperation();
        ZinklofDev.ConsoleV2.Console.Log("Ping started!", "Ping");
    }
}
