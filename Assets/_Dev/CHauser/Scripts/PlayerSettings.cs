using Unity.Collections;
using UnityEngine;
using ZinklofDev.Utils.Testing;

public static class PlayerSettings
{
    public static FixedString32Bytes userName;
    public enum ScreenMode { fullScreen, fullScreenBorderless, windowed }
    public static bool volumeMuted;
    public static float sensitivity;
    public static float volume;

    // Ignore

   /* public static Test TestTestHelpOhGod = new Test("PlayerSettings.cs", () =>
    {
        string x = "pp";

        TestTestHelpOhGod.Expect(x, "notpp");
    }); */
}
