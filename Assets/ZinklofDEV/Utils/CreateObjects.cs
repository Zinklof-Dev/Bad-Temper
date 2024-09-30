using UnityEditor;
using UnityEngine;
using ZinklofDev.Utils.Testing;

public class CreateObjects : MonoBehaviour
{
    private static Texture2D _icon = Resources.Load<Texture2D>("Assets/ZinklofDEV/Textures/TestManager.tiff");

    [MenuItem("ZinklofDev/UnitTesting/Create TestManager")]
    public static void CreateTestManager()
    {
        GameObject manager = new GameObject("TestManagerObject");
        manager.AddComponent<TestManagerObject>();
        manager.SetActive(true);
    }
}
