using Unity.Collections;
using UnityEngine;

public static class PlayerSettings
{
    public static FixedString32Bytes userName;
    public enum ScreenMode { fullScreen, fullScreenBorderless, windowed }
    public static bool volumeMuted;
    public static float sensitivity;
    public static float volume;
}
