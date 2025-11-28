using UnityEngine;
using UnityEngine.UI;
using MixedReality.Toolkit.UX;

public class MRTKSliderScrollBinder : MonoBehaviour
{
    public ScrollRect scrollRect;
    public MixedReality.Toolkit.UX.Slider slider;   // MRTK Slider, NOT Unity UI Slider

    private void Awake()
    {
        // 当 MRTK Slider 改变数值时调用
        slider.OnValueUpdated.AddListener(OnSliderValueUpdated);
    }

    private void OnSliderValueUpdated(SliderEventData data)
    {
        float v = data.NewValue;  // 0~1
        scrollRect.verticalNormalizedPosition = v;
    }
}

