using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonOccludingFollow : MonoBehaviour
{
    [Header("Placement")]
    public float baseDistance = 0.12f;   // 菜单离表面的基础距离 base distance between the menu and the surface
    public float sideOffset = 0.08f;    // 相对相机右侧偏移 
    public float upOffset = 0.02f;    // 向上偏移
    public LayerMask occlusionMask;      // 必须包含地球/国家的碰撞层

    [Header("Facing")]
    public bool yawOnly = true;        // 只绕Y轴朝向相机
    public bool flipForward = false;    // 某些Canvas需要反转180度
    public float posLerp = 14f;
    public float rotLerp = 14f;

    [SerializeField] private Transform cameraTransform;

    // 由 PlaceAt 写入的锚点与法线
    private Vector3 anchorPointWS;
    private Vector3 anchorNormalWS = Vector3.up;

    private Vector3 desiredPos;
    private Quaternion desiredRot;

    public void SetCamera(Transform cam) => cameraTransform = cam;

    private void Awake() => TryFindCameraIfNull();

    private void TryFindCameraIfNull()
    {
        if (cameraTransform) return;
        if (Camera.main) { cameraTransform = Camera.main.transform; return; }
        var anyCam = FindObjectOfType<Camera>();
        if (anyCam) cameraTransform = anyCam.transform;
    }

    /// <summary>
    /// 在点击命中的位置附近放置菜单；hitNormal 必须是命中处的世界法线（用 RaycastHit.normal）
    /// </summary>
    public void PlaceAt(Vector3 hitPoint, Vector3 hitNormal)
    {
        TryFindCameraIfNull();
        if (!cameraTransform) return;

        anchorPointWS = hitPoint;
        anchorNormalWS = hitNormal.normalized;

        // 立即更新一次，避免第一帧还没插值到位
        RecomputeDesiredTR();
        transform.position = desiredPos;
        transform.rotation = desiredRot;
    }

    private void LateUpdate()
    {
        if (!cameraTransform) return;

        // 每帧重算（相机/球体相对关系在动）
        RecomputeDesiredTR();

        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * posLerp);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * rotLerp);
    }

    private void RecomputeDesiredTR()
    {
        // 1) 依据锚点与法线先给一个“理想位置”（加上相机相关的左右/向上偏移）
        Vector3 pos = anchorPointWS
                    + anchorNormalWS * baseDistance
                    + cameraTransform.right * sideOffset
                    + Vector3.up * upOffset;

        // 2) 遮挡修正：如果从相机到该点被地球等遮住，就把菜单吸到命中点外侧
        Vector3 toPos = pos - cameraTransform.position;
        float dist = toPos.magnitude;
        if (dist > 1e-3f)
        {
            Vector3 dir = toPos / dist;
            if (Physics.Raycast(cameraTransform.position, dir, out var rh, dist, occlusionMask, QueryTriggerInteraction.Ignore))
            {
                // 使用“命中处的法线”更稳
                pos = rh.point + rh.normal.normalized * Mathf.Max(0.02f, baseDistance * 0.4f);
            }
        }

        desiredPos = pos;

        // 3) 朝向：让 +Z 指向相机（World Space Canvas 的正面是 +Z）
        Vector3 toCam = (cameraTransform.position - desiredPos);
        if (yawOnly) toCam.y = 0f;
        if (toCam.sqrMagnitude < 1e-6f) toCam = Vector3.forward;

        desiredRot = Quaternion.LookRotation(toCam.normalized, Vector3.up);

        // 需要时反转180°（某些Prefab/Canvas朝向做过180°）
        if (flipForward) desiredRot *= Quaternion.Euler(0f, 180f, 0f);
    }
}
