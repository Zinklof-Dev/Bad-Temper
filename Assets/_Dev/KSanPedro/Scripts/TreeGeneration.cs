using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;
using ZinklofDev.Utils.Mapping;
using ZinklofDev.Utils.MathZ;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.Rendering;

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
    #region //////////////////////////////////////////////// VARIABLES ////////////////////////////////////////////////

    #region Non-Serialized vars
    private static bool _IsServer;
    private PerlinMap _EditorPerlinMap;
    private bool _ServerHasSeed = false;
    private int _Seed;
    #endregion

    #region Global Vars
    [SerializeField] private Vector2 _MapSize = new Vector2(1000, 1000);
    #endregion

    #region Gizmos Vars
    [SerializeField] private bool _DrawGizmos;
    #endregion

    #region Tree Perlin Noise Vars
    [SerializeField] private float _TreePerlinScale = 3000;
    [SerializeField] private int _TreeImageSize = 500;
    [SerializeField] private int _TreeOctaves = 4;
    [SerializeField] private float _TreePersistance = 1;
    [SerializeField] private float _TreeLacunarity = 4;
    [SerializeField] private Vector2 _TreeOffset;
    #endregion

    #region Tree Poisson Vars
    [SerializeField] float _TreeRadius = 5;
    [SerializeField] int _TreeAccuracy = 30;
    #endregion

    #region Rocks Perlin Noise Vars
    [SerializeField] private float _RockPerlinScale = 3000;
    [SerializeField] private int _RockImageSize = 500;
    [SerializeField] private int _RockOctaves = 4;
    [SerializeField] private float _RockPersistance = 1;
    [SerializeField] private float _RockLacunarity = 4;
    [SerializeField] private Vector2 _RockOffset;
    #endregion

    #region Rocks Poisson Vars
    [SerializeField] float _RockRadius = 5;
    [SerializeField] int _RockAccuracy = 30;
    #endregion

    #region Perlin Debug Display Vars
    [SerializeField] private Texture2D _PerlinTexture;
    [SerializeField] private TreePerlinDisplay _TreePerlinDisplay;
    [SerializeField] private Material _PerlinMaterial;
    #endregion

    #region Exclusion Vars
    [SerializeField] private float _CampfireExclusionRadius = 20;
    [SerializeField] private float _TreeCutoffPercent = 0.1f;
    [SerializeField] private float _RockCutoffPercent = 0.1f;
    #endregion

    #region Reference Vars
    [SerializeField] private List<Mesh> _TreeMeshList;
    [SerializeField] private List<Mesh> _RockMeshList;
    [SerializeField] private GameObject _TreePrefab;
    [SerializeField] private GameObject _RockPrefab;
    #endregion

    #region Editor Vars
    [SerializeField] public bool _OverrideRandomSeed;
    [SerializeField] int _SetSeed;
    [SerializeField] public bool _AutoComputeEditor;
    [SerializeField] public bool _AutoDrawTexEditor;
    #endregion

    #endregion
    #region //////////////////////////////////////////////// UNITY FUNCTIONS ////////////////////////////////////////////////
    private void OnValidate()
    {
        // Cameron | never use onValidate like i did here, its really bad practice but eh..

        if (_AutoDrawTexEditor && !_AutoComputeEditor)
        {
            Debug.LogWarning("AutoDrawTexture is on, this is gonna get laggy");
            PerlinToTexture(_EditorPerlinMap, _TreeImageSize, _TreeCutoffPercent);
        }
        else if (_AutoComputeEditor)
        {
            Debug.LogWarning("AutoCompute is on, this is gonna get extra laggy");
            DrawPerlinEditor(0);
        }
    }

    private void OnDrawGizmos() // Cameron | moved this to the top, keeping unity auto called functions at the top and your own functions below those helps orginization
    {
        if (!_DrawGizmos) return;

        Gizmos.color = new Color(255, 0, 0, 0.5f);
        Gizmos.DrawWireSphere(new Vector3(0, 0, 0), _CampfireExclusionRadius);
        Gizmos.color = new Color(0, 255, 0, 0.5f);
        Gizmos.DrawWireCube(new Vector3(0, 0, 0), new Vector3(_MapSize.x, 100, _MapSize.y));
    }

    public override void OnNetworkSpawn()
    {
        _AutoDrawTexEditor = false;
        _AutoComputeEditor = false;
        _IsServer = IsServer;

        if (IsServer)
        {
            // Temp random seed for now, will maybe replace with System.Random and maybe have a player setting to set the seed
            if (!_OverrideRandomSeed)
                _Seed = UnityEngine.Random.Range(0, 9999);
            else
                _Seed = _SetSeed;

            _ServerHasSeed = true;

            ThreadManager();
        }
        else
        {
            AskForSeedRpc();
        }

        base.OnNetworkSpawn();
    }
    #endregion
    #region //////////////////////////////////////////////// OUR FUNCTIONS ////////////////////////////////////////////////
    private async void ThreadManager()
    {
        await GenTreesRuntime();
        Debug.Log("Trees Fin");
        await GenRocksRuntime();
        Debug.Log("Rocks Fin");
    }

    private async Task GenTreesRuntime()
    {
        //Debug.Log("Entered the tree func");

        List<Vector2> points = await Noise.PoissonSamplingAsync(_TreeRadius, _MapSize, _Seed, _TreeAccuracy);
        PerlinMap perlinMap = await Noise.GenPerlinMapAsnyc(_TreeImageSize, _TreeImageSize, _Seed, _TreePerlinScale, _TreeOctaves, _TreePersistance, _TreeLacunarity, _TreeOffset);
        System.Random randomRotationValue = new System.Random(_Seed);
        System.Random randomModel = new System.Random(_Seed + 2);

        //Debug.Log("Complex computations complete");

        //PerlinToTexture(perlinMap);

        float multiple = _TreeImageSize / _MapSize.x;
        foreach (Vector2 point in points)
        {
            float x = point.x - (_MapSize.x / 2);
            float y = point.y - (_MapSize.y / 2);
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, 9000, y), Vector3.down, out hit, 9999))
            {
                Vector2 pointToPerlinSpace = new Vector2(point.x * multiple, point.y * multiple);

                if (perlinMap.Map[(int)pointToPerlinSpace.x, (int)pointToPerlinSpace.y] <= _TreeCutoffPercent && Vectors.SqrDist3f(new Vector3(0, 0, 0), hit.point) > Numbers.Sqr(_CampfireExclusionRadius))
                {
                    //Debug.Log("Tree Placed " + hit.point);
                    int randomRotation = randomRotationValue.Next(0, 360);
                    Vector3 eulerRandomRotation = new Vector3(0, randomRotation, 0);
                    Quaternion quaternionRandomRotation = Quaternion.Euler(eulerRandomRotation);
                    Instantiate(_TreePrefab, new Vector3(hit.point.x, hit.point.y, hit.point.z), quaternionRandomRotation);
                }
            }
        }
    }

    private async Task GenRocksRuntime()
    {
        List<Vector2> points = await Noise.PoissonSamplingAsync(_RockRadius, _MapSize, _Seed + 1, _RockAccuracy);
        PerlinMap perlinMap = await Noise.GenPerlinMapAsnyc(_RockImageSize, _RockImageSize, _Seed, _RockPerlinScale, _RockOctaves, _RockPersistance, _RockLacunarity, _RockOffset);

        // PerlinToTexture(perlinMap);

        float multiple = _RockImageSize / _MapSize.x;

        foreach (Vector2 point in points)
        {
            float worldX = point.x - (_MapSize.x / 2);
            float worldY = point.y - (_MapSize.y / 2);

            Vector2 pointToPerlinSpace = new Vector2(point.x * multiple, point.y * multiple);

            if (perlinMap.Map[(int)pointToPerlinSpace.x, (int)pointToPerlinSpace.y] <= _RockCutoffPercent)
            {
                List<Vector2> clusterPoints = await Noise.PoissonSamplingAsync(1.5f, new Vector2(10, 10), _Seed + (int)point.x + (int)point.y);
                foreach(Vector2 clusterPoint in clusterPoints) 
                {
                    RaycastHit hit;
                    float x = clusterPoint.x - (10 / 2);
                    float y = clusterPoint.y - (10 / 2);

                    if (Physics.Raycast(new Vector3(x, 9000, y), Vector3.down, out hit, 9999) && Vectors.SqrDist3f(new Vector3(0,0,0), new Vector3(hit.point.x + worldX, hit.point.y, hit.point.z + worldY)) > Numbers.Sqr(_CampfireExclusionRadius))
                    {
                        Vector3 eulerRandomRotation = new Vector3(0, 0, 0);
                        Quaternion quaternionRandomRotation = Quaternion.Euler(eulerRandomRotation);
                        Instantiate(_RockPrefab, new Vector3(hit.point.x + worldX, hit.point.y, hit.point.z + worldY), quaternionRandomRotation);
                    }
                }
            }
        }
    }

    private async void AskAgain()
    {
        await Task.Delay(1000); // wait 500 ms, aka 0.5 secconds
        AskForSeedRpc(); // ask again
    }
    #endregion
    #region //////////////////////////////////////////////// RPC FUNCTIONS ////////////////////////////////////////////////
    [Rpc(SendTo.Server)]
    private void AskForSeedRpc(RpcParams rpcParams = default)
    {
        ulong clientID = rpcParams.Receive.SenderClientId; // get client ID
        if (!_ServerHasSeed) // if the server doesn't yet have the seed then deny the clients request
        {
            DenySeedRequestRpc(RpcTarget.Single(clientID, RpcTargetUse.Temp)); 
            return;
        }
        else // otherwise provide the seed
            SendSeedRpc(_Seed, RpcTarget.Single(clientID, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SendSeedRpc(int seed, RpcParams rpcParams = default)
    {
        this._Seed = seed;
        ThreadManager();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void DenySeedRequestRpc(RpcParams rpcParams = default) // the server has denied our request. so lets wait and ask again
    {
        AskAgain();
    }
    #endregion
    #region //////////////////////////////////////////////// EDITOR FUNCTIONS ////////////////////////////////////////////////
    public async void DrawPerlinEditor(int i)
    {
        PerlinMap perlinMap;
        int tempSeed = 0;

        if (!_OverrideRandomSeed)
        {
            tempSeed = UnityEngine.Random.Range(0, 99999);
        }
        else
        {
            tempSeed = _SetSeed;
        }

        if (i == 0)
        {
            perlinMap = await Noise.GenPerlinMapAsnyc(_TreeImageSize, _TreeImageSize, tempSeed, _TreePerlinScale, _TreeOctaves, _TreePersistance, _TreeLacunarity, _TreeOffset);
            PerlinToTexture(perlinMap, _TreeImageSize, _TreeCutoffPercent);
        }
        else
        {
            perlinMap = await Noise.GenPerlinMapAsnyc(_RockImageSize, _RockImageSize, tempSeed, _RockPerlinScale, _RockOctaves, _RockPersistance, _RockLacunarity, _RockOffset);
            PerlinToTexture(perlinMap, _RockImageSize, _RockCutoffPercent);
        }

        _EditorPerlinMap = perlinMap;
    }

    public void ClearEditorOnlyVariables()
    { 
        _EditorPerlinMap = null;
    }

    private void PerlinToTexture(PerlinMap perlinMap, int imageSize, float cutoffPercent) // Authored: Cameron
    {
        float min = 0;
        float max = 0;

        for (int y = 0; y < imageSize; y++)
        {
            for (int x = 0; x < imageSize; x++)
            {
                float temp = perlinMap.Map[x,y];

                if (temp < min) min = temp;
                if (temp > max) max = temp;
            }
        }

        //Debug.Log(min);
        //Debug.Log(max);

        Color[] colorMap = new Color[imageSize * imageSize]; // Creating a color array with the same number of pixels as a _TreeImageSize _TreeImageSize image, knowing our texture is a 4k texture

        if (_TreePerlinDisplay == TreePerlinDisplay.WhiteHot) // if we have the white hot setting on, make the larger values aproach white, and the smaller ones aproach black
        for(int y = 0; y < imageSize; y++)
        {
            for(int x = 0; x < imageSize; x++)
            {
                colorMap[y * imageSize + x] = Color.Lerp(Color.black, Color.white, perlinMap.Map[x,y]);
            }
        }
        else if (_TreePerlinDisplay == TreePerlinDisplay.BlackHot) // if instead we have black hot, make larger values aproach black, smaller values aproach white
        for(int y = 0; y < imageSize; y++)
        {
            for(int x = 0; x < imageSize; x++)
            {
                colorMap[y * imageSize + x] = Color.Lerp(Color.white, Color.black, perlinMap.Map[x,y]);
            }
        }
        else if (_TreePerlinDisplay == TreePerlinDisplay.PassFail)
        for(int y = 0; y < imageSize; y++)
        {
            for(int x = 0; x < imageSize; x++)
            {
                if (perlinMap.Map[x,y] > cutoffPercent)
                colorMap[y * imageSize + x] = Color.red;
                else
                colorMap[y * imageSize + x] = Color.green;
            }
        }
        else if (_TreePerlinDisplay == TreePerlinDisplay.PassFailWhiteHot) // if we have pass fail white hot, generate white hot, then apply a faint pass fail on top.
        {
            for(int y = 0; y < imageSize; y++)
            {
                for(int x = 0; x < imageSize; x++)
                {
                    colorMap[y * imageSize + x] = Color.Lerp(Color.black, Color.white, perlinMap.Map[x,y]);

                    if (perlinMap.Map[x, y] > cutoffPercent)
                        colorMap[y * imageSize + x] -= new Color(0, 0.1f, 0.25f, 0);
                    else
                        colorMap[y * imageSize + x] -= new Color(0.1f, 0, 0.25f, 0);
                }
            }
        }
        else // the only other scenario would be pass fail black hot, so generate black hot then apply a faint pass fail.
        {
            for(int y = 0; y < imageSize; y++)
            {
                for(int x = 0; x < imageSize; x++)
                {
                    colorMap[y * imageSize + x] = Color.Lerp(Color.white, Color.black, perlinMap.Map[x,y]);

                    if (perlinMap.Map[x, y] > cutoffPercent)
                        colorMap[y * imageSize + x] -= new Color(0, 0.1f, 0.25f, 0);
                    else
                        colorMap[y * imageSize + x] -= new Color(0.1f, 0, 0.25f, 0);
                }
            }
        }

        // Cameron | debug colors step removed, the problem we used it for was solved long ago

        _PerlinTexture = new Texture2D(imageSize, imageSize); // turn the null texture2d object into a new texture2d
        _PerlinTexture.filterMode = FilterMode.Point; // set the filter to point to see exact points
        _PerlinTexture.wrapMode = TextureWrapMode.Clamp; // clamp to avoid repeating
        _PerlinTexture.SetPixels(colorMap); // take the color array and set the pixels of our texture2d (for anyone unsure how this works as a 1d array for a 2d texture, check unity documentation :D)
        _PerlinTexture.Apply(); // apply all changes

        _PerlinMaterial.mainTexture = _PerlinTexture; // set the _MainTex value (desiganted in the HLSL on the materials shader) to the new texture2d.
    }
    #endregion
}
