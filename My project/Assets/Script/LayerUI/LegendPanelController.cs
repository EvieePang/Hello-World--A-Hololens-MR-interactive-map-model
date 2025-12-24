using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class LegendInfoEntry
{
    public string layer;    // topic name
    public string text;     // the description of this layer's legend
    public string image;    // the filename of legend image
}

[System.Serializable]
public class LegendInfoCollection
{
    public List<LegendInfoEntry> layers;
}

public class LegendPanelController : MonoBehaviour
{
    [Header("UI Refs")]
    public TMP_Text headerText;       // title(topic name)
    public TMP_Text LegendTypeText;   // description

    [Header("Scroll Content (Image Loader)")]
    public Image contentImage;
    public float desiredWidth = 230f;


    private LegendInfoCollection legendInfo;

    private void Awake()
    {
        // load legend json
        TextAsset jsonFile = Resources.Load<TextAsset>("legend_colorbar_info");

        legendInfo = JsonUtility.FromJson<LegendInfoCollection>(jsonFile.text);
    }

    // show legend panel
    public void Show(string layerType)
    {
        Awake();

        if (headerText != null)
            headerText.text = $"{UppercaseFirst(layerType)} Legend";

        // match with json data
        LegendInfoEntry entry = FindLegendEntry(layerType);
        if (entry == null)
        {
            LegendTypeText.text = "No legend description available.";
        }
        else
        {
            LegendTypeText.text = entry.text;
            LoadLegendImage(entry.image);
        }

        gameObject.SetActive(true);
    }

    // find layer's legend info 
    private LegendInfoEntry FindLegendEntry(string layerType)
    {
        if (legendInfo == null || legendInfo.layers == null)
            return null;

        foreach (var e in legendInfo.layers)
            if (e.layer == layerType)
                return e;

        return null;
    }


    /// load corresponding legend image from Resources/Legends 
    private void LoadLegendImage(string imageName)
    {

        Sprite sprite = Resources.Load<Sprite>($"Legends/{imageName}");

        float originalWidth = sprite.rect.width;
        float originalHeight = sprite.rect.height;

        // calculate scale factor
        float scale = desiredWidth / originalWidth;

        float newWidth = desiredWidth;
        float newHeight = originalHeight * scale;

        // apply scale factor
        contentImage.rectTransform.sizeDelta = new Vector2(newWidth, newHeight);

        contentImage.sprite = sprite;

        
    }

   
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private string UppercaseFirst(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpper(s[0]) + s.Substring(1);
    }
}
