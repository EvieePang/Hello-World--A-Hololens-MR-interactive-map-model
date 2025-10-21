using UnityEngine;
using System.Collections;

public class CountryFocus : MonoBehaviour
{
    public Transform earthTransform;        // 拖入 Earth
    public Transform cameraTransform;       // 主相机
    public float rotationSpeed = 1.0f;      // 旋转速度
    public float zoomSpeed = 2.0f;          // 缩放速度
    public float targetDistance = 3.0f;     // 放大后地球与相机距离

    private Coroutine currentAnim;

    // 外部调用：让地球旋转到目标国家
    public void FocusOnCountry(Transform country)
    {
        var mf = country.GetComponent<MeshFilter>();
        if (mf == null) return;

        Vector3 localCenter = mf.sharedMesh.bounds.center;
        Vector3 worldCenter = country.TransformPoint(localCenter);

        // 国家方向
        Vector3 targetDir = (worldCenter - earthTransform.position).normalized;
        // 摄像机方向
        Vector3 camDir = (cameraTransform.position - earthTransform.position).normalized;

        if (currentAnim != null)
            StopCoroutine(currentAnim);

        currentAnim = StartCoroutine(RotateAndZoom(targetDir, camDir));
    }


    IEnumerator RotateAndZoom(Vector3 targetDir, Vector3 camDir)
    {
        // 1) 先让国家居中：国家法线(=targetDir) -> 相机前向(=camDir)
        Quaternion alignRot = Quaternion.FromToRotation(targetDir, camDir) * earthTransform.rotation;

        // 2) 再把“国家真北”对齐相机的 up
        // localNorthAxis 是地球模型的北极轴（一般是 Vector3.up）；可做成 public 以适配你的模型
        Vector3 localNorthAxis = Vector3.up;

        // 对齐后的“地球北极方向”（世界空间）
        Vector3 worldNorthAfterAlign = alignRot * localNorthAxis;

        // 对齐后国家所在点的法线应为 camDir（因为已经把国家法线对齐到相机前方了）
        Vector3 nAligned = camDir;

        // 计算“国家真北切向” = 把地球北极方向投影到该点的切平面上
        // 也就是：从 worldNorth 去掉在法线方向上的分量
        Vector3 countryNorthTangent =
            Vector3.ProjectOnPlane(worldNorthAfterAlign, nAligned).normalized;

        // 极点附近可能几乎为零，做个兜底（用相机右手系近似）
        if (countryNorthTangent.sqrMagnitude < 1e-6f)
        {
            countryNorthTangent = Vector3.Cross(nAligned, cameraTransform.right).normalized;
        }

        // 目标是让“国家真北切向”与相机的 up 重合 —— 绕相机前向旋转
        float roll = Vector3.SignedAngle(countryNorthTangent, cameraTransform.up, camDir);
        Quaternion uprightRot = Quaternion.AngleAxis(roll, camDir);

        // 最终目标旋转 = 先居中，再绕前向调正北
        Quaternion targetRot = uprightRot * alignRot;

        // ---- 平滑动画（可保持原来的缩放逻辑）----
        Quaternion startRot = earthTransform.rotation;
        float t = 0f, duration = 1.5f / rotationSpeed;

        Vector3 startPos = cameraTransform.position;
        float startDist = Vector3.Distance(startPos, earthTransform.position);
        float targetDist = targetDistance;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = Mathf.SmoothStep(0f, 1f, t);

            earthTransform.rotation = Quaternion.Slerp(startRot, targetRot, k);

            // 缩放时保持居中：沿着 camDir 前后移动
            float dist = Mathf.Lerp(startDist, targetDist, k);
            cameraTransform.position = earthTransform.position - camDir * dist;

            yield return null;
        }
    }




    Vector3 GetMeshCenter(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        if (vertices == null || vertices.Length == 0) return Vector3.zero;

        float maxDist = 0f;
        Vector3 farthest = vertices[0];
        for (int i = 1; i < vertices.Length; i++)
        {
            float dist = vertices[i].sqrMagnitude;
            if (dist > maxDist)
            {
                maxDist = dist;
                farthest = vertices[i];
            }
        }
        return farthest;
    }
}
