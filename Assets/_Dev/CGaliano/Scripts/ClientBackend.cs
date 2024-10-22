using UnityEngine;
using ZinklofDev.Console;

public static class ClientBackend
{
    public delegate void ClientBackendEventHandler();
    public static event ClientBackendEventHandler OnClientEndUsernameChanged;

    public static string playerUsername = "New Player";

    public static void ChangeUsername(string username)
    {
        playerUsername = username;
        OnClientEndUsernameChanged?.Invoke();
    }

    public static LegacyCommand<string> CHANGEUSERNAME = new LegacyCommand<string>("0001x8800000002", "change_username", "changes the static username that exists clientside only", false, (t1) =>
    {
        ChangeUsername(t1);
    });
}
