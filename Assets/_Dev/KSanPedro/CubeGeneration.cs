using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CubeGeneration : NetworkBehaviour
{
    static bool _IsServer;
    public GameObject Cube = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _IsServer = IsServer;
        List<Vector2> points = ZinklofDev.Utils.Mapping.Noise.PoissonDiscSamplingVector2(5, new Vector2(1000,1000), 30);

        foreach (Vector2 point in points)
        {
            float x = point.x - 500;
            float y = point.y - 500;

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, 9000, y), Vector3.down, out hit, 9999))
            {
                GameObject.Instantiate(Cube, hit.point, new Quaternion(0, 0, 0, 0));
            } 
        }
    }
}