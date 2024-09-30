using System.Collections.Generic;
using UnityEngine;
using ZinklofDev.Utils.Testing;

[AddComponentMenu("!ZinklofDev/" + "UnitTesting")]
public class TestManagerObject : MonoBehaviour
{
    [Header("Settings")]
    public bool VerboseLogs = false;
    public bool runUnitTests = false;

    public void Awake()
    {
        TestManager.verbose = VerboseLogs;

        TestManager.VerboseLog("AWAKE FUNCTION ON TESTMANAGEROBJECT HAS BEEN RUN");
        if (runUnitTests)
        {
            TestManager.VerboseLog("UNIT TESTS IS TRUE");
            if (UnityEngine.Debug.isDebugBuild)
            {
                TestManager.VerboseLog("IS IN EDITOR OR DEBUG BUILD");
                TestManager.OnFirstSceneLoaded();
            }
        }
    }
}
