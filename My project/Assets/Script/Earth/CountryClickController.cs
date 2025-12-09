using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// High-level controller:
/// - Click: focus country (zoom + rotate), red border, audio, label pinned
/// - Hover: yellow border + label (via LabelController + BorderController)
/// </summary>
public class CountryClickController : MonoBehaviour
{
    [Header("Country Root")]
    public Transform countriesParent;

    [Header("Scene References")]
    public Transform earthTransform;
    public Transform cameraTransform;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Sub Controllers")]
    public CountryBorderController borderController;
    public CountryLabelController labelController;

    [Header("Highlight Settings")]
    [Range(0, 1)] public float transparentAlpha = 1.0f;

    [Header("Camera Movement")]
    public float rotationSpeed = 1.5f;

    [HideInInspector] public bool isAnyCountryActive = false;

    private GameObject currentCountry;
    private Material originalMat;
    private Coroutine currentAnim;
    private Vector3 baseScale;
    private List<GameObject> highlightedCountries = new List<GameObject>();

    public static string selectedCountry;

    // -------- Utility --------
    private bool IsChinaOrTaiwan(string name)
    {
        return name == "China" || name == "Taiwan";
    }

    private void Start()
    {
        baseScale = earthTransform.localScale;

        // Initialize borders and labels once
        borderController.CreateStaticBorders();
        labelController.InitLabels();
    }

    private void Update()
    {
        // Update label LOD every frame
        labelController.UpdateLabelLOD();
    }

    // =========================================================
    // Public entry point from RayTapCountrySelector
    // =========================================================
    public void FocusCountryByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        name = name.Trim().ToLower();

        Transform found = null;

        foreach (Transform c in countriesParent)
        {
            if (c.name.ToLower() == name)
            {
                found = c;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning($"[CountryClick] Country not found: {name}");
            return;
        }

        FocusCountry(found.gameObject);
    }


    //public void FocusCountryWrapper(GameObject country)
    //{
    //    FocusCountry(country);
    //}

    // =========================================================
    // Focus / select a country
    // =========================================================
    public void FocusCountry(GameObject country)
    {
        string name = country.name;
        selectedCountry = name;

        // Merge Taiwan into China
        if (IsChinaOrTaiwan(name))
        {
            var chinaObj = transform.Find("China");
            if (chinaObj) country = chinaObj.gameObject;
        }

        if (country == currentCountry) return;

        ClearHighlight();

        currentCountry = country;
        HighlightCountry(country);

        // If selecting China/Taiwan, also highlight the other
        if (IsChinaOrTaiwan(name))
        {
            var other = countriesParent.Find(name == "China" ? "Taiwan" : "China");
            if (other) HighlightCountry(other.gameObject);
        }

        // Pin label for selected country (always visible)
        labelController.SetForcedVisible(country, true);

        // Camera center + rotate + zoom
        var mf = country.GetComponent<MeshFilter>();
        Vector3 localCenter = mf.sharedMesh.bounds.center;
        Vector3 worldCenter = country.transform.TransformPoint(localCenter);

        Vector3 targetDir = (worldCenter - earthTransform.position).normalized;
        Vector3 camDir = (cameraTransform.position - earthTransform.position).normalized;

        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(RotateAndZoom(targetDir, camDir, country));

        // Play audio
        PlayCountryAudio(country);

        isAnyCountryActive = true;
    }

    private void PlayCountryAudio(GameObject country)
    {
        if (!audioSource) return;

        string audioPath = "Audio/" + country.name;
        AudioClip clip = Resources.Load<AudioClip>(audioPath);

        if (clip)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"[CountryAudio] Audio not found: {audioPath}");
        }
    }

    // =========================================================
    // Highlight country: transparent fill + red border
    // =========================================================
    private void HighlightCountry(GameObject country)
    {
        var mr = country.GetComponent<MeshRenderer>();
        var mf = country.GetComponent<MeshFilter>();
        if (!mr || !mf || !mf.sharedMesh) return;

        originalMat = mr.sharedMaterial;

        Material transparentMat = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
        Color baseColor = Color.white;
        if (originalMat && originalMat.HasProperty("_Color"))
            baseColor = originalMat.color;

        baseColor.a = transparentAlpha;
        transparentMat.color = baseColor;

        if (originalMat && originalMat.HasProperty("_MainTex"))
            transparentMat.mainTexture = originalMat.mainTexture;

        mr.sharedMaterial = transparentMat;

        borderController.ShowSelectedBorder(country);

        if (!highlightedCountries.Contains(country))
            highlightedCountries.Add(country);
    }

    // =========================================================
    // Clear selection highlight
    // =========================================================
    public void ClearHighlight()
    {
        foreach (var c in highlightedCountries)
        {
            if (!c) continue;

            // Restore material
            var mr = c.GetComponent<MeshRenderer>();
            if (mr && originalMat) mr.sharedMaterial = originalMat;

            // Restore border
            borderController.ClearSelectedBorder(c);

            // Unpin label
            labelController.SetForcedVisible(c, false);
        }

        highlightedCountries.Clear();
        currentCountry = null;
        originalMat = null;
        currentAnim = null;
    }

    // =========================================================
    // Camera rotate + zoom
    // =========================================================
    private IEnumerator RotateAndZoom(Vector3 targetDir, Vector3 camDir, GameObject country)
    {
        Quaternion alignRot = Quaternion.FromToRotation(targetDir, camDir) * earthTransform.rotation;

        Vector3 localNorthAxis = Vector3.up;
        Vector3 worldNorthAfterAlign = alignRot * localNorthAxis;
        Vector3 nAligned = camDir;
        Vector3 countryNorthTangent = Vector3.ProjectOnPlane(worldNorthAfterAlign, nAligned).normalized;
        if (countryNorthTangent.sqrMagnitude < 1e-6f)
            countryNorthTangent = Vector3.Cross(nAligned, cameraTransform.right).normalized;

        float roll = Vector3.SignedAngle(countryNorthTangent, cameraTransform.up, camDir);
        Quaternion uprightRot = Quaternion.AngleAxis(roll, camDir);
        Quaternion targetRot = uprightRot * alignRot;

        float angleDeg = ComputeCountryAngularRadiusDeg(country);

        float a = 1.1f;
        float b = 0.035f;
        float c = 1.2f;
        float d = 0.95f;
        float scaleFactor = 1.2f * (a / (1 + b * Mathf.Pow(angleDeg, c)) + d);

        Vector3 startScale = earthTransform.localScale;
        Vector3 targetScale = baseScale * scaleFactor;

        Quaternion startRot = earthTransform.rotation;
        float t = 0f;
        float duration = 1.2f / rotationSpeed;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = Mathf.SmoothStep(0, 1, t);

            earthTransform.rotation = Quaternion.Slerp(startRot, targetRot, k);
            earthTransform.localScale = Vector3.Lerp(startScale, targetScale, k);

            yield return null;
        }
    }

    private float ComputeCountryAngularRadiusDeg(GameObject country)
    {
        var mf = country.GetComponent<MeshFilter>();
        if (!mf || !mf.sharedMesh) return 10f;

        Vector3 worldCenter = country.transform.TransformPoint(mf.sharedMesh.bounds.center);
        Vector3 centerDir = (worldCenter - earthTransform.position).normalized;

        var verts = mf.sharedMesh.vertices;
        float maxAngle = 0f;

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 wv = country.transform.TransformPoint(verts[i]);
            Vector3 vDir = (wv - earthTransform.position).normalized;
            float dot = Mathf.Clamp(Vector3.Dot(centerDir, vDir), -1f, 1f);
            float ang = Mathf.Acos(dot);
            if (ang > maxAngle) maxAngle = ang;
        }

        return maxAngle * Mathf.Rad2Deg;
    }

    // =========================================================
    // Hover API (called from RayTapCountrySelector)
    // =========================================================
    public void HoverEnterCountry(GameObject country)
    {
        if (country == currentCountry) return;

        borderController.ShowHoverBorder(country);

        // Temporarily force label visible while hovered
        labelController.SetForcedVisible(country, true);
    }

    public void HoverExitCountry(GameObject country)
    {
        if (country == currentCountry) return;

        borderController.ClearHoverBorder(country);

        // Remove hover pin; let LOD handle visibility
        labelController.SetForcedVisible(country, false);
    }

    public void SetCurrentCountryAlpha(float alpha)
    {
        // 没有当前国家就什么也不做
        if (!currentCountry) return;

        var mr = currentCountry.GetComponent<MeshRenderer>();
        if (!mr) return;

        var mat = mr.sharedMaterial;
        if (!mat) return;

        if (mat.HasProperty("_Color"))
        {
            Color c = mat.color;
            c.a = Mathf.Clamp01(alpha);
            mat.color = c;
        }
    }
}
