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
    public TMP_Text headerText;        // title
    public TMP_Text highValueText;     // high value and low value
    public TMP_Text lowValueText;     
    public TMP_Text colorbarInfoText;  // description of the colorbar

    [Header("Renderer for colorbar material")]
    public Renderer colorbarRenderer;  

    private LayerInfoCollection layerData;
    private bool jsonLoaded = false;

    private void Awake()
    {
        LoadJSON();
        gameObject.SetActive(true);
    }

    private void LoadJSON()
    {
        if (jsonLoaded) return;

        TextAsset jsonFile = Resources.Load<TextAsset>("colorbar_info");

        layerData = JsonUtility.FromJson<LayerInfoCollection>(jsonFile.text);
        jsonLoaded = true;

    }

    public void Show(string layerType)
    {
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

        // set title
        if (headerText)
            headerText.text = $"Description of {UpperFirst(layerType)} layer";

        // load high/low + description
        ApplyLayerInfo(layerType);

        // load render
        LoadColorbarMaterial(layerType);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // load corresponding colorbar material based on the layer type
    private void LoadColorbarMaterial(string layerType)
    {
        if (colorbarRenderer == null) return;

        string path = $"Colorbars/{layerType}_colorbar";
        Material mat = Resources.Load<Material>(path);

        if (mat != null)
        {
            colorbarRenderer.material = mat;
        }

    }

    // load high+low+description
    private void ApplyLayerInfo(string layerType)
    {
        if (layerData == null || layerData.layers == null) return;

        LayerInfo layer = layerData.layers.Find(
            l => l.type.ToLower() == layerType.ToLower()
        );

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
