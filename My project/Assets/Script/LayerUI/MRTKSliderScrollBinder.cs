using UnityEngine;
using UnityEngine.UI;
using MixedReality.Toolkit.UX;

public class MRTKSliderScrollBinder : MonoBehaviour
{
    public ScrollRect scrollRect;
    public MixedReality.Toolkit.UX.Slider slider;   

    private void Awake()
    {
        //call when MRTK Slider's value change
        slider.OnValueUpdated.AddListener(OnSliderValueUpdated);
    }

    // tie the value of MRTK Slider with the scroll view vertical position
    private void OnSliderValueUpdated(SliderEventData data)
    {
        float v = data.NewValue;  // 0~1
        scrollRect.verticalNormalizedPosition = v;
    }
}

