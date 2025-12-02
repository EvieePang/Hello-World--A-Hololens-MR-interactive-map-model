using UnityEngine;

public class EarthScaleTransitionController : MonoBehaviour
{
    [Header("Earth References")]
    public Transform earthTransform;

    [Header("Layer Renderers")]
    public Renderer earthLayerRenderer;   // EarthLayers 的 MeshRenderer
    public GameObject earthCountries;     // EarthCountries parent

    [Header("Materials")]
    public Material earthMaterial;        // 初始地球材质
    public Material waterMaterial;        // 水面材质

    [Header("Settings")]
    public float threshold = 0.07f;       // 切换阈值

    private bool isWaterMode = false;

    void Start()
    {
        // 初始状态：正常地球
        earthLayerRenderer.sharedMaterial = earthMaterial;
        earthCountries.SetActive(false);
    }

    void Update()
    {
        float scale = earthTransform.localScale.x;

        // 切换到 Water
        if (!isWaterMode && scale >= threshold)
        {
            isWaterMode = true;
            ApplyWaterMode(true);
        }
        // 切换回 Earth
        else if (isWaterMode && scale < threshold)
        {
            isWaterMode = false;
            ApplyWaterMode(false);
        }
    }

    void ApplyWaterMode(bool toWater)
    {
        if (toWater)
        {
            earthLayerRenderer.sharedMaterial = waterMaterial;
            earthCountries.SetActive(true);
        }
        else
        {
            earthLayerRenderer.sharedMaterial = earthMaterial;
            earthCountries.SetActive(false);
        }
    }
}
