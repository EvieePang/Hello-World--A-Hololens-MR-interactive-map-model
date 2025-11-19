using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerButtonBinderPressable : MonoBehaviour
{
    [Tooltip("this button switch to material index (same with EarthLayerSwitcher.layerMaterials)")]
    public int layerIndex = 0;

    [Tooltip("Menu Root LayerMenu")]
    public LayerMenu menu;

    [Tooltip("object's PressableButton component")]
    public PressableButton button;

    private void Reset()
    {
        // auto grab
        if (!button) button = GetComponent<PressableButton>();
        if (!menu) menu = GetComponentInParent<LayerMenu>();
    }

    private void Awake()
    {
        if (!button) button = GetComponent<PressableButton>();
        if (!menu) menu = GetComponentInParent<LayerMenu>();

        if (button)
        {
            // subscript PressableButton's OnClicked£¨no parameters)
            button.OnClicked.AddListener(() =>
            {
                if (menu) menu.OnClickSetLayer(layerIndex);
            });
        }
        else
        {
            Debug.LogWarning($"[{nameof(LayerButtonBinderPressable)}] cann't find PressableButton¡£");
        }
    }
}
