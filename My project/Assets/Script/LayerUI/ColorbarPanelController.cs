using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class LayerInfo
{
    public string type;
    public string low;
    public string high;
}

[System.Serializable]
public class LayerInfoCollection
{
    public List<LayerInfo> layers;
}

public class ColorbarPanelController : MonoBehaviour
{
    [Header("UI Refs")]
    public TMP_Text headerText;        // 显示当前图层类型，如 "Terrain"
    public TMP_Text countryNameText;   // 显示国家名
    public TMP_Text highValueText;     // 高值标签
    public TMP_Text lowValueText;      // 低值标签

    [Header("Renderer for colorbar material")]
    public Renderer colorbarRenderer;  // 显示colorbar的Quad

    private LayerInfoCollection layerData;

    private void Awake()
    {
        // 从 Resources 加载 colorbar_info.json
        TextAsset jsonFile = Resources.Load<TextAsset>("colorbar_info");
        if (jsonFile == null)
        {
            Debug.LogError("[ColorbarPanelController] Failed to load Resources/colorbar_info.json");
            return;
        }

        layerData = JsonUtility.FromJson<LayerInfoCollection>(jsonFile.text);
        if (layerData == null || layerData.layers == null)
        {
            Debug.LogError("[ColorbarPanelController] Failed to parse colorbar_info.json");
        }
        else
        {
            Debug.Log($"[ColorbarPanelController] Loaded {layerData.layers.Count} layers from JSON.");
        }

        // 默认隐藏
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示 Colorbar 面板，并加载对应材质与高低值
    /// </summary>
    public void Show(string countryName, string layerType)
    {
        Debug.Log($"[ColorbarPanelController] Show called: {countryName}, layer={layerType}");
        Debug.Log($"[ColorbarPanelController] GameObject active before: {gameObject.activeSelf}");
        gameObject.SetActive(true);
        Debug.Log($"[ColorbarPanelController] GameObject active after: {gameObject.activeSelf}");
        Awake();

        if (headerText != null)
            headerText.text = $"Colorbar of {layerType}";

        if (countryNameText != null)
            countryNameText.text = $"in {countryName}:";

        LoadColorbarMaterial(layerType);
        SetHighLowValues(layerType);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 从 Resources/Colorbars 下加载对应的 colorbar 材质
    /// </summary>
    private void LoadColorbarMaterial(string layerType)
    {
        if (colorbarRenderer == null)
        {
            Debug.LogWarning("[ColorbarPanelController] colorbarRenderer not assigned!");
            return;
        }

        string path = $"Colorbars/{layerType}_colorbar";  // e.g. Resources/Colorbars/terrain_colorbar.mat
        Material mat = Resources.Load<Material>(path);

        if (mat != null)
        {
            colorbarRenderer.material = mat;
            Debug.Log($"[ColorbarPanelController] Loaded material: {path}");
        }
        else
        {
            Debug.LogWarning($"[ColorbarPanelController] Material not found at: {path}");
        }
    }

    /// <summary>
    /// 从 JSON 中找到对应 layer 的 high / low 值并更新 UI
    /// </summary>
    private void SetHighLowValues(string layerType)
    {
        if (layerData == null || layerData.layers == null)
        {
            Debug.LogWarning("[ColorbarPanelController] No layer data loaded.");
            return;
        }

        LayerInfo layer = layerData.layers.Find(l => l.type.ToLower() == layerType.ToLower());
        if (layer != null)
        {
            if (lowValueText != null)
                lowValueText.text = layer.low;

            if (highValueText != null)
                highValueText.text = layer.high;

            Debug.Log($"[ColorbarPanelController] {layerType} range: {layer.low} - {layer.high}");
        }
        else
        {
            Debug.LogWarning($"[ColorbarPanelController] Layer type '{layerType}' not found in JSON!");
            if (lowValueText != null) lowValueText.text = "-";
            if (highValueText != null) highValueText.text = "-";
        }
    }
}

