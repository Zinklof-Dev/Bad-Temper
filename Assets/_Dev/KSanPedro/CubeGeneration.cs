using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Utilities;

public class CubeGeneration : NetworkBehaviour
{
    static bool _IsServer;
    public GameObject Cube = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _IsServer = IsServer;
        List<Vector2> points = ZinklofDev.Utils.Mapping.Noise.PoissonDiscSamplingVector2(1, new Vector2(50,50), 30);
        foreach (Vector2 point in points)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(point.x, 9000, point.y), Vector3.down, out hit, 9999))
            {
                //Ins.transform.position(Object.Instantiate(Cube));
            }
        }
    }

    void SpawnCube()
    {
        GameObject.Instantiate(Cube);
    }
}