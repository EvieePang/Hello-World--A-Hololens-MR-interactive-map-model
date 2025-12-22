using UnityEngine;

// Controls the visual transition between Earth surface mode and Water mode based on globe scale.
public class EarthScaleTransitionController : MonoBehaviour
{
    [Header("Earth References")]
    public Transform earthTransform;

    [Header("Layer Renderers")]
    public Renderer earthLayerRenderer;  
    public GameObject earthCountries;    

    [Header("Materials")]
    public Material earthMaterial;       
    public Material waterMaterial;        

    [Header("Settings")]
    public float threshold = 0.07f;       
    private bool isWaterMode = false;

    void Start()
    {
        earthLayerRenderer.sharedMaterial = earthMaterial;
        earthCountries.SetActive(false);
    }

    // Monitor Earth scale and trigger mode transitions when crossing the threshold
    void Update()
    {
        float scale = earthTransform.localScale.x;

        if (!isWaterMode && scale >= threshold)
        {
            isWaterMode = true;
            ApplyWaterMode(true);
        }
        else if (isWaterMode && scale < threshold)
        {
            isWaterMode = false;
            ApplyWaterMode(false);
        }
    }

    // Applies the material and visibility changes required for switching modes
    void ApplyWaterMode(bool toWater)
    {
        // Switch to Water mode or revert back to Earth mode
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
