using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using ZinklofDev.Utils.Mapping;
using ZinklofDev.Utils.MathZ;

public enum TreePerlinDisplay {
    None,
    WhiteHot,
    BlackHot,
    PassFail,
    PassFailWhiteHot,
    PassFailBlackHot
}

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
    [Header("Perlin Noise Settings")]
    //[SerializeField] private bool useDefalutValuesPerlin;
    [SerializeField] private float perlinScale;
    [SerializeField] private int imgSize = 4096;
    [SerializeField] private int octaves = 3;
    [SerializeField] private float persistance = 1;
    [SerializeField] private float lacunarity = 5;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float perlinCuttoffPercent; // Perlin Cuttof sweet spot is just between 50 and 51 percent.
    
    // Allows us to view the perlin noise generated in order to dubug
    [Header("Perlin Noise Texture")]
    [SerializeField] private Texture2D perlinTexture;
    [SerializeField] private TreePerlinDisplay treePerlinDisplayOptions;
    [SerializeField] private bool debugColors;
    [SerializeField] private Material perlinMaterial;
    
    // These are all settings that remove trees from being placed independently from the perlin noise system that cuts out where the trees are that we get.
    [Header("Manual Exclusion Settings")]
    [SerializeField] private float campfireExclusionRaduis;
    [SerializeField] private float minimumTreeCount;
    [SerializeField] private float maximuimTreePlacementFails;
    [SerializeField] private Vector2 mapSize;
    
    // Where we input any refrences to prefabs or in scene Game Objects that we need refrences to, like the tree prefab.
    [Header("Game Object Refrences")]
    [SerializeField] private GameObject treePrefab;

    [Space(15)]
    // Allows interfacing with the custom editor for this class that then makes debugging easier
    [Header("Perlin Noise Editor Settings")]
    [SerializeField] bool overrideRandSeed;
    [SerializeField] int setSeed;
    [SerializeField] public bool autoComputeInEditor;
    [SerializeField] public bool autoDrawTextureInEditor;
    
    // Allows interfacing with the custom editor for this class, related to trees and memory.
    [Header("EDITOR ONLY trees (ENSURE CLEAR BEFORE BUILD FOR MEMORY REASONS)")]
    [SerializeField] public bool autoUpdateTreeVisibility;
    [SerializeField] List<GameObject> trees = new List<GameObject>();
    [SerializeField] float editorDist = 100;
    [SerializeField] Vector3 editorScale;
    [SerializeField] PerlinMap editorPerlinMap;
    [SerializeField] Material passMat;
    [SerializeField] Material failMat;

    private void OnValidate()
    {
        if (autoDrawTextureInEditor && !autoComputeInEditor)
        {
            Debug.LogWarning("AutoDrawTexture is on, this is gonna get laggy");
            PerlinToTexture(editorPerlinMap);
        }
        else if(autoComputeInEditor)
        {
            Debug.LogWarning("AutoCompute is on, this is gonna get extra laggy");
            DrawPerlinEditor();
        }

        if (autoUpdateTreeVisibility)
        {
            Debug.LogWarning("AutoUpdateVisibility is on this might get a little laggy");
            TreeHiderEditor();
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Probably going to move to OnNetworkSpawn function in the future
    private void Start()
    {
        autoDrawTextureInEditor = false;
        autoComputeInEditor = false;
        autoUpdateTreeVisibility = false;
        _IsServer = IsServer;
        //if (!_IsServer)
        //  return;
        SetVariableDefaultValues();

            List<Vector2> points = Noise.PoissonDiscSamplingVector2(5, mapSize, 30);
            int tempSeed = Random.Range(0, 99999);
            PerlinMap perlinMap = Noise.GenPerlinMap(imgSize, imgSize, tempSeed, perlinScale, octaves, persistance, lacunarity, offset);

            //maxPerlinValue = ((perlinMap.MaxMapHeight - perlinMap.MinMapHeight) * perlinCuttoffPercent) + perlinMap.MinMapHeight;
            //Debug.Log("Max: " + perlinMap.MaxMapHeight + "\nMin: " + perlinMap.MinMapHeight + "\nCutoff Value: " + maxPerlinValue);
            PerlinToTexture(perlinMap);

            float multiple = imgSize / mapSize.x;

            foreach (Vector2 point in points)
            {
                float x = point.x - (mapSize.x / 2);
                float y = point.y - (mapSize.y / 2);
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(x, 9000, y), Vector3.down, out hit, 9999))
                {
                    Vector2 pointToPerlinSpace = new Vector2(point.x * multiple, point.y * multiple);
                    //float value = perlinMap.Map[(int)point.x, (int)point.y];
                    if (perlinMap.Map[(int)pointToPerlinSpace.x, (int)pointToPerlinSpace.y] <= perlinCuttoffPercent  && Vectors.SqrDist3f(new Vector3(0, 0, 0), hit.point) > Numbers.Sqr(campfireExclusionRaduis))
                    {
                        Vector3 eulerRandomRotation = new Vector3(0, Random.Range(0, 360));
                        Quaternion quaternionRandomRotation = Quaternion.Euler(eulerRandomRotation);
                        GameObject temp = GameObject.Instantiate(treePrefab, hit.point, quaternionRandomRotation);
                        temp.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                    }
                }
            }
    }

    private void PlaceTrees(List<Vector2> points, PerlinMap perlinMap)
    {
        foreach (Vector2 point in points)
        {
            float x = point.x - 500;
            float y = point.y - 500;

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, 9000, y), Vector3.down, out hit, 9999))
            {
                if (CheckIfPlaceable(new Vector3(point.x, hit.point.y, point.y), perlinMap))
                {
                    GameObject temp = GameObject.Instantiate(treePrefab, hit.point, new Quaternion(0, 0, 0, 0));
                    temp.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                }
            }
        }
    }

    private bool CheckIfPlaceable(Vector3 treePos, PerlinMap perlinMap)
    {
        float multiple = imgSize / mapSize.x;
    
        Vector2 pointToPerlinSpace = new Vector2(treePos.x * multiple, treePos.y * multiple);

        Debug.Log((int)pointToPerlinSpace.y);
        Debug.Log((int)pointToPerlinSpace.x);

        float value = perlinMap.Map[(int)pointToPerlinSpace.x, (int)pointToPerlinSpace.y];

        if (value <= maxPerlinValue && Vectors.SqrDist3f(new Vector3(0, 0, 0), treePos) > Numbers.Sqr(campfireExclusionRaduis))
        {
            return true;
        }
        else
        return false;
    }

    public void DrawPerlinEditor()
    {
        PerlinMap perlinMap;
        if (!overrideRandSeed)
        {
            int tempSeed = Random.Range(0, 99999);
            perlinMap = Noise.GenPerlinMap(imgSize, imgSize, tempSeed, perlinScale, octaves, persistance, lacunarity, offset);
        }
        else
        {
            perlinMap = Noise.GenPerlinMap(imgSize, imgSize, setSeed, perlinScale, octaves, persistance, lacunarity, offset);
        }

        editorPerlinMap = perlinMap;
        PerlinToTexture(perlinMap);
    }

    public void GenTreesEditor()
    {
        if (editorPerlinMap == null)
        {
            Debug.LogError("Attempting to gen editor trees when there is no editorPerlinMap!");
            return;
        }

        trees = new List<GameObject>();
        List<Vector2> points = Noise.PoissonDiscSamplingVector2(editorDist, mapSize, 30);

        float multiple = imgSize / 1000;

        foreach (Vector2 point in points)
        {
            float x = point.x - 500;
            float y = point.y - 500;

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, 9000, y), Vector3.down, out hit, 9999))
            {
                GameObject temp = GameObject.Instantiate(treePrefab, hit.point, new Quaternion(0, 0, 0, 0));
                temp.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                temp.transform.localScale = editorScale;
                trees.Add(temp);
            }

            TreeHiderEditor();
        }
    }   

    public void TreeHiderEditor()
    {
        foreach (GameObject tree in trees)
        {
            Renderer renderer = tree.GetComponent<Renderer>();
            if (CheckIfPlaceable(tree.transform.position, editorPerlinMap))
            {
                renderer.material = passMat;
            }
            else
            {
                renderer.material = failMat;
            }
        }
    }

    public void ClearEditorOnlyVariables()
    {
        trees = null;
        editorPerlinMap = null;
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

    private void PerlinToTexture(PerlinMap perlinMap) // Authored: Cameron
    {
        Color[] colorMap = new Color[imgSize * imgSize]; // Creating a color array with the same number of pixels as a imgSize imgSize image, knowing our texture is a 4k texture

        if (treePerlinDisplayOptions == TreePerlinDisplay.WhiteHot) // if we have the white hot setting on, make the larger values aproach white, and the smaller ones aproach black
        for(int y = 0; y < imgSize; y++)
        {
            for(int x = 0; x < imgSize; x++)
            {
                colorMap[y * imgSize + x] = Color.Lerp(Color.black, Color.white, perlinMap.Map[x,y]);
            }
        }
        else if (treePerlinDisplayOptions == TreePerlinDisplay.BlackHot) // if instead we have black hot, make larger values aproach black, smaller values aproach white
        for(int y = 0; y < imgSize; y++)
        {
            for(int x = 0; x < imgSize; x++)
            {
                colorMap[y * imgSize + x] = Color.Lerp(Color.white, Color.black, perlinMap.Map[x,y]);
            }
        }
        else if (treePerlinDisplayOptions == TreePerlinDisplay.PassFail)
        for(int y = 0; y < imgSize; y++)
        {
            for(int x = 0; x < imgSize; x++)
            {
                if (perlinMap.Map[x,y] > perlinCuttoffPercent)
                colorMap[y * imgSize + x] = Color.red;
                else
                colorMap[y * imgSize + x] = Color.green;
            }
        }
        else if (treePerlinDisplayOptions == TreePerlinDisplay.PassFailWhiteHot) // if we have pass fail white hot, generate white hot, then apply a faint pass fail on top.
        {
            for(int y = 0; y < imgSize; y++)
            {
                for(int x = 0; x < imgSize; x++)
                {
                    colorMap[y * imgSize + x] = Color.Lerp(Color.black, Color.white, perlinMap.Map[x,y]);
                }
            }
            for(int y = 0; y < imgSize; y++)
            {
                for(int x = 0; x < imgSize; x++)
                {
                    if (perlinMap.Map[x,y] > perlinCuttoffPercent)
                    colorMap[y * imgSize + x] -= new Color(0, 0.25f, 0.25f, 0);
                    else
                    colorMap[y * imgSize + x] -= new Color(0.25f, 0, 0.25f, 0);
                }
            } 
        }
        else // the only other scenario would be pass fail black hot, so generate black hot then apply a faint pass fail.
        {
            for(int y = 0; y < imgSize; y++)
            {
                for(int x = 0; x < imgSize; x++)
                {
                    colorMap[y * imgSize + x] = Color.Lerp(Color.white, Color.black, perlinMap.Map[x,y]);
                }
            }
            for(int y = 0; y < imgSize; y++)
            {
                for(int x = 0; x < imgSize; x++)
                {
                    if (perlinMap.Map[x,y] > perlinCuttoffPercent)
                    colorMap[y * imgSize + x] -= new Color(0, 0.25f, 0.25f, 0);
                    else
                    colorMap[y * imgSize + x] -= new Color(0.25f, 0, 0.25f, 0);
                }
            } 
        }
        
        if (debugColors) // if we have debug colors on, log the first 200 colors so we can see them
        {
            for (int y = 0; y < 200; y++)
            {
                Debug.Log(colorMap[y]);
            }
        }

        perlinTexture = new Texture2D(imgSize, imgSize); // turn the null texture2d object into a new texture2d
        perlinTexture.filterMode = FilterMode.Point; // set the filter to point to see exact points
        perlinTexture.wrapMode = TextureWrapMode.Clamp; // clamp to avoid repeating
        perlinTexture.SetPixels(colorMap); // take the color array and set the pixels of our texture2d (for anyone unsure how this works as a 1d array for a 2d texture, check unity documentation :D)
        perlinTexture.Apply(); // apply all changes

        perlinMaterial.mainTexture = perlinTexture; // set the _MainTex value (desiganted in the HLSL on the materials shader) to the new texture2d.
        
        //byte[] bytes = perlinTexture.EncodeToPNG(); // this turns the texture 2d into bytes that work in the png format
        //File.WriteAllBytes(Application.dataPath + "perlinDebugView.png", bytes); // this saves the bytes, though i think its actually an entirely wrong way to do this... FileStream and StreamWriter would likley be the best method, no worries as we wont be using this again.
    }
}
