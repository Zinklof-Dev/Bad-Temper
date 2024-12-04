using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using ZinklofDev.Utils.Mapping;
using ZinklofDev.Utils.MathZ;

public class TreeGeneration : NetworkBehaviour
{
    static bool _IsServer;
    [SerializeField] bool drawGizmos;
    private float maxPerlinValue;
    [SerializeField] private float perlinScale;
    [SerializeField] private float perlinCuttoffPercent; // Perlin Cuttof sweet spot is just between 50 and 51 percent.
    [SerializeField] private float campfireExclusionRaduis =50;
    [SerializeField] private Vector2 mapSize;
    [SerializeField] private GameObject Cube;
    [SerializeField] private Texture2D perlinTexture;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _IsServer = IsServer;

        //if (!_IsServer)
        //  return;

        List<Vector2> points = Noise.PoissonDiscSamplingVector2(5, mapSize, 30);
        int tempSeed = Random.Range(0, 99999);
        PerlinMap perlinMap = Noise.GenPerlinMap(4096, 4096, tempSeed, perlinScale, 3, 3, 3, new Vector2(0,0));

        maxPerlinValue = ((perlinMap.MaxMapHeight - perlinMap.MinMapHeight) * perlinCuttoffPercent) + perlinMap.MinMapHeight;
        Debug.Log("Max: " + perlinMap.MaxMapHeight + "\nMin: " + perlinMap.MinMapHeight + "\nCutoff Value: " + maxPerlinValue);

        PlaceTrees(points, perlinMap);

        PerlinToTexture(perlinMap);
    }

    private void PlaceTrees(List<Vector2> points, PerlinMap perlinMap)
    {
        float multiple = 4096 / 1000;

        foreach (Vector2 point in points)
        {
            float x = point.x - 500;
            float y = point.y - 500;

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, 9000, y), Vector3.down, out hit, 9999))
            {
                Vector2 pointToPerlinSpace = new Vector2(point.x * multiple, point.y * multiple);

                float value = perlinMap.Map[(int)pointToPerlinSpace.x, (int)pointToPerlinSpace.y];

                if (value <= maxPerlinValue && Vectors.SqrDist3f(new Vector3(0, 0, 0), hit.point) > Numbers.Sqr(campfireExclusionRaduis))
                {
                    GameObject temp = GameObject.Instantiate(Cube, hit.point, new Quaternion(0, 0, 0, 0));
                    temp.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(!drawGizmos) return;

        Gizmos.color = new Color(255, 0, 0, 0.5f);
        Gizmos.DrawWireSphere(new Vector3(0, 0, 0), campfireExclusionRaduis);
        Gizmos.color = new Color(0, 255, 0, 0.5f);
        Gizmos.DrawWireCube(new Vector3(0, 0, 0), new Vector3(mapSize.x, 100, mapSize.y));
    }

    private void PerlinToTexture(PerlinMap perlinMap)
    {
        Color[] colorMap = new Color[4096 * 4096];
        for(int y = 0; y < 4096; y++)
        {
            for(int x = 0; x < 4096; x++)
            {
                colorMap[y * 4096 + x] = Color.Lerp(Color.black, Color.white, (perlinMap.Map[x, y] - perlinMap.MinMapHeight) / (perlinMap.MaxMapHeight - perlinMap.MinMapHeight));
            }
        }

        perlinTexture = new Texture2D(4096, 4096);
        perlinTexture.filterMode = FilterMode.Point;
        perlinTexture.wrapMode = TextureWrapMode.Clamp;
        perlinTexture.SetPixels(colorMap);
        perlinTexture.Apply();
    }
}