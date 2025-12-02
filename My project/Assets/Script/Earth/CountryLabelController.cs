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
        if (!mf || !mf.sharedMesh)
            return null;

        Mesh mesh = mf.sharedMesh;

        Vector3 localCenter = mesh.bounds.center;
        Vector3 worldCenter = country.transform.TransformPoint(localCenter);
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
