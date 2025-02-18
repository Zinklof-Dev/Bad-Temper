using System;
using System.Collections.Generic;
using UnityEngine;
using ZinklofDev.Utils.Mapping;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using System.Threading.Tasks;

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

    private TriangleNet.Mesh _Mesh;
    private UnityEngine.Mesh _UnityMesh;

    private PerlinMap _PerlinMap;
    private float[,] _FalloffMap;

    public async Task Initialize(int seed)
    {
        // !!!!!! Troll is here :D
        //_HeightScale = 99999f;
    
        Polygon polygon = new Polygon();

        Debug.Log("Perlin, poisson, and falloff");
        _PerlinMap = await Noise.GenPerlinMapAsnyc(_imageSize, _imageSize, seed + 69, _Scale, _Octaves, _Persistance, _Lacunarity, _Offset);
        List<Vector2> points = await Noise.PoissonSamplingAsync(_Radius, _RegionSize, seed + 69);
        _FalloffMap = Noise.GenerateFalloffMap(_imageSize, 3, 25);

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

        Debug.Log("genMesh");
        await GenerateMesh();

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

        Debug.Log(vertices.Count + " Verts to be meshed");
        Debug.Log(triangles.Count + " Tris to be meshed");
        
        _UnityMesh = new UnityEngine.Mesh();
        _UnityMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _UnityMesh.vertices = vertices.ToArray();
        _UnityMesh.triangles = triangles.ToArray();
        _UnityMesh.uv = uvs.ToArray();
        _UnityMesh.colors = colors.ToArray();
        _UnityMesh.normals = normals.ToArray();

        _MeshCollider.sharedMesh = _UnityMesh;
        _MeshFilter.mesh = _UnityMesh;
    }
}
