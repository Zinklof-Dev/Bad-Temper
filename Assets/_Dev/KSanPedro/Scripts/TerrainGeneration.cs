using System;
using System.Collections.Generic;
using UnityEngine;
using ZinklofDev.Utils.Mapping;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using System.Threading.Tasks;
using System.Linq;

public class TerrainGeneration : MonoBehaviour
{
    [Header("References")]
    [SerializeField] MeshCollider _MeshCollider;
    [SerializeField] MeshFilter _MeshFilter;
    [Header("Perlin Noise Settings")]
    [SerializeField] int _imageSize;
    [SerializeField] float _Scale;
    [SerializeField] int _Octaves;
    [SerializeField] float _Persistance;
    [SerializeField] float _Lacunarity;
    [SerializeField] Vector2 _Offset;
    [SerializeField] AnimationCurve heightCurve;
    [Header("Poisson Sampling Settings")]
    [SerializeField] float _Radius;
    [SerializeField] Vector2 _RegionSize;
    [Header("Misc")]
    [SerializeField] float _HeightScale;
    [SerializeField] int meshCount = 4;
    [SerializeField] bool divideMesh;

    private TriangleNet.Mesh _Mesh;
    private UnityEngine.Mesh _UnityMesh;

    private PerlinMap _PerlinMap;
    private float[,] _FalloffMap;

    public async Task Initialize(int seed)
    {
        // !!!!!! Troll is here :D
        //_HeightScale = 45;

        if (meshCount % 4 != 0)
        {
            Debug.LogError("MeshCount must be divisible by four");
            return;
        }
        
        Polygon polygon = new Polygon();

        Debug.Log("Perlin, poisson, and falloff");
        _PerlinMap = await Noise.GenPerlinMapAsnyc(_imageSize, _imageSize, seed + 69, _Scale, _Octaves, _Persistance, _Lacunarity, _Offset);
        List<Vector2> points = await Noise.PoissonSamplingAsync(_Radius, _RegionSize, seed + 69);
        _FalloffMap = Noise.GenerateFalloffMap(_imageSize, 3, 25);

        if (!divideMesh)
        {
            Debug.Log("thread to triangulate");
            await Task.Run(() =>
            {
                for (int i = 0; i < points.Count; i++)
                {
                    polygon.Add(new Vertex(points[i].x, points[i].y));
                }
    
                ConstraintOptions options = new ConstraintOptions();
                options.ConformingDelaunay = true;
    
                _Mesh = polygon.Triangulate(options) as TriangleNet.Mesh;
            });

            //Debug.Log("genMesh");
            await GenerateMesh();
        }
        else
        {
            
        }

        //clearing memory
        _PerlinMap = null;
        _FalloffMap = null;
        _Mesh = null;
    }

    private float GetVertexHeight(float x, float y) 
    {
        AnimationCurve tempCurve = heightCurve;

        float percentageX = x / _RegionSize.x;
        float percentageY = y / _RegionSize.y;

        int yValue = (int)MathF.Floor(percentageY * _imageSize);
        int xValue = (int)MathF.Floor(percentageX * _imageSize);

        float vertexHeight = tempCurve.Evaluate(Mathf.Clamp(_PerlinMap.Map[xValue, yValue] - _FalloffMap[xValue, yValue], 0, 1));
        return vertexHeight * _HeightScale;
    }

    private Color EvaluateTriangleColor(Triangle triangle, Vector3 normal, float averageY)
    {
        float angle = Vector3.Angle(Vector3.up, normal.normalized);
        
        if (angle > 10 && averageY > -0.5f && averageY > 25)
        {
            return new Color(0.2f, 0.2f, 0.2f);
        }
        else if (averageY > 31)
        {
            return new Color(0.2f, 0.2f, 0.2f);
        }
        else if (averageY > 20)
        {
            return new Color(0, 0.35f, 0);
        }
        else if (averageY > 2.5f)
        {
            return new Color(0, 0.65f, 0);
        }
        else if (averageY > -10)
        {
            return new Color(0.660f, 0.5980f, 0.4019f);
        }
        else
        {
            return new Color(0.560f, 0.4980f, 0.3019f);
        }
    }

    
    async Task SplitVerts(List<Vector2> verticies)
    {
        // Idea: find a point where the center of each mesh would be, then assign each vert to its closest mesh (in the xy plane ignoring Y height)
        Vector2 halfRegionSize = _RegionSize / 2;
        List<Vector2>[] meshes = null;

        // Gonna hard code the 4 mesh divide first

        await Task.Run(() =>
        {
            if (meshCount == 4)
            {
                Vector2[] meshOrigins = new Vector2[4];

                meshOrigins[0] = new Vector2(0 + halfRegionSize.x, 0 + halfRegionSize.y);
                meshOrigins[1] = new Vector2(0 + halfRegionSize.x, 0 - halfRegionSize.y);
                meshOrigins[2] = new Vector2(0 - halfRegionSize.x, 0 + halfRegionSize.y);
                meshOrigins[3] = new Vector2(0 - halfRegionSize.x, 0 - halfRegionSize.y);

                meshes = new List<Vector2>[4];

                foreach (Vector2 vertex in verticies)
                {
                    int closestMesh = -1;
                    float closestMeshDistance = 999999999f;

                    for (int i = 0; i < meshOrigins.Length; i++)
                    {
                        if (Vector2.Distance(vertex, meshOrigins[i]) < closestMeshDistance)
                        {
                            closestMesh = i;
                        }
                    }

                    meshes[closestMesh].Add(vertex);
                }
            }
        });
        
        if (meshes != null)
            await TriangulateMeshes(meshes);
        else
            Debug.LogError("Null meshes list");
    }

    async Task ReTriangulateEdges(List<TriangleNet.Mesh> meshes) // gets rid of gap between meshes by gathering edges and connecting them
    { // the ammount of if statements inside other if/for statements here hurts me... | Cameron
      /*
        list<Vector2> potentialEdgeVectors = new List<Vector2>;
        list<Vector2> discreditedVectors = new List<Vector2>;

        IEnumerator<Triangle> triangleEnum = meshes[0].triangles.GetEnumerator();
        
        for (int i = 0; i < meshes[0].triangles.Count; i++)
        {
            if (!triangleEnum.MoveNext())
            {
                break;
            }

            Triangle currentTriangle = triangleEnum.Current;
            
            for (int i = 0; i < currentTriangle.Verticies.Length; i++)
            {
                byte result = await PointIsValid(New vector2(currentTriangle.Verticies[i].x, currentTriangle.Verticies[i].z), potentialEdgeVectors, discreditedVectors);
            
                if (result == 1)
                {
                    potentialEdgeVectors.Add(New vector2(currentTriangle.Verticies[i].x, currentTriangle.Verticies[i].z));
                }
                else if (result == 0)
                {
                    potentialEdgeVectors.Remove(currentTriangle.Verticies[i].x, currentTriangle.Verticies[i].z));
                    discreditedVectors.Add(currentTriangle.Verticies[i].x, currentTriangle.Verticies[i].z));
                }
                else // aka result = 2
                {
                }
            }
        }

        */
    }

    async Task<byte> PointIsValid(Vector2 p, list<Vector2> potentialEdgeVectors, List<Vector2> discreditedVectors) // 1 = point is valid | 0 = point must be discredited | 2 = point was already discredited
    {
        if (await !PointIsNotDiscredited(v, discreditedVectors)) // check if discredited
            return 2; // this only runs if the point was already discredited, so we inform our calling function it was;
    
        foreach (Vector2 v in potentailEdgeVectors)
        {
            if (p == v) // check if our point already has a triangle
                return 0; // in the case it does, inform the calling function it needs to be discredited
        }
        return 1; // no issues found, tell calling function we are good to add it to potential points.
    }

    async Task<bool> PointIsNotDiscredited(Vector2 p, List<Vector2> discreditedVectors)
    {
        foreach (Vector2 v in discreditedVectors)
        {
            if (p == v)
                return false;
            else
                continue;
        }
        return true;
    }

    async Task TriangulateMeshes(List<Vector2>[] meshes)
    {
        List<TriangleNet.Mesh> triNetMeshes = new List<TriangleNet.Mesh>();

        await Task.Run(() =>
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                Polygon polygon = new Polygon();

                for (int j = 0; j < meshes[i].Count; j++)
                {
                    polygon.Add(new Vertex(meshes[i][j].x, meshes[i][j].y));

                    ConstraintOptions options = new ConstraintOptions();
                    options.ConformingDelaunay = true;

                    triNetMeshes[i] = polygon.Triangulate(options) as TriangleNet.Mesh;
                }
            }
        });

        triNetMeshes = ReTriangulateEdges(TriNetMeshes);
    }
    
    /*async Task GenerateDividedMeshes(List<TriangleNet.Mesh> triNetMeshes)
    {
        float halfWidth = _RegionSize.x / 2;
        float halfHeight = _RegionSize.y / 2;

        foreach (TriangleNet.Mesh mesh in triNetMeshes)
        {
            List<Vector3> v = new List<Vector3>();
            List<Vector3> n = new List<Vector3>();
            List<Vector2> u = new List<Vector2>();
            List<Color> c = new List<Color>();
            List<int> t = new List<int>();

            IEnumerator<Triangle> triangleEnum = mesh.triangles.GetEnumerator();

            Debug.Log("Thread About to Start");
            await Task.Run(() =>
            {
                Debug.Log("Thread Started");

                for (int i = 0; i < mesh.triangles.Count; i++)
                {
                    if (!triangleEnum.MoveNext())
                    {
                        break;
                    }

                    Triangle currentTriangle = triangleEnum.Current;

                    Vector3 v0 = new Vector3((float)currentTriangle.vertices[2].x - halfWidth, GetVertexHeight((float)currentTriangle.vertices[2].x, (float)currentTriangle.vertices[2].y) - 90, (float)currentTriangle.vertices[2].y - halfHeight);
                    Vector3 v1 = new Vector3((float)currentTriangle.vertices[1].x - halfWidth, GetVertexHeight((float)currentTriangle.vertices[1].x, (float)currentTriangle.vertices[1].y) - 90, (float)currentTriangle.vertices[1].y - halfHeight);
                    Vector3 v2 = new Vector3((float)currentTriangle.vertices[0].x - halfWidth, GetVertexHeight((float)currentTriangle.vertices[0].x, (float)currentTriangle.vertices[0].y) - 90, (float)currentTriangle.vertices[0].y - halfHeight);

                    t.Add(v.Count);
                    t.Add(v.Count + 1);
                    t.Add(v.Count + 2);

                    v.Add(v0);
                    v.Add(v1);
                    v.Add(v2);

                    var normal = Vector3.Cross(v1 - v0, v2 - v0);

                    float averageY = ((GetVertexHeight((float)currentTriangle.vertices[2].x, (float)currentTriangle.vertices[2].y) + GetVertexHeight((float)currentTriangle.vertices[1].x, (float)currentTriangle.vertices[1].y) + GetVertexHeight((float)currentTriangle.vertices[0].x, (float)currentTriangle.vertices[0].y)) / 3f) - 90;

                    var color = EvaluateTriangleColor(currentTriangle, normal, averageY);

                    for (int x = 0; x < 3; x++)
                    {
                        n.Add(normal);
                        u.Add(Vector3.zero);
                        c.Add(color);
                    }
                }
            });
        }
    }*/
    

    async Task GenerateMesh()
    {

        float halfWidth = _RegionSize.x / 2;
        float halfHeight = _RegionSize.y / 2;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color> colors = new List<Color>();
        List<int> triangles = new List<int>();

        IEnumerator<Triangle> triangleEnum = _Mesh.triangles.GetEnumerator();

        Debug.Log("Thread About to Start");
        await Task.Run(() =>
        {
            Debug.Log("Thread Started");

            for (int i = 0; i < _Mesh.triangles.Count; i++)
            {
                if (!triangleEnum.MoveNext())
                {
                    break;
                }

                Triangle currentTriangle = triangleEnum.Current;

                Vector3 v0 = new Vector3((float)currentTriangle.vertices[2].x - halfWidth, GetVertexHeight((float)currentTriangle.vertices[2].x, (float)currentTriangle.vertices[2].y) - 90, (float)currentTriangle.vertices[2].y - halfHeight);
                Vector3 v1 = new Vector3((float)currentTriangle.vertices[1].x - halfWidth, GetVertexHeight((float)currentTriangle.vertices[1].x, (float)currentTriangle.vertices[1].y) - 90, (float)currentTriangle.vertices[1].y - halfHeight);
                Vector3 v2 = new Vector3((float)currentTriangle.vertices[0].x - halfWidth, GetVertexHeight((float)currentTriangle.vertices[0].x, (float)currentTriangle.vertices[0].y) - 90, (float)currentTriangle.vertices[0].y - halfHeight);

                triangles.Add(vertices.Count);
                triangles.Add(vertices.Count + 1);
                triangles.Add(vertices.Count + 2);

                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);

                var normal = Vector3.Cross(v1 - v0, v2 - v0);

                float averageY = ((GetVertexHeight((float)currentTriangle.vertices[2].x, (float)currentTriangle.vertices[2].y) + GetVertexHeight((float)currentTriangle.vertices[1].x, (float)currentTriangle.vertices[1].y) + GetVertexHeight((float)currentTriangle.vertices[0].x, (float)currentTriangle.vertices[0].y)) / 3f) - 90;

                var color = EvaluateTriangleColor(currentTriangle, normal, averageY);

                for (int x = 0; x < 3; x++)
                {
                    normals.Add(normal);
                    uvs.Add(Vector3.zero);
                    colors.Add(color);
                }
            }
        });

        try
        {
            Debug.Log(vertices.Count + " Verts to be meshed");
            Debug.Log(triangles.Count + " Tris to be meshed");

            _UnityMesh = new UnityEngine.Mesh();
            Debug.Log("Empty Mesh Made");
            _UnityMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Debug.Log("Format set to UInt32");
            _UnityMesh.vertices = vertices.ToArray();
            Debug.Log("Verts assigned to array");
            _UnityMesh.triangles = triangles.ToArray();
            Debug.Log("Tris assigned to array");
            _UnityMesh.uv = uvs.ToArray();
            Debug.Log("UVs assigned to array");
            _UnityMesh.colors = colors.ToArray();
            Debug.Log("Colors assigned to array");
            _UnityMesh.normals = normals.ToArray();
            Debug.Log("Normals aissnged to Array");

            _MeshCollider.sharedMesh = _UnityMesh;
            Debug.Log("Collision Mesh Created");
            _MeshFilter.mesh = _UnityMesh;
            Debug.Log("Mesh assigned to filter");
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        Debug.Log("Mesh Creation Finished");
    }
}
