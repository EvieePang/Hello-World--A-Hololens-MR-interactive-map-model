using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class CountryBorderBuilder : EditorWindow
{
    [Tooltip("outline width (world/local unit")]
    public float borderWidth = 0.02f; // 2 cm 
    [Tooltip("To avoid Z-fighting with country object, outside the country mesh a little")]
    public float normalPush = 0.001f;
    [Tooltip("Outline color")]
    public Color borderColor = Color.red;
    [Tooltip("Whether to set the material as double faces")]
    public bool doubleSided = true;

    [MenuItem("Tools/Geo/Build Country Borders")]
    public static void ShowWindow()
    {
        GetWindow<CountryBorderBuilder>("Country Borders");
    }

    void OnGUI()
    {
        GUILayout.Label("Build Borders from Mesh Boundary", EditorStyles.boldLabel);
        borderWidth = EditorGUILayout.Slider(new GUIContent("Border Width"), borderWidth, 0.001f, 0.5f);
        normalPush = EditorGUILayout.Slider(new GUIContent("Normal Push"), normalPush, 0f, 0.01f);
        borderColor = EditorGUILayout.ColorField("Border Color", borderColor);
        doubleSided = EditorGUILayout.Toggle("Double Sided", doubleSided);

        EditorGUILayout.Space();

        if (GUILayout.Button("Build for Selected Mesh Objects"))
        {
            BuildForSelection();
        }

        EditorGUILayout.HelpBox(
            "User£º\n1) multi select countries object in Hierarchy (need MeshFilter£©¡£\n" +
            "2) tap the button to auto generate ¡°*_Border¡± child object£¨Unlit color material£©¡£\n" +
            "3) Set the country object as transparent£¨BaseColor ¦Á=0 / Surface=Transparent£©, then can only show the outline",
            MessageType.Info);
    }

    void BuildForSelection()
    {
        var objs = Selection.gameObjects;
        if (objs == null || objs.Length == 0)
        {
            EditorUtility.DisplayDialog("Country Borders", "First, choose one country object which has MeshFilter Hierarchy", "OK");
            return;
        }

        int total = 0;
        foreach (var go in objs)
        {
            total += BuildOne(go);
        }
        EditorUtility.DisplayDialog("Country Borders", $"Compelete: for {objs.Length} countries object generate {total} outline", "OK");
    }

    int BuildOne(GameObject go)
    {
        var mf = go.GetComponent<MeshFilter>();
        if (!mf || !mf.sharedMesh)
        {
            Debug.LogWarning($"[CountryBorderBuilder] skip {go.name}£¨non MeshFilter or empty mesh£©.");
            return 0;
        }
        var mesh = mf.sharedMesh;
        if (!mesh.isReadable)
        {
            Debug.LogError($"[CountryBorderBuilder] {go.name} used Mesh without Read/Write Enabled£¬can not read the vertex. Open it.");
            return 0;
        }

        var verts = mesh.vertices;
        var norms = mesh.normals;
        var tris = mesh.triangles;

        if (norms == null || norms.Length != verts.Length)
        {
            // if no normal line, use polygon interpretation temporarily
            mesh.RecalculateNormals();
            norms = mesh.normals;
        }

        // collect outline's outline: only use by one mesh's triangular
        var edgeMap = new Dictionary<EdgeKey, EdgeVal>(2048);

        for (int t = 0; t < tris.Length; t += 3)
        {
            int i0 = tris[t], i1 = tris[t + 1], i2 = tris[t + 2];

            AddEdge(edgeMap, i0, i1);
            AddEdge(edgeMap, i1, i2);
            AddEdge(edgeMap, i2, i0);
        }

        var boundaryEdges = ListPool<System.Tuple<int, int>>.Get();
        foreach (var kv in edgeMap)
        {
            if (kv.Value.count == 1)
            {
                boundaryEdges.Add(new System.Tuple<int, int>(kv.Key.a, kv.Key.b));
            }
        }

        if (boundaryEdges.Count == 0)
        {
            Debug.LogWarning($"[CountryBorderBuilder] {go.name} not find.");
            ListPool<System.Tuple<int, int>>.Release(boundaryEdges);
            return 0;
        }

        // according to every line made a widen rectangle
        float half = Mathf.Max(1e-6f, borderWidth * 0.5f);

        var outVerts = new List<Vector3>(boundaryEdges.Count * 4);
        var outTris = new List<int>(boundaryEdges.Count * 6);
        var outCols = new List<Color>(boundaryEdges.Count * 4);

        int quadIndex = 0;
        foreach (var e in boundaryEdges)
        {
            int ia = e.Item1;
            int ib = e.Item2;
            var va = verts[ia];
            var vb = verts[ib];

            var na = norms[ia];
            var nb = norms[ib];
            var nAvg = (na + nb);
            if (nAvg.sqrMagnitude < 1e-12f) nAvg = na.sqrMagnitude > 0 ? na : Vector3.up;
            nAvg.Normalize();

            var edgeDir = (vb - va);
            if (edgeDir.sqrMagnitude < 1e-12f) continue; 
            edgeDir.Normalize();

            // Widen direction
            var tangent = Vector3.Cross(nAvg, edgeDir);
            if (tangent.sqrMagnitude < 1e-12f)
            {
                // change to a similar one
                tangent = Vector3.Cross(nb, edgeDir);
                if (tangent.sqrMagnitude < 1e-12f) tangent = Vector3.up;
            }
            tangent.Normalize();

            var push = nAvg * normalPush;

            // a rectangle£¨a+, b+, b-, a-£©and push outside to avoid Z-fighting
            var v0 = va + tangent * half + push;
            var v1 = vb + tangent * half + push;
            var v2 = vb - tangent * half + push;
            var v3 = va - tangent * half + push;

            outVerts.Add(v0);
            outVerts.Add(v1);
            outVerts.Add(v2);
            outVerts.Add(v3);

            // 2 triangles: (0,1,2), (0,2,3)
            outTris.Add(quadIndex * 4 + 0);
            outTris.Add(quadIndex * 4 + 1);
            outTris.Add(quadIndex * 4 + 2);

            outTris.Add(quadIndex * 4 + 0);
            outTris.Add(quadIndex * 4 + 2);
            outTris.Add(quadIndex * 4 + 3);

            outTris.Add(quadIndex * 4 + 2);
            outTris.Add(quadIndex * 4 + 1);
            outTris.Add(quadIndex * 4 + 0);
            outTris.Add(quadIndex * 4 + 3);
            outTris.Add(quadIndex * 4 + 2);
            outTris.Add(quadIndex * 4 + 0);

            // vertax color£¨use for Unlit/Color or URP Unlit _BaseColor£©
            outCols.Add(borderColor);
            outCols.Add(borderColor);
            outCols.Add(borderColor);
            outCols.Add(borderColor);

            quadIndex++;
        }

        var borderMesh = new Mesh();
        borderMesh.indexFormat = (outVerts.Count > 65000) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        borderMesh.SetVertices(outVerts);
        borderMesh.SetTriangles(outTris, 0, true);
        borderMesh.SetColors(outCols);
        borderMesh.RecalculateNormals();
        borderMesh.RecalculateBounds();

        // build child object
        var child = new GameObject($"{go.name}_Border");
        child.transform.SetParent(go.transform, false);
        var cmf = child.AddComponent<MeshFilter>();
        var cmr = child.AddComponent<MeshRenderer>();

        // Save outline mesh as asset
        #if UNITY_EDITOR
                string meshPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/{go.name}_BorderMesh.asset");
                AssetDatabase.CreateAsset(borderMesh, meshPath);
                AssetDatabase.SaveAssets();
                borderMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath); // make sure it is a disk asset
        #endif
                cmf.sharedMesh = borderMesh;

        // choose a available Unlit shader£¨first URP£©
        Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (!sh) sh = Shader.Find("Unlit/Color");
        if (!sh) sh = Shader.Find("Legacy Shaders/Transparent/Diffuse"); 

        var mat = new Material(sh);
        // try to set color properties
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", borderColor);
        else if (mat.HasProperty("_Color")) mat.SetColor("_Color", borderColor);

        // transparent, double faces (if support)
        mat.renderQueue = 3000; // Transparent
        TrySetKeyword(mat, "_SURFACE_TYPE_TRANSPARENT", true);
        TrySetFloat(mat, "_Surface", 1f); // URP: 0=Opaque,1=Transparent
        if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0);

        // save the material
        #if UNITY_EDITOR
            string matPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/{go.name}_BorderMat.mat");
                AssetDatabase.CreateAsset(mat, matPath);
                AssetDatabase.SaveAssets();
                mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        #endif
        
        cmr.sharedMaterial = mat;

        // record the change to prefab
        #if UNITY_EDITOR
                PrefabUtility.RecordPrefabInstancePropertyModifications(child.transform);
                PrefabUtility.RecordPrefabInstancePropertyModifications(cmf);
                PrefabUtility.RecordPrefabInstancePropertyModifications(cmr);
        #endif

        ListPool<System.Tuple<int, int>>.Release(boundaryEdges);
        return quadIndex;
    }

    struct EdgeKey
    {
        public int a, b;
        public EdgeKey(int i, int j)
        {
            if (i < j) { a = i; b = j; }
            else { a = j; b = i; }
        }
        public override int GetHashCode() => a * 73856093 ^ b * 19349663;
        public override bool Equals(object obj)
        {
            if (!(obj is EdgeKey)) return false;
            var o = (EdgeKey)obj;
            return a == o.a && b == o.b;
        }
    }
    struct EdgeVal { public int count; }

    static void AddEdge(Dictionary<EdgeKey, EdgeVal> map, int i, int j)
    {
        var key = new EdgeKey(i, j);
        if (map.TryGetValue(key, out var val))
        {
            val.count++;
            map[key] = val;
        }
        else
        {
            map[key] = new EdgeVal { count = 1 };
        }
    }

    static void TrySetFloat(Material m, string name, float value)
    {
        if (m.HasProperty(name))
            m.SetFloat(name, value);
    }

    static void TrySetKeyword(Material m, string kw, bool on)
    {
        if (on) m.EnableKeyword(kw); else m.DisableKeyword(kw);
    }

    // simple object pool, avoid GC
    static class ListPool<T>
    {
        static readonly Stack<List<T>> pool = new Stack<List<T>>();
        public static List<T> Get()
        {
            return pool.Count > 0 ? pool.Pop() : new List<T>();
        }
        public static void Release(List<T> list)
        {
            list.Clear();
            pool.Push(list);
        }
    }
}
