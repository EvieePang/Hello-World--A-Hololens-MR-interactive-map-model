using UnityEngine;
using MixedReality.Toolkit.UX;
using System.Diagnostics;

public class EarthRotationMRTKSlider : MonoBehaviour
{
    [Header("References")]
    public Transform earthTransform;        // 拖 Earth
    public Transform cameraTransform;       // 拖 Main Camera
    public Slider slider;                   // 拖 NonCanvasSliderBase

    [Header("Position Settings")]
    public float earthRadius = 1.0f;        // 地球半径
    public float surfaceOffset = 0.15f;     // 地表偏移
    public float verticalOffset = -0.2f;    // 向下偏移
    public float tiltAngle = 15f;           // 初始倾斜角

    [Header("Rotation Control")]
    public float rotationRange = 360f;      // 最大旋转角度
    public bool clockwise = true;           // 是否顺时针

    // 缓存初始旋转（保持面板稳定）
    private Quaternion initialRotation;

    void Start()
    {

        // 注册滑动事件
        slider.OnValueUpdated.AddListener(OnSliderValueChanged);

        // 记录初始面板旋转方向
        initialRotation = transform.rotation;
    }

    void OnDestroy()
    {
        if (slider)
            slider.OnValueUpdated.RemoveListener(OnSliderValueChanged);
    }

    void Update()
    {
        if (!earthTransform || !cameraTransform) return;

        // 1️⃣ 相机到地球中心方向
        Vector3 dirToCamera = (cameraTransform.position - earthTransform.position).normalized;

        // 2️⃣ 投影到地球水平面（XZ平面），得到仅绕Y轴的方向
        Vector3 flatDir = Vector3.ProjectOnPlane(dirToCamera, Vector3.up).normalized;

        // 3️⃣ 设置滑动条的位置（沿赤道方向 + 偏移）
        Vector3 targetPos = earthTransform.position
                            + flatDir * (earthRadius + surfaceOffset)
                            - Vector3.up * verticalOffset;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);

        // 4️⃣ 让滑动条面向地球中心（保持水平，不旋转仰角）
        Quaternion lookRot = Quaternion.LookRotation(-flatDir, Vector3.up);

        // 只在初始时倾斜一次（绕X轴仰15°）
        Quaternion tilt = Quaternion.Euler(tiltAngle, 0, 0);

        transform.rotation = lookRot * tilt;
    }

    private void OnSliderValueChanged(SliderEventData data)
    {
        if (!earthTransform) return;

        float angle = (data.NewValue - 0.5f) * rotationRange;
        if (!clockwise) angle = -angle;

        earthTransform.localRotation = Quaternion.Euler(0, angle, 0);
    }
}
