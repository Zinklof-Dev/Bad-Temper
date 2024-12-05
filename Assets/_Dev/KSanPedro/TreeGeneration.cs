using System.Collections.Generic;
// using System.Collections;
// using System.IO;
using UnityEngine;
using Unity.Netcode;
using ZinklofDev.Utils.Mapping;
using ZinklofDev.Utils.MathZ;

public class TreeGeneration : NetworkBehaviour
{
    // Invisible variables
    // Provides a static bool to determine if the player is the server, allowing us to do commands later down the line that involve networking
    private static bool _IsServer;
    // Value that is calculated and stored as the max perlin value that trees can be placed.
    private float maxPerlinValue;

    // Settings for the Gizmos
    [Header("Debug Gizmos Settings")]
    [SerializeField] private bool drawGizmos;

    // Settings for the Poisson Disc Sampling that generates the points where trees will be placed. 
    [Header("Poisson Disc Sampling Settings")]
    [SerializeField] private bool useDefalutValuesPoisson;
    
    // Settings for the perlin noise that cuts out tress that have been placed from the Poisson Disc Sampling function
    [Header("Perlin Noise Cutout Settings")]
    [SerializeField] private bool useDefalutValuesPerlin;
    [SerializeField] private float perlinScale;
    [SerializeField] private float perlinCuttoffPercent; // Perlin Cuttof sweet spot is just between 50 and 51 percent.
    
    // Allows us to view the perlin noise generated in order to dubug
    [Header("Perlin Noise Texture")]
    [SerializeField] private Texture2D perlinTexture;
    
    // These are all settings that remove trees from being placed independently from the perlin noise system that cuts out where the trees are that we get.
    [Header("Manual Exclusion Settings")]
    [SerializeField] private float campfireExclusionRaduis;
    [SerializeField] private float minimumTreeCount;
    [SerializeField] private float maximuimTreePlacementFails;
    [SerializeField] private Vector2 mapSize;
    
    // Where we input any refrences to prefabs or in scene Game Objects that we need refrences to, like the tree prefab.
    [Header("Game Object Refrences")]
    [SerializeField] private GameObject treePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Probably going to move to OnNetworkSpawn function in the future
    private void Start()
    {
        _IsServer = IsServer;
        //if (!_IsServer)
        //  return;
        SetVariableDefaultValues();
        float fails = 0;
        bool success = false;

        while(!success)
        {
            List<Vector2> points = Noise.PoissonDiscSamplingVector2(5, mapSize, 30);
            int tempSeed = Random.Range(0, 99999);
            PerlinMap perlinMap = Noise.GenPerlinMap(4096, 4096, tempSeed, perlinScale, 3, 3, 3, new Vector2(0,0));
    
            maxPerlinValue = ((perlinMap.MaxMapHeight - perlinMap.MinMapHeight) * perlinCuttoffPercent) + perlinMap.MinMapHeight;
            // Debug.Log("Max: " + perlinMap.MaxMapHeight + "\nMin: " + perlinMap.MinMapHeight + "\nCutoff Value: " + maxPerlinValue);
            PerlinToTexture(perlinMap);
    
            PlaceTrees(points, perlinMap);

            GameObject[] treeCounter = GameObject.FindGameObjectsWithTag("Tree"); // Have to add a "Tree" tag to the tree prefab in Flower's

            if(treeCounter.Length >= minimumTreeCount)
            {
                success = true;
                break;
            }

            fails++;

            if(fails >= maximuimTreePlacementFails)
                break;
        }
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
                    GameObject temp = GameObject.Instantiate(treePrefab, hit.point, new Quaternion(0, 0, 0, 0));
                    temp.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                }
            }
        }
    }

    private void SetVariableDefaultValues()
    {
        // For each variable group, this checks if we have the use default values bool checked, an it sets the correspnding variables to the hard coded default amounts
        
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
        /*
        byte[] bytes = perlinTexture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/../", bytes); WRITE PATH LATER
        */
    }
}
