using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Country interaction controller:
/// - Hover: yellow border + label
/// - Click: red border + highlight + zoom to country + label + audio
/// - Static black border for all countries
/// </summary>
public class CountryClickController : MonoBehaviour
{
    [Header("Country Root")]
    public Transform countriesParent;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public List<CountryAudio> countryAudios = new List<CountryAudio>();

    [System.Serializable]
    public class CountryAudio
    {
        public string countryName;  // Must match GameObject name
        public AudioClip clip;
    }

    [Header("Scene References")]
    public Transform earthTransform;
    public Transform cameraTransform;
    public GameObject labelPrefab;

    [Header("Highlight Settings")]
    public Color highlightColor = Color.red;
    public float borderWidth = 0.02f;
    public float normalPush = 0.001f;
    [Range(0, 1)] public float transparentAlpha = 1.0f;

    [Header("Camera Movement")]
    public float targetDistance = 2.0f;
    public float rotationSpeed = 1.5f;

    [HideInInspector]
    public bool isAnyCountryActive = false;

    // Runtime state
    private GameObject currentCountry;
    private Material originalMat;
    private Coroutine currentAnim;

    private Vector3 baseScale;
    // Store static black borders (always generated once)
    private List<GameObject> highlightedCountries = new List<GameObject>();
    // Store labels for each country (generated on-demand)
    private Dictionary<GameObject, GameObject> countryLabels = new Dictionary<GameObject, GameObject>();
    public static string selectedCountry;


    // ---------- Utility ----------
    private bool IsChinaOrTaiwan(string name)
    {
        return name == "China" || name == "Taiwan";
    }

    void Start()
    {
        baseScale = earthTransform.localScale;

        // Create static borders + register label slots
        foreach (Transform c in countriesParent)
        {
            CreateStaticBorder(c.gameObject);

            if (!countryLabels.ContainsKey(c.gameObject))
                countryLabels[c.gameObject] = null;  // placeholder
        }
    }

    public void FocusCountryWrapper(GameObject country)
    {
        FocusCountry(country);
    }

    void FocusCountry(GameObject country)
    {
        string name = country.name;
        selectedCountry = name;

        if (IsChinaOrTaiwan(name))
        {
            // 把点击统一映射为“China”
            country = transform.Find("China")?.gameObject ?? country;
        }

        if (country == currentCountry) return;

        ClearHighlight();
        currentCountry = country;
        HighlightCountry(country);

        // If selecting China/Taiwan, highlight both
        if (IsChinaOrTaiwan(name))
        {
            var other = countriesParent.Find(name == "China" ? "Taiwan" : "China");
            if (other) HighlightCountry(other.gameObject);
        }

        // Show label for selected country
        ShowLabel(country);

        // Camera rotation + zoom
        var mf = country.GetComponent<MeshFilter>();
        Vector3 localCenter = mf.sharedMesh.bounds.center;
        Vector3 worldCenter = country.transform.TransformPoint(localCenter);

        Vector3 targetDir = (worldCenter - earthTransform.position).normalized;        
        Vector3 camDir = (cameraTransform.position - earthTransform.position).normalized; 

        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(RotateAndZoom(targetDir, camDir, country));

        // Play country audio
        if (audioSource)
        {
            string audioPath = "Audio/" + country.name; 
            AudioClip clip = Resources.Load<AudioClip>(audioPath);

            if (clip)
            {
                // 播放音频
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning($"[CountryAudio] Audio not found：{audioPath}");
            }
        }

        isAnyCountryActive = true;

    }

    // ===============================================================
    // HIGHLIGHT (SELECT)
    // ===============================================================
    public void HighlightCountry(GameObject country)
    {
        var mr = country.GetComponent<MeshRenderer>();
        var mf = country.GetComponent<MeshFilter>();
        if (mr == null || mf == null || mf.sharedMesh == null) return;

        // Store original material
        originalMat = mr.sharedMaterial;

        // Transparent material
        Material transparentMat = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
        transparentMat.color = new Color(1f, 1f, 1f, 1f); 

        if (originalMat != null && originalMat.HasProperty("_MainTex"))
            transparentMat.mainTexture = originalMat.mainTexture;

        Color baseColor = Color.white;
        if (originalMat != null && originalMat.HasProperty("_Color"))
            baseColor = originalMat.color;

        baseColor.a = transparentAlpha;    
        transparentMat.color = baseColor;
        mr.sharedMaterial = transparentMat; 

        // generate border
        HighlightCountryBorder(country);
        if (!highlightedCountries.Contains(country))
            highlightedCountries.Add(country);
    }

    void HighlightCountryBorder(GameObject country)
    {
        string staticName = country.name + "_BorderStatic";
        string hoverName = country.name + "_BorderHover";
        string runtimeName = country.name + "_BorderRuntime";

        // Remove previous runtime border
        var oldR = country.transform.Find(runtimeName);
        if (oldR) Destroy(oldR.gameObject);

        // Create red border
        GameObject border = GenerateBorder(
            country,
            0.05f,      
            0.001f,
            Color.red
        );
        border.name = runtimeName;

        // Hide static & hover borders
        var staticBorder = country.transform.Find(staticName);
        var hoverBorder = country.transform.Find(hoverName);
        if (staticBorder) staticBorder.gameObject.SetActive(false);
        if (hoverBorder) hoverBorder.gameObject.SetActive(false);
    }


    // ===============================================================
    // LABEL GENERATION
    // ===============================================================
    private GameObject ShowLabel(GameObject country)
    {
        MeshFilter mf = country.GetComponent<MeshFilter>();
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

        float dynamicOffset = (surfaceRadius - centerRadius) + 0.005f;
        dynamicOffset = Mathf.Max(dynamicOffset, 0.005f);

        Vector3 labelPos = worldCenter + normal * dynamicOffset;
        Quaternion rot = Quaternion.LookRotation(-normal, cameraTransform.up);

        GameObject label = Instantiate(labelPrefab, labelPos, rot, country.transform);

        var tmp = label.GetComponent<TMPro.TextMeshPro>();
        tmp.text = country.name;

        label.SetActive(false);
        return label;
    }

    public void ClearHighlight()
    {
        foreach (var c in highlightedCountries)
        {
            if (c == null) continue;

            var mr = c.GetComponent<MeshRenderer>();
            if (mr && originalMat)
                mr.sharedMaterial = originalMat;    

            string staticName = c.name + "_BorderStatic";
            string runtimeName = c.name + "_BorderRuntime";

            var runtime = c.transform.Find(runtimeName);
            if (runtime) Destroy(runtime.gameObject);

            var staticBorder = c.transform.Find(staticName);
            if (staticBorder) staticBorder.gameObject.SetActive(true);
        }

        highlightedCountries.Clear();
        currentCountry = null;
        originalMat = null;
        currentAnim = null;
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


    // ===============================================================
    // ROTATE + ZOOM ANIMATION
    // ===============================================================
    IEnumerator RotateAndZoom(Vector3 targetDir, Vector3 camDir, GameObject country)
    {
        // Step 1: Align country direction
        Quaternion alignRot = Quaternion.FromToRotation(targetDir, camDir) * earthTransform.rotation;

        // Step 2: Upright rotation
        Vector3 localNorthAxis = Vector3.up;
        Vector3 worldNorthAfterAlign = alignRot * localNorthAxis;
        Vector3 nAligned = camDir;
        Vector3 countryNorthTangent = Vector3.ProjectOnPlane(worldNorthAfterAlign, nAligned).normalized;
        if (countryNorthTangent.sqrMagnitude < 1e-6f)
            countryNorthTangent = Vector3.Cross(nAligned, cameraTransform.right).normalized;

        float roll = Vector3.SignedAngle(countryNorthTangent, cameraTransform.up, camDir);
        Quaternion uprightRot = Quaternion.AngleAxis(roll, camDir);
        Quaternion targetRot = uprightRot * alignRot;

        // Step 3: Smooth dynamic zoom
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

    float ComputeCountryAngularRadiusDeg(GameObject country)
    {
        var mf = country.GetComponent<MeshFilter>();
        if (!mf || mf.sharedMesh == null) return 10f; 

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


    // ===============================================================
    // BORDER GENERATION
    // ===============================================================
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

        Shader sh = Shader.Find("Unlit/Color");
        if (!sh) sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (!sh) sh = Shader.Find("Legacy Shaders/Transparent/Diffuse");
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

    void CreateStaticBorder(GameObject country)
    {
        GameObject border = GenerateBorder(country, 0.02f, 0.001f, Color.black);
        border.name = country.name + "_BorderStatic";
    }

    // ===============================================================
    // EDGE DATA STRUCTURES
    // ===============================================================
    struct EdgeKey
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
    struct EdgeVal { public int count; }
    static void AddEdge(Dictionary<EdgeKey, EdgeVal> map, int i, int j)
    {
        var key = new EdgeKey(i, j);
        if (map.TryGetValue(key, out var val)) { val.count++; map[key] = val; }
        else map[key] = new EdgeVal { count = 1 };
    }

    // ===============================================================
    // HOVER LOGIC
    // ===============================================================
    public void HoverEnterCountry(GameObject country)
    {
        if (country == currentCountry) return; 

        string staticName = country.name + "_BorderStatic";
        string hoverName = country.name + "_BorderHover";

        var oldHover = country.transform.Find(hoverName);
        if (oldHover) Destroy(oldHover.gameObject);

        var staticBorder = country.transform.Find(staticName);
        if (staticBorder) staticBorder.gameObject.SetActive(false);

        GameObject border = GenerateBorder(
            country,
            0.04f,     
            0.001f,
            Color.yellow
        );
        border.name = hoverName;

        if (countryLabels[country] == null)
        {
            GameObject label = ShowLabel(country);
            countryLabels[country] = label;
        }

        countryLabels[country].SetActive(true);

    }

    public void HoverExitCountry(GameObject country)
    {
        if (country == currentCountry) return;  

        string staticName = country.name + "_BorderStatic";
        string hoverName = country.name + "_BorderHover";

        var hover = country.transform.Find(hoverName);
        if (hover) Destroy(hover.gameObject);

        var staticBorder = country.transform.Find(staticName);
        if (staticBorder) staticBorder.gameObject.SetActive(true);

        if (countryLabels[country] != null)
            countryLabels[country].SetActive(false);

    }

    void CreateLabelObject(GameObject country)
    {
        if (countryLabels.ContainsKey(country))
            return;

        MeshFilter mf = country.GetComponent<MeshFilter>();
        if (!mf) return;
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

        float dynamicOffset = (surfaceRadius - centerRadius) + 0.005f;
        dynamicOffset = Mathf.Max(dynamicOffset, 0.005f);

        Vector3 labelPos = worldCenter + normal * dynamicOffset;
        Quaternion rotation = Quaternion.LookRotation(-normal, cameraTransform.up);

        GameObject newLabel = Instantiate(labelPrefab, labelPos, rotation, country.transform);
        newLabel.SetActive(false); 

        var tmp = newLabel.GetComponent<TextMeshPro>();
        if (tmp) tmp.text = country.name;

        countryLabels[country] = newLabel;
    }

    void UpdateLabelLOD()
    {
        float scale = earthTransform.localScale.x;

        foreach (var kvp in countryLabels)
        {
            GameObject country = kvp.Key;
            GameObject label = kvp.Value;

            float area = country.GetComponent<MeshFilter>().sharedMesh.bounds.size.sqrMagnitude;

            bool shouldShow = false;

            if (scale <= 0.05f)
            {
                shouldShow = area > 0.8f;
            }
            else if (scale < 0.08f)
            {
                shouldShow = area > 0.6f;
            }
            else if (scale < 0.1f)
            {
                shouldShow = area > 0.4f;
            }
            else if (scale < 0.15f)
            {
                shouldShow = area > 0.2f;
            }
            else
            {
                shouldShow = true;
            }

            label.SetActive(shouldShow);
        }
    }


}
