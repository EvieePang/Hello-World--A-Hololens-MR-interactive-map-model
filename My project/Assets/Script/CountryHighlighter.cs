using System.Collections.Generic;
using UnityEngine;

public class CountryHighlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color highlightColor = Color.red;
    public float borderWidth = 0.02f;   // 边框加粗
    public float normalPush = 0.001f;  // 防止Z-Fighting
    public float transparentAlpha = 0.2f; // 内部透明度

    private GameObject currentHighlighted;
    private Material originalMat;
    private GameObject currentBorder;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject country = hit.collider.gameObject;
                if (country == currentHighlighted)
                    return;
                HighlightCountry(country);
            }
            else
            {
                ClearHighlight();
            }
        }
    }

    void HighlightCountry(GameObject country)
    {
        ClearHighlight();

        var mr = country.GetComponent<MeshRenderer>();
        if (mr == null) return;
        var mf = country.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        currentHighlighted = country;
        originalMat = mr.sharedMaterial;

        // ============= 1️⃣ 创建支持透明的副本材质 =============
        Material transparentMat = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
        transparentMat.color = new Color(1f, 1f, 1f, 1f); // 先设置为白色

        // 如果原材质有主纹理，则保留
        if (mr.sharedMaterial.HasProperty("_MainTex"))
        {
            Texture tex = mr.sharedMaterial.mainTexture;
            transparentMat.mainTexture = tex;
        }

        // 设置透明度
        Color baseColor = Color.white;
        if (mr.sharedMaterial.HasProperty("_Color"))
            baseColor = mr.sharedMaterial.color;

        baseColor.a = transparentAlpha; // 比如 0.3f
        transparentMat.color = baseColor;

        // 应用新材质
        mr.sharedMaterial = transparentMat;


        // -------- 2️⃣ 生成红色边界 --------
        currentBorder = GenerateBorder(country, borderWidth, normalPush, highlightColor);
    }

    void ClearHighlight()
    {
        if (currentHighlighted != null)
        {
            var mr = currentHighlighted.GetComponent<MeshRenderer>();
            if (mr != null && originalMat != null)
                mr.sharedMaterial = originalMat;
        }

        if (currentBorder != null)
            Destroy(currentBorder);

        currentHighlighted = null;
        currentBorder = null;
        originalMat = null;
    }

    GameObject GenerateBorder(GameObject go, float width, float push, Color color)
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

        var edgeMap = new Dictionary<EdgeKey, EdgeVal>(2048);
        for (int t = 0; t < tris.Length; t += 3)
        {
            AddEdge(edgeMap, tris[t], tris[t + 1]);
            AddEdge(edgeMap, tris[t + 1], tris[t + 2]);
            AddEdge(edgeMap, tris[t + 2], tris[t]);
        }

        List<System.Tuple<int, int>> boundaryEdges = new List<System.Tuple<int, int>>();
        foreach (var kv in edgeMap)
        {
            if (kv.Value.count == 1)
                boundaryEdges.Add(new System.Tuple<int, int>(kv.Key.a, kv.Key.b));
        }

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

            var v0 = va + tangent * half + pushVec;
            var v1 = vb + tangent * half + pushVec;
            var v2 = vb - tangent * half + pushVec;
            var v3 = va - tangent * half + pushVec;

            outVerts.Add(v0);
            outVerts.Add(v1);
            outVerts.Add(v2);
            outVerts.Add(v3);

            outTris.Add(quadIndex * 4 + 2);
            outTris.Add(quadIndex * 4 + 1);
            outTris.Add(quadIndex * 4 + 0);
            outTris.Add(quadIndex * 4 + 3);
            outTris.Add(quadIndex * 4 + 2);
            outTris.Add(quadIndex * 4 + 0);
            quadIndex++;
        }

        var borderMesh = new Mesh();
        borderMesh.SetVertices(outVerts);
        borderMesh.SetTriangles(outTris, 0);
        borderMesh.RecalculateNormals();

        var borderObj = new GameObject(go.name + "_BorderRuntime");
        borderObj.transform.SetParent(go.transform, false);
        var cmf = borderObj.AddComponent<MeshFilter>();
        var cmr = borderObj.AddComponent<MeshRenderer>();
        cmf.mesh = borderMesh;

        // ---------- 创建红色材质 ----------
        Shader sh = Shader.Find("Unlit/Color");
        if (!sh) sh = Shader.Find("Universal Render Pipeline/Unlit");
        var mat = new Material(sh);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        mat.renderQueue = 3000; // Transparent
        if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0); // 双面渲染
        cmr.sharedMaterial = mat;

        return borderObj;
    }

    struct EdgeKey { public int a, b; public EdgeKey(int i, int j) { if (i < j) { a = i; b = j; } else { a = j; b = i; } } public override int GetHashCode() => a * 73856093 ^ b * 19349663; public override bool Equals(object obj) { if (!(obj is EdgeKey)) return false; var o = (EdgeKey)obj; return a == o.a && b == o.b; } }
    struct EdgeVal { public int count; }
    static void AddEdge(Dictionary<EdgeKey, EdgeVal> map, int i, int j)
    {
        var key = new EdgeKey(i, j);
        if (map.TryGetValue(key, out var val)) { val.count++; map[key] = val; }
        else map[key] = new EdgeVal { count = 1 };
    }
}
