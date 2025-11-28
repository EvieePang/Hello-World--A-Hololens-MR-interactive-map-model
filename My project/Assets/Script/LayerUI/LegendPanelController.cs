using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class LegendInfoEntry
{
    public string layer;    // 比如 "climate", "population"
    public string text;     // 显示在 LegendTypeText 里的文字
    public string image;    // 对应 Resources/Legends 下的图片名
}

[System.Serializable]
public class LegendInfoCollection
{
    public List<LegendInfoEntry> layers;
}

public class LegendPanelController : MonoBehaviour
{
    [Header("UI Refs")]
    public TMP_Text headerText;       // 图层标题
    public TMP_Text LegendTypeText;   // 描述文字

    [Header("Scroll Content (Image Loader)")]
    public Image contentImage;
    public float desiredWidth = 230f;


    private LegendInfoCollection legendInfo;

    private void Awake()
    {
        // 读取 legend json
        TextAsset jsonFile = Resources.Load<TextAsset>("legend_colorbar_info");
        if (jsonFile == null)
        {
            Debug.LogError("[LegendPanelController] Missing Resources/legend_colorbar_info.json");
            return;
        }

        legendInfo = JsonUtility.FromJson<LegendInfoCollection>(jsonFile.text);
        if (legendInfo == null || legendInfo.layers == null)
        {
            Debug.LogError("[LegendPanelController] Failed to parse legend_colorbar_info.json");
        }
        else
        {
            Debug.Log($"[LegendPanelController] Loaded {legendInfo.layers.Count} legend entries.");
        }
    }

    /// <summary>
    /// 根据 layerType 显示 legend 面板
    /// </summary>
    public void Show(string layerType)
    {
        Awake();
        Debug.Log($"[LegendPanel] Show legend for layer = {layerType}");

        // 设置标题
        if (headerText != null)
            headerText.text = $"{UppercaseFirst(layerType)} Legend";

        // 查询 JSON 数据
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

    /// <summary>
    /// 查找 legend 条目
    /// </summary>
    private LegendInfoEntry FindLegendEntry(string layerType)
    {
        if (legendInfo == null || legendInfo.layers == null)
            return null;

        foreach (var e in legendInfo.layers)
            if (e.layer == layerType)
                return e;

        return null;
    }

    /// <summary>
    /// 从 Resources/Legends 加载对应图层图像
    /// </summary>
    private void LoadLegendImage(string imageName)
    {
        if (contentImage == null)
        {
            Debug.LogError("[LegendPanel] legendImageUI not assigned.");
            return;
        }

        Sprite sprite = Resources.Load<Sprite>($"Legends/{imageName}");
        if (sprite == null)
        {
            Debug.LogError($"[LegendPanel] Missing legend sprite: Resources/Legends/{imageName}.png");
            return;
        }

        float originalWidth = sprite.rect.width;
        float originalHeight = sprite.rect.height;

        // 计算缩放比例
        float scale = desiredWidth / originalWidth;

        float newWidth = desiredWidth;
        float newHeight = originalHeight * scale;

        // 应用缩放
        contentImage.rectTransform.sizeDelta = new Vector2(newWidth, newHeight);

        Debug.Log($"[LegendPanel] Force resize to {newWidth} x {newHeight} (scale = {scale})");

        //FitImageToWidth(sprite);

        contentImage.sprite = sprite;

        
    }

    //private void FitImageToWidth(Sprite sprite)
    //{
    //    float originalWidth = sprite.rect.width;
    //    float originalHeight = sprite.rect.height;

    //    // 计算缩放比例
    //    float scale = desiredWidth / originalWidth;

    //    float newWidth = desiredWidth;
    //    float newHeight = originalHeight * scale;

    //    // 应用缩放
    //    contentImage.rectTransform.sizeDelta = new Vector2(newWidth, newHeight);

    //    Debug.Log($"[LegendPanel] Force resize to {newWidth} x {newHeight} (scale = {scale})");
    //}


    /// <summary>
    /// 隐藏
    /// </summary>
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
