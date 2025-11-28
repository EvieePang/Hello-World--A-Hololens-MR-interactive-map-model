using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;        // 必需命名空间


/// 点击国家 → 居中放大 + 红色边界 + 半透明 + 标签（整合 CountryHighlighter 全逻辑）
public class CountryClickController : MonoBehaviour
{
    //[Header("Country Pull-out")]
    //public CountryPulloutController pulloutController;

    [Header("Country Parent")]
    public Transform countriesParent;

    [Header("Audio Settings")]
    public AudioSource audioSource;   // 播放器组件（统一播放）
    [Tooltip("配置国家与音频的映射表")]
    public List<CountryAudio> countryAudios = new List<CountryAudio>();

    [System.Serializable]
    public class CountryAudio
    {
        public string countryName;   // 国家名（必须和 GameObject 名字一致）
        public AudioClip clip;       // 对应的音频
    }

    [Header("Scene References")]
    public Transform earthTransform;            // 拖 Earth
    public Transform cameraTransform;           // 拖 Main Camera
    public GameObject labelPrefab;              // 拖 TextMeshPro 3D 预制体

    [Header("Highlight (from CountryHighlighter)")]
    public Color highlightColor = Color.red;
    public float borderWidth = 0.02f;    // 描边粗细（CountryHighlighter 的 width）
    public float normalPush = 0.001f;   // 防 ZFighting 的外推距离
    [Range(0, 1)] public float transparentAlpha = 1.0f;     // 国家内部透明度

    [Header("Focus / Camera")]
    public float targetDistance = 2.0f;         // 相机到地心的距离（地球半径=1时建议 1.6~2.5）
    public float rotationSpeed = 1.5f;         // 旋转速度（越大越快）

    //[Header("UI Controller")]
    //public UIController uiController;

    [HideInInspector]
    public bool isAnyCountryActive = false;

    // 运行时引用
    private GameObject currentLabel;
    private GameObject currentBorder;
    private GameObject currentCountry;

    // 记录并恢复原材质
    private Material originalMat;
    private Coroutine currentAnim;

    private Vector3 baseScale;
    // 所有当前高亮的国家
    private List<GameObject> highlightedCountries = new List<GameObject>();
    // 保存所有国家的标签（避免重复生成）
    private Dictionary<GameObject, GameObject> countryLabels = new Dictionary<GameObject, GameObject>();
    public static string selectedCountry;


    // 国家合并（别名映射）
    bool IsChinaOrTaiwan(string name)
    {
        return name == "China" || name == "Taiwan";
    }



    void Start()
    {
        // 记录地球初始缩放，用于防止多次点击累积放大
        baseScale = earthTransform.localScale;

        // === 初始化所有国家的默认黑色边界 ===
        foreach (Transform c in countriesParent)
        {
            CreateStaticBorder(c.gameObject);
            CreateLabelObject(c.gameObject);
        }
    }

    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    if (Physics.Raycast(ray, out RaycastHit hit))
        //    {
        //        if (hit.collider && hit.collider.GetComponent<MeshFilter>())
        //            FocusCountry(hit.collider.gameObject);
        //    }
        //    else
        //    {
        //        // 点空白：清理并可选复位
        //        ClearHighlight();
        //    }
        //}
        UpdateLabelLOD();
    }

    public void FocusCountryWrapper(GameObject country)
    {
        FocusCountry(country);
    }

    // 主入口：点击国家
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

        // 清理旧的
        ClearHighlight();

        currentCountry = country;

        // 半透明 + 红色边界（完整 CountryHighlighter 逻辑）
        HighlightCountry(country);

        // 如果点击的是 China 或 Taiwan，额外高亮另一个
        if (IsChinaOrTaiwan(name))
        {
            var other = countriesParent.Find(name == "China" ? "Taiwan" : "China");
            if (other) HighlightCountry(other.gameObject);
        }

        // 标签
        //ShowLabel(country);
        //pulloutController.PullOutCountry(country);

        // 居中 + 摆正（国家真北对齐相机北）+ 放大
        var mf = country.GetComponent<MeshFilter>();
        Vector3 localCenter = mf.sharedMesh.bounds.center;
        Vector3 worldCenter = country.transform.TransformPoint(localCenter);

        Vector3 targetDir = (worldCenter - earthTransform.position).normalized;         // 国家方向（地心→国家）
        Vector3 camDir = (cameraTransform.position - earthTransform.position).normalized; // 地心→相机

        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(RotateAndZoom(targetDir, camDir, country));

        // === 播放对应国家音频（从 Resources/Audio/ 动态加载） ===
        if (audioSource)
        {
            string audioPath = "Audio/" + country.name;  // 对应 Resources/Audio/<CountryName>
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
                Debug.LogWarning($"[CountryAudio] 没找到音频文件：{audioPath}");
            }
        }

        isAnyCountryActive = true;

    }

    // 半透明 + 红色边界（完整移植自 CountryHighlighter）
    public void HighlightCountry(GameObject country)
    {
        var mr = country.GetComponent<MeshRenderer>();
        var mf = country.GetComponent<MeshFilter>();
        if (mr == null || mf == null || mf.sharedMesh == null) return;

        // 记住原材质
        originalMat = mr.sharedMaterial;

        // —— 创建支持透明的副本材质（兼容 Mobile/Unlit）——
        Material transparentMat = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
        transparentMat.color = new Color(1f, 1f, 1f, 1f); // 初始白

        if (originalMat != null && originalMat.HasProperty("_MainTex"))
            transparentMat.mainTexture = originalMat.mainTexture;

        Color baseColor = Color.white;
        if (originalMat != null && originalMat.HasProperty("_Color"))
            baseColor = originalMat.color;

        baseColor.a = transparentAlpha;     // 设置透明度
        transparentMat.color = baseColor;
        mr.sharedMaterial = transparentMat; // 应用半透明

        // —— 生成红色边界（边界网格法）——
        HighlightCountryBorder(country);
        if (!highlightedCountries.Contains(country))
            highlightedCountries.Add(country);
    }

    void HighlightCountryBorder(GameObject country)
    {
        string staticName = country.name + "_BorderStatic";
        string runtimeName = country.name + "_BorderRuntime";

        // 删除旧的 runtime（确保不会重复）
        var oldR = country.transform.Find(runtimeName);
        if (oldR) Destroy(oldR.gameObject);

        // 生成新的粗红边界
        GameObject border = GenerateBorder(
            country,
            0.04f,       // 粗边界（可调）
            0.001f,      // 推开一点点，避免重叠
            Color.red
        );
        border.name = runtimeName;

        // 隐藏 static（否则会透出来）
        var staticBorder = country.transform.Find(staticName);
        if (staticBorder) staticBorder.gameObject.SetActive(false);
    }


    // 标签：放在包围盒中心外沿，避免遮挡
    //void ShowLabel(GameObject country)
    //{
    //    if (countryLabels.ContainsKey(country))
    //        return;  // 已生成，避免重复

    //    MeshFilter mf = country.GetComponent<MeshFilter>();
    //    if (!mf) return;
    //    Mesh mesh = mf.sharedMesh;

    //    Vector3 localCenter = mesh.bounds.center;
    //    Vector3 worldCenter = country.transform.TransformPoint(localCenter);
    //    Vector3 normal = (worldCenter - earthTransform.position).normalized;

    //    float maxDist = float.MinValue;
    //    foreach (var v in mesh.vertices)
    //    {
    //        Vector3 wv = country.transform.TransformPoint(v);
    //        float d = (wv - earthTransform.position).sqrMagnitude;
    //        if (d > maxDist) maxDist = d;
    //    }

    //    float surfaceRadius = Mathf.Sqrt(maxDist);
    //    float centerRadius = (worldCenter - earthTransform.position).magnitude;

    //    float dynamicOffset = (surfaceRadius - centerRadius) + 0.005f;
    //    dynamicOffset = Mathf.Max(dynamicOffset, 0.005f);

    //    Vector3 labelPos = worldCenter + normal * dynamicOffset;
    //    Quaternion rotation = Quaternion.LookRotation(-normal, cameraTransform.up);

    //    GameObject newLabel = Instantiate(labelPrefab, labelPos, rotation, country.transform);
    //    var tmp = newLabel.GetComponent<TextMeshPro>();
    //    if (tmp) tmp.text = country.name;

    //    // ★ 保存该 label，防止丢失
    //    countryLabels[country] = newLabel;
    //}


    // 恢复上一个国家
    public void ClearHighlight()
    {
        // 清除所有国家（包括 China + Taiwan）的高亮
        foreach (var c in highlightedCountries)
        {
            if (c == null) continue;

            var mr = c.GetComponent<MeshRenderer>();
            if (mr && originalMat)
                mr.sharedMaterial = originalMat;    // 恢复原来的材质

            string staticName = c.name + "_BorderStatic";
            string runtimeName = c.name + "_BorderRuntime";

            var runtime = c.transform.Find(runtimeName);
            if (runtime) Destroy(runtime.gameObject);

            var staticBorder = c.transform.Find(staticName);
            if (staticBorder) staticBorder.gameObject.SetActive(true);
        }

        highlightedCountries.Clear();

        // 清除标签 & 动画
        if (currentLabel) Destroy(currentLabel);
        //if (currentBorder) Destroy(currentBorder);

        //if (currentCountry)
        //{
        //    var mr = currentCountry.GetComponent<MeshRenderer>();
        //    if (mr && originalMat) mr.sharedMaterial = originalMat;
        //}

        currentCountry = null;
        originalMat = null;
        currentAnim = null;
    }

    // 设置国家透明度
    // 供外部调用：把当前选中的国家改成指定透明度（0~1）
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


    // ======= 旋转 + 放大（两步：居中 → 真北对齐相机北）=======
    IEnumerator RotateAndZoom(Vector3 targetDir, Vector3 camDir, GameObject country)
    {
        // === Step 1: 居中国家 ===
        Quaternion alignRot = Quaternion.FromToRotation(targetDir, camDir) * earthTransform.rotation;

        // === Step 2: 计算“真北”旋转（国家朝上）===
        Vector3 localNorthAxis = Vector3.up;
        Vector3 worldNorthAfterAlign = alignRot * localNorthAxis;
        Vector3 nAligned = camDir;
        Vector3 countryNorthTangent = Vector3.ProjectOnPlane(worldNorthAfterAlign, nAligned).normalized;
        if (countryNorthTangent.sqrMagnitude < 1e-6f)
            countryNorthTangent = Vector3.Cross(nAligned, cameraTransform.right).normalized;

        float roll = Vector3.SignedAngle(countryNorthTangent, cameraTransform.up, camDir);
        Quaternion uprightRot = Quaternion.AngleAxis(roll, camDir);
        Quaternion targetRot = uprightRot * alignRot;

        // === Step 3: 动态放大倍数（绝对缩放版） ===
        float angleDeg = ComputeCountryAngularRadiusDeg(country);  // 计算国家的球面角半径（单位：度）
        float scaleFactor;

        // 根据角度分段控制放大倍数（整体再调小一档）
        if (angleDeg <= 2f) scaleFactor = 1.9f;  // 极小国家（瑞士）
        else if (angleDeg <= 5f) scaleFactor = Mathf.Lerp(1.9f, 1.7f, (angleDeg - 2f) / 3f);
        else if (angleDeg <= 10f) scaleFactor = Mathf.Lerp(1.7f, 1.5f, (angleDeg - 5f) / 5f);
        else if (angleDeg <= 18f) scaleFactor = Mathf.Lerp(1.5f, 1.35f, (angleDeg - 10f) / 8f);
        else if (angleDeg <= 28f) scaleFactor = Mathf.Lerp(1.35f, 1.2f, (angleDeg - 18f) / 10f);
        else if (angleDeg <= 40f) scaleFactor = Mathf.Lerp(1.2f, 1.08f, (angleDeg - 28f) / 12f);
        else scaleFactor = 1.03f;  // 特大国家（俄罗斯）



        // 基于 baseScale 放大（防止累计）
        Vector3 startScale = earthTransform.localScale;
        Vector3 targetScale = baseScale * scaleFactor;


        // === Step 4: 动画参数 ===
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
        if (!mf || mf.sharedMesh == null) return 10f; // 默认

        Vector3 worldCenter = country.transform.TransformPoint(mf.sharedMesh.bounds.center);
        Vector3 centerDir = (worldCenter - earthTransform.position).normalized;

        var verts = mf.sharedMesh.vertices;
        float maxAngle = 0f;

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 wv = country.transform.TransformPoint(verts[i]);
            Vector3 vDir = (wv - earthTransform.position).normalized;
            float dot = Mathf.Clamp(Vector3.Dot(centerDir, vDir), -1f, 1f);
            float ang = Mathf.Acos(dot); // 弧度
            if (ang > maxAngle) maxAngle = ang;
        }

        return maxAngle * Mathf.Rad2Deg; // 转成度
    }


    // ======= ↓↓↓ 完整 CountryHighlighter 的边界生成逻辑 ↓↓↓ =======
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

            // 反向索引：确保外侧为正面（你之前验证过颜色在反面的问题）
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

        // 红色材质（URP/Builtin 兼容）
        Shader sh = Shader.Find("Unlit/Color");
        if (!sh) sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (!sh) sh = Shader.Find("Legacy Shaders/Transparent/Diffuse");
        var mat = new Material(sh);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

        // 透明队列 + 双面 + 不写深度，确保叠放在上面可见
        mat.renderQueue = 3100;
        if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0);
        if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
        cmr.sharedMaterial = mat;

        cmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        cmr.receiveShadows = false;

        return borderObj;
    }

    // 为每个国家生成一次黑色边界（Static）
    void CreateStaticBorder(GameObject country)
    {
        // 使用你原来的 GenerateBorder 生成一次边界
        GameObject border = GenerateBorder(country, 0.02f, 0.001f, Color.black);
        border.name = country.name + "_BorderStatic";
    }


    // CountryHighlighter 用到的辅助结构
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
    public void HoverEnterCountry(GameObject country)
    {
        if (country == currentCountry) return;  // 已经选中时不改变

        string staticName = country.name + "_BorderStatic";
        string hoverName = country.name + "_BorderHover";

        // 删除旧 Hover
        var oldHover = country.transform.Find(hoverName);
        if (oldHover) Destroy(oldHover.gameObject);

        // 隐藏 static
        var staticBorder = country.transform.Find(staticName);
        if (staticBorder) staticBorder.gameObject.SetActive(false);

        // 生成 Hover 边界（黄色 + 适中粗细）
        GameObject border = GenerateBorder(
            country,
            0.02f,      // 比 default 粗，比 select 细
            0.001f,
            Color.red
        );
        border.name = hoverName;
    }

    public void HoverExitCountry(GameObject country)
    {
        if (country == currentCountry) return;  // 当前选中的国家不清除

        string staticName = country.name + "_BorderStatic";
        string hoverName = country.name + "_BorderHover";

        var hover = country.transform.Find(hoverName);
        if (hover) Destroy(hover.gameObject);

        var staticBorder = country.transform.Find(staticName);
        if (staticBorder) staticBorder.gameObject.SetActive(true);
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
        newLabel.SetActive(false); // ★ 初始隐藏

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

            // 计算国家面积（用 bounds）
            float area = country.GetComponent<MeshFilter>().sharedMesh.bounds.size.sqrMagnitude;

            bool shouldShow = false;

            if (scale < 1.1f)
            {
                // ★ LOD0: 只显示特别大国家
                shouldShow = area > 0.4f;
            }
            else if (scale < 1.4f)
            {
                // ★ LOD1: 显示中等 + 大国家
                shouldShow = area > 0.15f;
            }
            else
            {
                // ★ LOD2: 显示全部国家
                shouldShow = true;
            }

            label.SetActive(shouldShow);
        }
    }


}
