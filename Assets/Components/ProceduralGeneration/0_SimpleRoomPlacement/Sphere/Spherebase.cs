using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class IcoSphere : MonoBehaviour
{
    [Range(0, 6)]
    public int subdivisions;

    [Range(0.1f, 10f)]
    public float radius;

    [Header("Extrusion Settings")]
    public bool applyExtrusion = false;
    public int numberOfLevels;
    public float extrusionAmount;


    [Header("Debug Wireframe")]
    public bool showWireframe = true;
    public Color wireframeColor = Color.black;

    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Color> vertexColors;
    private Dictionary<long, int> middlePointIndexCache;

    private class Triangle
    {
        public int v1, v2, v3;
        public int elevationLevel;
        public Vector3 normal;
        public Vector3 center;

        public Triangle(int v1, int v2, int v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }

    void Start()
    {
        GenerateIcoSphere();
        Material mat = GetComponent<MeshRenderer>().material;
        mat.SetInt("_Cull", 0);
    }

    private void Update()
    {
        transform.localEulerAngles = new Vector3(0, 90, 0);
    }

    public void GenerateIcoSphere()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        vertices = new List<Vector3>();
        triangles = new List<int>();
        vertexColors = new List<Color>();
        middlePointIndexCache = new Dictionary<long, int>();

        GenerateIcosahedron();

        for (int i = 0; i < subdivisions; i++)
        {
            SubdivideTriangles();
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = vertices[i].normalized * radius;
        }

        if (applyExtrusion)
        {
            ApplyExtrusion();
        }
        Debug.Log($"Vertices: {vertices.Count}, VertexColors: {vertexColors.Count}");

        UpdateMesh();

    }

    private void ApplyExtrusion()
    {
        List<Triangle> triangleList = new List<Triangle>();
        Dictionary<int, int> levelCount = new Dictionary<int, int>();
        for (int i = 0; i < triangles.Count; i += 3)
        {
            Triangle tri = new Triangle(triangles[i], triangles[i + 1], triangles[i + 2]);

            tri.center = (vertices[tri.v1] + vertices[tri.v2] + vertices[tri.v3]) / 3f;

            Vector3 n1 = vertices[tri.v1].normalized;
            Vector3 n2 = vertices[tri.v2].normalized;
            Vector3 n3 = vertices[tri.v3].normalized;
            tri.normal = ((n1 + n2 + n3) / 3f).normalized;

            float noiseValue = Mathf.PerlinNoise(tri.center.x * 2f, tri.center.z * 5f);
            tri.elevationLevel = Mathf.FloorToInt(noiseValue * numberOfLevels);

            if (!levelCount.ContainsKey(tri.elevationLevel))
                levelCount[tri.elevationLevel] = 0;
            levelCount[tri.elevationLevel]++;

            triangleList.Add(tri);
        }

        string distribution = "Elevation distribution: ";
        foreach (var kvp in levelCount)
            distribution += $"Level {kvp.Key}: {kvp.Value} triangles | ";
        Debug.Log(distribution);

        Dictionary<int, List<Triangle>> vertexToTriangles = new Dictionary<int, List<Triangle>>();
        foreach (Triangle tri in triangleList)
        {
            if (!vertexToTriangles.ContainsKey(tri.v1)) vertexToTriangles[tri.v1] = new List<Triangle>();
            if (!vertexToTriangles.ContainsKey(tri.v2)) vertexToTriangles[tri.v2] = new List<Triangle>();
            if (!vertexToTriangles.ContainsKey(tri.v3)) vertexToTriangles[tri.v3] = new List<Triangle>();

            vertexToTriangles[tri.v1].Add(tri);
            vertexToTriangles[tri.v2].Add(tri);
            vertexToTriangles[tri.v3].Add(tri);
        }

        Dictionary<long, int> extrudedVertexCache = new Dictionary<long, int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Color> newColors = new List<Color>();

        System.Func<int, int, int> GetOrCreateExtrudedVertex = (int originalVertexIndex, int elevationLevel) =>
        {
            long key = ((long)originalVertexIndex << 32) | (long)elevationLevel;
            if (extrudedVertexCache.TryGetValue(key, out int cached))
                return cached;

            Vector3 originalPos = vertices[originalVertexIndex];
            List<Triangle> adjacentSameLevel = vertexToTriangles[originalVertexIndex]
                .Where(t => t.elevationLevel == elevationLevel)
                .ToList();

            Vector3 averageNormal = Vector3.zero;
            foreach (Triangle adjTri in adjacentSameLevel)
                averageNormal += adjTri.normal;

            if (averageNormal == Vector3.zero)
                averageNormal = originalPos.normalized;
            else
                averageNormal.Normalize();

            float extrusionDistance = elevationLevel * extrusionAmount;

            Vector3 newPos = originalPos + averageNormal * extrusionDistance;

            float normalizedElevation = numberOfLevels > 0 ? (float)elevationLevel / (float)numberOfLevels : 0f;

            float r = Mathf.Lerp(0.0f, 0.5f, normalizedElevation);
            float g = Mathf.Lerp(0.3f, 1.0f, normalizedElevation);
            float b = Mathf.Lerp(0.0f, 0.3f, normalizedElevation);


            float steps = 20f;
            r = Mathf.Floor(r * steps + 0.5f) / steps;
            g = Mathf.Floor(g * steps + 0.5f) / steps;
            b = Mathf.Floor(b * steps + 0.5f) / steps;

            Color vertexColor = new Color(r, g, b, 1f);

            int newIndex = newVertices.Count;
            newVertices.Add(newPos);
            newColors.Add(vertexColor);
            extrudedVertexCache[key] = newIndex;
            return newIndex;
        };

        List<int> newTriangles = new List<int>();

        foreach (Triangle tri in triangleList)
        {
            int idx1 = GetOrCreateExtrudedVertex(tri.v1, tri.elevationLevel);
            int idx2 = GetOrCreateExtrudedVertex(tri.v2, tri.elevationLevel);
            int idx3 = GetOrCreateExtrudedVertex(tri.v3, tri.elevationLevel);

            newTriangles.Add(idx1);
            newTriangles.Add(idx3);
            newTriangles.Add(idx2);
        }

        Dictionary<long, List<(Triangle tri, int v1, int v2)>> edgeToTriangles = new Dictionary<long, List<(Triangle, int, int)>>();

        foreach (Triangle tri in triangleList)
        {
            int[][] edges = new int[][]
            {
        new int[] { tri.v1, tri.v2 },
        new int[] { tri.v2, tri.v3 },
        new int[] { tri.v3, tri.v1 }
            };

            foreach (int[] edge in edges)
            {
                long edgeKey = ((long)Mathf.Min(edge[0], edge[1]) << 32) | (long)Mathf.Max(edge[0], edge[1]);
                if (!edgeToTriangles.ContainsKey(edgeKey))
                    edgeToTriangles[edgeKey] = new List<(Triangle, int, int)>();

                edgeToTriangles[edgeKey].Add((tri, edge[0], edge[1]));
            }
        }

        HashSet<(int, int, int, int)> addedWalls = new HashSet<(int, int, int, int)>();

        foreach (var kvp in edgeToTriangles)
        {
            var trisOnEdge = kvp.Value;
            if (trisOnEdge.Count < 2)
                continue;

            var (tri1, v1a, v2a) = trisOnEdge[0];
            var (tri2, v1b, v2b) = trisOnEdge[1];
            if (tri1.elevationLevel == tri2.elevationLevel)
                continue;

            int lowLevel = Mathf.Min(tri1.elevationLevel, tri2.elevationLevel);
            int highLevel = Mathf.Max(tri1.elevationLevel, tri2.elevationLevel);

            for (int level = lowLevel; level < highLevel; level++)
            {
                bool levelExists = triangleList.Any(t => t.elevationLevel == level || t.elevationLevel == level + 1);
                if (!levelExists) continue;

                var wallKey = (Mathf.Min(v1a, v2a), Mathf.Max(v1a, v2a), level, level + 1);
                if (addedWalls.Contains(wallKey))
                    continue;
                addedWalls.Add(wallKey);

                int idxA_low = GetOrCreateExtrudedVertex(v1a, level);
                int idxB_low = GetOrCreateExtrudedVertex(v2a, level);
                int idxA_high = GetOrCreateExtrudedVertex(v1a, level + 1);
                int idxB_high = GetOrCreateExtrudedVertex(v2a, level + 1);

                Vector3 posA_low = newVertices[idxA_low];
                Vector3 posB_low = newVertices[idxB_low];
                Vector3 posA_high = newVertices[idxA_high];
                Vector3 posB_high = newVertices[idxB_high];

                Color colorA_low = newColors[idxA_low];
                Color colorB_low = newColors[idxB_low];
                Color colorA_high = newColors[idxA_high];
                Color colorB_high = newColors[idxB_high];

                int quadV1 = newVertices.Count;
                newVertices.Add(posA_low);
                newColors.Add(colorA_low);

                int quadV2 = newVertices.Count;
                newVertices.Add(posB_low);
                newColors.Add(colorB_low);

                int quadV3 = newVertices.Count;
                newVertices.Add(posA_high);
                newColors.Add(colorA_low);

                int quadV4 = newVertices.Count;
                newVertices.Add(posB_high);
                newColors.Add(colorB_low);

                //---------- RENDER BOTH FACES ----------// TO FIX
                newTriangles.Add(quadV3);
                newTriangles.Add(quadV1);
                newTriangles.Add(quadV2);

                newTriangles.Add(quadV3);
                newTriangles.Add(quadV2);
                newTriangles.Add(quadV4);


                newTriangles.Add(quadV2);
                newTriangles.Add(quadV1);
                newTriangles.Add(quadV3);

                newTriangles.Add(quadV2);
                newTriangles.Add(quadV3);
                newTriangles.Add(quadV4);
                //-------------------------------------//
            }
        }

        vertices = newVertices;
        triangles = newTriangles;
        vertexColors = newColors;
    }


    public void GenerateIcosahedron()
    {
        float phi = (1f + Mathf.Sqrt(5f)) / 2f;
        float a = 1f;
        float b = 1f / phi;

        float scale = 1f / Mathf.Sqrt(a * a + b * b);
        a *= scale;
        b *= scale;

        AddVertex(new Vector3(0, b, -a));
        AddVertex(new Vector3(b, a, 0));
        AddVertex(new Vector3(-b, a, 0));
        AddVertex(new Vector3(0, b, a));
        AddVertex(new Vector3(0, -b, a));
        AddVertex(new Vector3(-a, 0, b));
        AddVertex(new Vector3(0, -b, -a));
        AddVertex(new Vector3(a, 0, -b));
        AddVertex(new Vector3(a, 0, b));
        AddVertex(new Vector3(-a, 0, -b));
        AddVertex(new Vector3(b, -a, 0));
        AddVertex(new Vector3(-b, -a, 0));

        int[][] faces = new int[][]
      {
            new int[] {0, 1, 2},
            new int[] {3, 2, 1},
            new int[] {3, 4, 5},
            new int[] {3, 8, 4},
            new int[] {0, 6, 7},
            new int[] {0, 9, 6},
            new int[] {4, 10, 11},
            new int[] {6, 11, 10},
            new int[] {2, 5, 9},
            new int[] {11, 9, 5},
            new int[] {1, 7, 8},
            new int[] {10, 8, 7},
            new int[] {3, 5, 2},
            new int[] {3, 1, 8},
            new int[] {0, 2, 9},
            new int[] {0, 7, 1},
            new int[] {6, 9, 11},
            new int[] {6, 10, 7},
            new int[] {4, 11, 5},
            new int[] {4, 8, 10}
      };

        foreach (int[] face in faces)
        {
            triangles.Add(face[0]);
            triangles.Add(face[1]);
            triangles.Add(face[2]);
        }
    }

    public void SubdivideTriangles()
    {
        List<int> newTriangles = new List<int>();

        for (int i = 0; i < triangles.Count; i += 3)
        {
            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            int a = GetMiddlePoint(v1, v2);
            int b = GetMiddlePoint(v2, v3);
            int c = GetMiddlePoint(v3, v1);

            newTriangles.Add(v1); newTriangles.Add(a); newTriangles.Add(c);
            newTriangles.Add(v2); newTriangles.Add(b); newTriangles.Add(a);
            newTriangles.Add(v3); newTriangles.Add(c); newTriangles.Add(b);
            newTriangles.Add(a); newTriangles.Add(b); newTriangles.Add(c);

        }
        triangles = newTriangles;
        middlePointIndexCache.Clear();
    }

    int GetMiddlePoint(int p1, int p2)
    {
        long smallerIndex = Mathf.Min(p1, p2);
        long greaterIndex = Mathf.Max(p1, p2);
        long key = (smallerIndex << 32) + greaterIndex;

        if (middlePointIndexCache.ContainsKey(key))
        {
            return middlePointIndexCache[key];
        }

        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = (point1 + point2) / 2f;

        middle = middle.normalized;

        int index = AddVertex(middle);
        middlePointIndexCache.Add(key, index);

        return index;
    }

    int AddVertex(Vector3 vertex)
    {
        vertices.Add(vertex);
        return vertices.Count - 1;
    }

    public void UpdateMesh()
    {
        List<Vector3> flatVertices = new List<Vector3>();
        List<int> flatTriangles = new List<int>();
        List<Color> flatColors = new List<Color>();

        int maxTriangleIndex = triangles.Max();
        Debug.Log($"UpdateMesh: triangles.Count={triangles.Count}, vertices.Count={vertices.Count}, vertexColors.Count={vertexColors.Count}, maxIndex={maxTriangleIndex}");

        if (maxTriangleIndex >= vertices.Count)
        {
            Debug.LogError($"PROBLÈME: Triangle index {maxTriangleIndex} >= vertices.Count {vertices.Count}");
        }
        if (maxTriangleIndex >= vertexColors.Count)
        {
            Debug.LogError($"PROBLÈME: Triangle index {maxTriangleIndex} >= vertexColors.Count {vertexColors.Count}");
        }

        // Pour chaque triangle
        for (int i = 0; i < triangles.Count; i += 3)
        {
            int i1 = triangles[i];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Color c1 = vertexColors.Count > i1 ? vertexColors[i1] : Color.white;
            Color c2 = vertexColors.Count > i2 ? vertexColors[i2] : Color.white;
            Color c3 = vertexColors.Count > i3 ? vertexColors[i3] : Color.white;

            if (c1 == Color.white || c2 == Color.white || c3 == Color.white)
            {
                Debug.LogWarning($"Triangle {i / 3}: indices [{i1}, {i2}, {i3}], colors [{c1}, {c2}, {c3}]");
            }

            Vector3 faceNormal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

            int baseIndex = flatVertices.Count;
            flatVertices.Add(v1);
            flatVertices.Add(v2);
            flatVertices.Add(v3);

            flatTriangles.Add(baseIndex);
            flatTriangles.Add(baseIndex + 1);
            flatTriangles.Add(baseIndex + 2);

            flatColors.Add(c1);
            flatColors.Add(c2);
            flatColors.Add(c3);
        }

        mesh.Clear();
        mesh.SetVertices(flatVertices);
        mesh.SetTriangles(flatTriangles, 0);
        mesh.SetColors(flatColors);

        Vector3[] normals = new Vector3[flatVertices.Count];
        for (int i = 0; i < flatTriangles.Count; i += 3)
        {
            Vector3 v1 = flatVertices[flatTriangles[i]];
            Vector3 v2 = flatVertices[flatTriangles[i + 1]];
            Vector3 v3 = flatVertices[flatTriangles[i + 2]];

            Vector3 faceNormal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
            normals[flatTriangles[i]] = faceNormal;
            normals[flatTriangles[i + 1]] = faceNormal;
            normals[flatTriangles[i + 2]] = faceNormal;
        }
 
        mesh.normals = normals;
        mesh.RecalculateBounds();
        Debug.Log("Vertices: " + flatVertices.Count + ", VertexColors: " + flatColors.Count);
    }


    void OnValidate()
    {
        if (Application.isPlaying && mesh != null)
        {
            GenerateIcoSphere();
        }
    }

    void OnRenderObject()
    {
        if (!showWireframe || mesh == null || !Application.isPlaying) return;

        Material lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        GL.Color(wireframeColor);

        for (int i = 0; i < triangles.Count; i += 3)
        {
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            GL.Vertex(v1); GL.Vertex(v2);
            GL.Vertex(v2); GL.Vertex(v3);
            GL.Vertex(v3); GL.Vertex(v1);
        }

        GL.End();
        GL.PopMatrix();
    }
}