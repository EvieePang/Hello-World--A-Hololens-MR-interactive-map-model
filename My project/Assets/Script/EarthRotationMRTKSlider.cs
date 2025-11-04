using UnityEngine;
using MixedReality.Toolkit.UX;   // ✅ MRTK3 UX 命名空间

/// <summary>
/// 控制地球旋转的 MRTK3 Slider 控制器
/// </summary>
public class EarthRotationMRTKSlider : MonoBehaviour
{
    [Header("References")]
    public Transform earthTransform;        // 拖 Earth
    public float rotationRange = 360f;      // 最大旋转角度
    public bool clockwise = true;           // 是否顺时针

    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        if (!slider)
        {
            Debug.LogError("❌ 未找到 MRTK Slider 组件！");
            return;
        }

        // 注册滑动事件
        slider.OnValueUpdated.AddListener(OnSliderValueChanged);
    }

    void OnDestroy()
    {
        if (slider)
            slider.OnValueUpdated.RemoveListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(SliderEventData data)
    {
        if (!earthTransform) return;

        // data.NewValue ∈ [0,1]
        float angle = (data.NewValue - 0.5f) * rotationRange;
        if (!clockwise) angle = -angle;

        earthTransform.localRotation = Quaternion.Euler(0, angle, 0);
    }
}
