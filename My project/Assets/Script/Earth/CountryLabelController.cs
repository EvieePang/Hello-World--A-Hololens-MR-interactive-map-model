using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages country name labels: creation + LOD (fade in/out with earth scale).
/// </summary>
public class CountryLabelController : MonoBehaviour
{
    [Header("Refs")]
    public Transform countriesParent;
    public Transform earthTransform;
    public Transform cameraTransform;
    public GameObject labelPrefab;

    // All labels for each country
    private Dictionary<GameObject, GameObject> countryLabels = new Dictionary<GameObject, GameObject>();

    // Countries whose labels should always be visible (e.g., selected or hovered)
    private HashSet<GameObject> forcedVisible = new HashSet<GameObject>();

    public IReadOnlyDictionary<GameObject, GameObject> Labels => countryLabels;

    /// <summary>
    /// Create labels for all countries once, and keep them initially hidden.
    /// Call this from CountryClickController.Start().
    /// </summary>
    public void InitLabels()
    {
        countryLabels.Clear();

        foreach (Transform c in countriesParent)
        {
            GameObject label = CreateLabelObject(c.gameObject);
            label.SetActive(false);
            countryLabels[c.gameObject] = label;
        }
    }

    /// <summary>
    /// Force a country's label to always be visible (e.g., when selected or hovered).
    /// </summary>
    public void SetForcedVisible(GameObject country, bool forced)
    {
        if (forced)
            forcedVisible.Add(country);
        else
            forcedVisible.Remove(country);
    }

    /// <summary>
    /// Update label visibility and alpha based on earth scale and country area.
    /// Should be called every frame from CountryClickController.Update().
    /// </summary>
    public void UpdateLabelLOD()
    {
        float scale = earthTransform.localScale.x;

        foreach (var kvp in countryLabels)
        {
            GameObject country = kvp.Key;
            GameObject label = kvp.Value;

            if (!label) continue;

            float area = country.GetComponent<MeshFilter>().sharedMesh.bounds.size.sqrMagnitude;
            area = Mathf.Max(area, 0.0001f);

            // Larger area → smaller threshold → appears earlier
            float K = 0.12f;
            float threshold = K / area;
            float factor = Mathf.Clamp01(scale / threshold);

            float alpha = Mathf.SmoothStep(0f, 1f, factor);

            // If this label is forced visible (selected / hovered), override alpha to 1
            if (forcedVisible.Contains(country))
                alpha = 1f;

            var tmp = label.GetComponent<TextMeshPro>();
            if (tmp)
            {
                Color c = tmp.color;
                c.a = alpha;
                tmp.color = c;
            }

            label.SetActive(alpha > 0.02f);
        }
    }

    /// <summary>
    /// Get a country's label GameObject.
    /// </summary>
    public GameObject GetLabel(GameObject country)
    {
        if (countryLabels.TryGetValue(country, out var label))
            return label;
        return null;
    }

    // ----------- Internal: create a single label for a given country -----------

    private GameObject CreateLabelObject(GameObject country)
    {
        MeshFilter mf = country.GetComponent<MeshFilter>();
        if (!mf || !mf.sharedMesh) return null;

        Mesh mesh = mf.sharedMesh;

        // --- Step 1: 找到最大连通块 ---
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        // 每个三角形组成一个面，建立邻接表
        Dictionary<int, List<int>> adj = new Dictionary<int, List<int>>();
        for (int i = 0; i < vertices.Length; i++)
            adj[i] = new List<int>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int a = triangles[i];
            int b = triangles[i + 1];
            int c = triangles[i + 2];

            adj[a].Add(b); adj[a].Add(c);
            adj[b].Add(a); adj[b].Add(c);
            adj[c].Add(a); adj[c].Add(b);
        }

        // BFS 找连通块
        HashSet<int> visited = new HashSet<int>();
        List<int> bestComponent = null;

        for (int i = 0; i < vertices.Length; i++)
        {
            if (visited.Contains(i)) continue;

            Queue<int> q = new Queue<int>();
            List<int> comp = new List<int>();

            q.Enqueue(i);
            visited.Add(i);

            while (q.Count > 0)
            {
                int v = q.Dequeue();
                comp.Add(v);

                foreach (var nxt in adj[v])
                {
                    if (!visited.Contains(nxt))
                    {
                        visited.Add(nxt);
                        q.Enqueue(nxt);
                    }
                }
            }

            if (bestComponent == null || comp.Count > bestComponent.Count)
                bestComponent = comp;
        }

        // --- Step 2: 用最大连通块的中心作为 label 位置 ---
        Vector3 avg = Vector3.zero;
        foreach (int id in bestComponent)
            avg += country.transform.TransformPoint(vertices[id]);

        avg /= bestComponent.Count;

        Vector3 worldCenter = avg;
        Vector3 normal = (worldCenter - earthTransform.position).normalized;

        float maxDist = float.MinValue;
        foreach (var v in mesh.vertices)
        {
            Vector3 wv = country.transform.TransformPoint(v);
            float d = (wv - earthTransform.position).sqrMagnitude;
            if (d > maxDist) maxDist = d;
        }

        float surfaceRadius = Mathf.Sqrt(maxDist);
        float centerRadius = (worldCenter - earthTransform.position).magnitude;
        float offset = (surfaceRadius - centerRadius) + 0.005f;
        offset = Mathf.Max(offset, 0.005f);

        Vector3 labelPos = worldCenter + normal * offset;
        Quaternion rot = Quaternion.LookRotation(-normal, cameraTransform.up);

        GameObject label = Instantiate(labelPrefab, labelPos, rot, country.transform);
        var tmp = label.GetComponent<TextMeshPro>();
        if (tmp) tmp.text = country.name;

        label.SetActive(false);
        return label;
    }
}
