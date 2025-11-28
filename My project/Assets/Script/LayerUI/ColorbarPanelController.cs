using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class LayerInfo
{
    public string type;
    public string low;
    public string high;
    public string description;
}

[System.Serializable]
public class LayerInfoCollection
{
    public List<LayerInfo> layers;
}

public class ColorbarPanelController : MonoBehaviour
{
    [Header("UI Refs")]
    public TMP_Text headerText;        // 标题：Colorbar of xx
    public TMP_Text highValueText;     // 高值
    public TMP_Text lowValueText;      // 低值
    public TMP_Text colorbarInfoText;  // 描述文字

    [Header("Renderer for colorbar material")]
    public Renderer colorbarRenderer;  // 颜色条材质显示

    private LayerInfoCollection layerData;
    private bool jsonLoaded = false;

    private void Awake()
    {
        LoadJSON();
        gameObject.SetActive(true);
        Debug.Log($"[ColorbarPanel] delete");
    }

    private void LoadJSON()
    {
        if (jsonLoaded) return;

        TextAsset jsonFile = Resources.Load<TextAsset>("colorbar_info");

        if (jsonFile == null)
        {
            Debug.LogError("[ColorbarPanel] Failed to load colorbar_info.json");
            return;
        }

        layerData = JsonUtility.FromJson<LayerInfoCollection>(jsonFile.text);
        jsonLoaded = true;

        Debug.Log($"[ColorbarPanel] Loaded {layerData.layers.Count} layers from JSON");
    }

    // ⭐⭐ 你想要的新版本：只接收 layerType
    public void Show(string layerType)
    {
        Debug.Log($"[ColorbarPanel] Show {layerType}");

        gameObject.SetActive(true);

        if (layerType.ToLower() == "humanactivity")
        {
            if (colorbarRenderer) colorbarRenderer.gameObject.SetActive(false);
            if (highValueText) highValueText.gameObject.SetActive(false);
            if (lowValueText) lowValueText.gameObject.SetActive(false);
        }
        else
        {
            if (colorbarRenderer) colorbarRenderer.gameObject.SetActive(true);
            if (highValueText) highValueText.gameObject.SetActive(true);
            if (lowValueText) lowValueText.gameObject.SetActive(true);
        }

        // 设置标题
        if (headerText)
            headerText.text = $"Colorbar of {UpperFirst(layerType)}";

        // 加载 high/low + description
        ApplyLayerInfo(layerType);

        // 加载材质
        LoadColorbarMaterial(layerType);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // 加载材质
    private void LoadColorbarMaterial(string layerType)
    {
        if (colorbarRenderer == null) return;

        string path = $"Colorbars/{layerType}_colorbar";
        Material mat = Resources.Load<Material>(path);

        if (mat != null)
        {
            colorbarRenderer.material = mat;
            Debug.Log($"[ColorbarPanel] Loaded material {path}");
        }
        else
        {
            Debug.LogWarning($"[ColorbarPanel] Material not found at {path}");
        }
    }

    // high / low / description
    private void ApplyLayerInfo(string layerType)
    {
        if (layerData == null || layerData.layers == null) return;

        LayerInfo layer = layerData.layers.Find(
            l => l.type.ToLower() == layerType.ToLower()
        );

        if (layer == null)
        {
            Debug.LogWarning($"[ColorbarPanel] No layer info for {layerType}");
            if (highValueText) highValueText.text = "-";
            if (lowValueText) lowValueText.text = "-";
            if (colorbarInfoText) colorbarInfoText.text = "No description available.";
            return;
        }

        if (lowValueText) lowValueText.text = layer.low;
        if (highValueText) highValueText.text = layer.high;
        if (colorbarInfoText) colorbarInfoText.text = layer.description;
    }

    private string UpperFirst(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpper(s[0]) + s.Substring(1);
    }
}
