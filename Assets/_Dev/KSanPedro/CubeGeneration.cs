using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CubeGeneration : NetworkBehaviour
{
    static bool _IsServer;
    [SerializeField] float maxPerlinValue;
    [SerializeField] float perlinScale;
    public GameObject Cube = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _IsServer = IsServer;
        List<Vector2> points = ZinklofDev.Utils.Mapping.Noise.PoissonDiscSamplingVector2(5, new Vector2(1000,1000), 30);
        int tempRand = Random.Range(0, 99999);
        float[,] perlinMap = ZinklofDev.Utils.Mapping.Noise.GenPerlinNoiseMap(4096, 4096, tempRand, perlinScale, 3, 3, 3, new Vector2(0, 0));

        float multiple = 4096 / 1000;

        foreach (Vector2 point in points)
        {
            float x = point.x - 500;
            float y = point.y - 500;

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, 9000, y), Vector3.down, out hit, 9999))
            {
                Vector2 pointToPerlinSpace = new Vector2(point.x * multiple, point.y * multiple);

                float value = perlinMap[(int)pointToPerlinSpace.x, (int)pointToPerlinSpace.y];
                if (value <= maxPerlinValue)
                {
                    GameObject temp = GameObject.Instantiate(Cube, hit.point, new Quaternion(0, 0, 0, 0));
                    temp.transform.position = new Vector3(hit.point.x,value,hit.point.z);
                }
            } 
        }
    }
}