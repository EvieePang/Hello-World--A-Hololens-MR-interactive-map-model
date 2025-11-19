using UnityEngine;
using TMPro;

public class CountryClickLabel : MonoBehaviour
{
    public GameObject labelPrefab;   // 拖入 CountryLabel3D.prefab
    public float surfaceOffset = 0.1f;
    private GameObject currentLabel;
    public Transform earthTransform;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Transform country = hit.transform;

                // ① 显示标签
                ShowCountryLabel(country);

                // ② 旋转地球聚焦
                CountryFocus focus = FindObjectOfType<CountryFocus>();
                if (focus != null)
                    focus.FocusOnCountry(country);
            }
        }
    }


    void ShowCountryLabel(Transform country)
    {
        // 清除旧标签
        if (currentLabel != null)
        {
            Destroy(currentLabel);
            currentLabel = null;
        }

        // 获取 Mesh 中心点
        MeshFilter mf = country.GetComponent<MeshFilter>();
        if (mf == null) return;

        // ================= 改进后的标签位置计算 =================
        Vector3 localCenter = GetMeshCentroid(mf.sharedMesh);
        Vector3 worldCenter = country.TransformPoint(localCenter);

        // 从地球中心到该点的法线方向
        Vector3 normal = (worldCenter - earthTransform.position).normalized;

        // 🧩 计算地球半径和国家中心半径的差值
        float earthRadius = earthTransform.lossyScale.x * 1.0f; // 如果你的 Earth 半径为 1，就写成 1f
        float centerRadius = (worldCenter - earthTransform.position).magnitude;
        float dynamicOffset = Mathf.Max(0.05f, (earthRadius - centerRadius) + surfaceOffset);

        // 最终标签位置
        Vector3 labelPos = worldCenter + normal * dynamicOffset;
        Quaternion rotation = Quaternion.LookRotation(-normal, Vector3.up);

        // 实例化标签
        currentLabel = Instantiate(labelPrefab, labelPos, rotation, country);
        TextMeshPro tmp = currentLabel.GetComponent<TextMeshPro>();
        tmp.text = country.name;

    }


    Vector3 GetMeshCentroid(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3 sum = Vector3.zero;
        foreach (var v in vertices) sum += v;
        return sum / vertices.Length;
    }

}
