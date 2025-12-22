using System.Collections.Generic;
using UnityEngine;

// Generates runtime border meshes dynamically based on mesh edges for visual highlighting
public class CountryBorderController : MonoBehaviour
{
    [Header("Country Root")]
    public Transform countriesParent;

    [Header("Border Settings")]
    public float staticBorderWidth = 0.02f;
    public float hoverBorderWidth = 0.04f;
    public float selectedBorderWidth = 0.05f;
    public float normalPush = 0.001f;
    public Color staticColor = Color.black;
    public Color hoverColor = Color.yellow;
    public Color selectedColor = Color.red;

    // Generates a thin static border for every country at initialization
    public void CreateStaticBorders()
    {
        // Iterate through all country meshes and generate static borders
        foreach (Transform c in countriesParent)
        {
            GameObject border = GenerateBorder(c.gameObject, staticBorderWidth, normalPush, staticColor);
            border.name = c.name + "_BorderStatic";
        }
    }

    // Replaces existing borders with a highlighted border for the selected country
    public void ShowSelectedBorder(GameObject country)
    {
        string staticName = country.name + "_BorderStatic";
        string hoverName = country.name + "_BorderHover";
        string runtimeName = country.name + "_BorderRuntime";

        // Remove any previously generated selected border
        var oldRuntime = country.transform.Find(runtimeName);
        if (oldRuntime) Destroy(oldRuntime.gameObject);

        // Create red border
        GameObject border = GenerateBorder(country, selectedBorderWidth, normalPush, selectedColor);
        border.name = runtimeName;

        // Temporarily hide static and hover borders
        var staticBorder = country.transform.Find(staticName);
        var hoverBorder = country.transform.Find(hoverName);
        if (staticBorder) staticBorder.gameObject.SetActive(false);
        if (hoverBorder) hoverBorder.gameObject.SetActive(false);
    }

    // Clears the selected border and restores the static border
    public void ClearSelectedBorder(GameObject country)
    {
        string staticName = country.name + "_BorderStatic";
        string runtimeName = country.name + "_BorderRuntime";

        var runtime = country.transform.Find(runtimeName);
        if (runtime) Destroy(runtime.gameObject);

        var staticBorder = country.transform.Find(staticName);
        if (staticBorder) staticBorder.gameObject.SetActive(true);
    }

    // Displays a hover border while the pointer is over a country
    public void ShowHoverBorder(GameObject country)
    {
        string staticName = country.name + "_BorderStatic";
        string hoverName = country.name + "_BorderHover";

        // Remove old hover
        var oldHover = country.transform.Find(hoverName);
        if (oldHover) Destroy(oldHover.gameObject);

        // Hide static
        var staticBorder = country.transform.Find(staticName);
        if (staticBorder) staticBorder.gameObject.SetActive(false);

        // Create hover border
        GameObject border = GenerateBorder(country, hoverBorderWidth, normalPush, hoverColor);
        border.name = hoverName;
    }

    // Removes the hover border and restores the static border
    public void ClearHoverBorder(GameObject country)
    {
        string staticName = country.name + "_BorderStatic";
        string hoverName = country.name + "_BorderHover";

        var hover = country.transform.Find(hoverName);
        if (hover) Destroy(hover.gameObject);

        var staticBorder = country.transform.Find(staticName);
        if (staticBorder) staticBorder.gameObject.SetActive(true);
    }

    // Dynamically generates a border mesh by extracting boundary edges from the country mesh
    private GameObject GenerateBorder(GameObject go, float width, float push, Color color)
    {
        var mf = go.GetComponent<MeshFilter>();
        var mesh = mf.sharedMesh;
        var verts = mesh.vertices;
        var tris = mesh.triangles;
        var norms = mesh.normals;

        if (norms == null || norms.Length != verts.Length)
        {
            mesh.RecalculateNormals();
            norms = mesh.normals;
        }

        // Build an edge map to count how many triangles share each edge
        var edgeMap = new Dictionary<EdgeKey, EdgeVal>(2048);
        for (int t = 0; t < tris.Length; t += 3)
        {
            AddEdge(edgeMap, tris[t], tris[t + 1]);
            AddEdge(edgeMap, tris[t + 1], tris[t + 2]);
            AddEdge(edgeMap, tris[t + 2], tris[t]);
        }

        // Boundary edges are those referenced by only one triangle
        var boundaryEdges = new List<System.Tuple<int, int>>();
        foreach (var kv in edgeMap)
            if (kv.Value.count == 1)
                boundaryEdges.Add(new System.Tuple<int, int>(kv.Key.a, kv.Key.b));

        float half = Mathf.Max(1e-6f, width * 0.5f);
        var outVerts = new List<Vector3>();
        var outTris = new List<int>();
        int quadIndex = 0;

        foreach (var e in boundaryEdges)
        {
            int ia = e.Item1;
            int ib = e.Item2;
            var va = verts[ia];
            var vb = verts[ib];

            var na = norms[ia];
            var nb = norms[ib];
            var nAvg = (na + nb).normalized;

            var edgeDir = (vb - va).normalized;
            var tangent = Vector3.Cross(nAvg, edgeDir).normalized;
            var pushVec = nAvg * push;

            // Expand each boundary edge into a thin quad to create visible border geometry
            var v0 = va + tangent * half + pushVec;
            var v1 = vb + tangent * half + pushVec;
            var v2 = vb - tangent * half + pushVec;
            var v3 = va - tangent * half + pushVec;

            outVerts.Add(v0); outVerts.Add(v1); outVerts.Add(v2); outVerts.Add(v3);

            outTris.Add(quadIndex * 4 + 2);
            outTris.Add(quadIndex * 4 + 1);
            outTris.Add(quadIndex * 4 + 0);
            outTris.Add(quadIndex * 4 + 3);
            outTris.Add(quadIndex * 4 + 2);
            outTris.Add(quadIndex * 4 + 0);

            quadIndex++;
        }

        var borderMesh = new Mesh();
        borderMesh.name = go.name + "_BorderMeshRuntime";
        borderMesh.SetVertices(outVerts);
        borderMesh.SetTriangles(outTris, 0);
        borderMesh.RecalculateNormals();
        borderMesh.RecalculateBounds();

        var borderObj = new GameObject(go.name + "_BorderRuntime");
        borderObj.transform.SetParent(go.transform, false);

        var cmf = borderObj.AddComponent<MeshFilter>();
        var cmr = borderObj.AddComponent<MeshRenderer>();
        cmf.sharedMesh = borderMesh;

        // Configure an unlit material so borders are always clearly visible
        Shader sh = Shader.Find("Unlit/Color") ??
                    Shader.Find("Universal Render Pipeline/Unlit") ??
                    Shader.Find("Legacy Shaders/Transparent/Diffuse");
        var mat = new Material(sh);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

        mat.renderQueue = 3100;
        if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0);
        if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
        cmr.sharedMaterial = mat;

        cmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        cmr.receiveShadows = false;

        return borderObj;
    }

    // Represents an undirected mesh edge
    private struct EdgeKey
    {
        public int a, b;
        public EdgeKey(int i, int j) { if (i < j) { a = i; b = j; } else { a = j; b = i; } }
        public override int GetHashCode() => a * 73856093 ^ b * 19349663;
        public override bool Equals(object obj)
        {
            if (!(obj is EdgeKey)) return false;
            var o = (EdgeKey)obj; return a == o.a && b == o.b;
        }
    }

    private struct EdgeVal { public int count; }

    // Adds an edge to the map or increments its usage count
    private static void AddEdge(Dictionary<EdgeKey, EdgeVal> map, int i, int j)
    {
        var key = new EdgeKey(i, j);
        if (map.TryGetValue(key, out var val)) { val.count++; map[key] = val; }
        else map[key] = new EdgeVal { count = 1 };
    }
}
