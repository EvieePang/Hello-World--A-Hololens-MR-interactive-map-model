using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class CountryInfo
{
    public string name;
    public string climate;
    public string nature;
    public string humanactivity;
    public string climateType;  
}

[System.Serializable]
public class CountryInfoCollection
{
    public List<CountryInfo> countries;
}

public class ClimateLegendPanelController : MonoBehaviour
{
    [Header("UI Refs")]
    public TMP_Text headerText;   // 你说的 layer type 标题


    public TMP_Text climateTypeText;  // 面板底下那块描述文字

    [Header("Quad that shows the legend image")]
    public Renderer legendQuadRenderer;

    private CountryInfoCollection countryData;

    // 这个面板默认不显示（你可以也可以在Prefab里勾掉Active）
    private void Awake()
    {
        // start hidden
        //gameObject.SetActive(false);

        // load JSON from Resources/country_info.json
        TextAsset jsonFile = Resources.Load<TextAsset>("country_info");
        if (jsonFile == null)
        {
            Debug.LogError("[ClimateLegendPanelController] Failed to load Resources/country_info.json");
            return;
        }

        countryData = JsonUtility.FromJson<CountryInfoCollection>(jsonFile.text);
        if (countryData == null || countryData.countries == null)
        {
            Debug.LogError("[ClimateLegendPanelController] Failed to parse country_info.json");
        }
        else
        {
            Debug.Log("[ClimateLegendPanelController] Loaded " + countryData.countries.Count + " countries from JSON.");
        }
    }

    /// <summary>
    /// 打开面板，并更新内容
    /// </summary>
    /// <param name="countryName">当前选中国家，比如 "Australia"</param>
    /// <param name="climateLabel">这个国家的气候类型，比如 "Hot desert"</param>
    public void Show(string countryName)
    {
        Debug.Log($"[ClimateLegendPanelShow] countryName received: '{countryName}'");
        Awake();

        // 标题（可改可不改）
        if (headerText != null)
        {
            headerText.text = "Climate Type Legend";
        }

        string labelToDisplay = GetClimateTypeForCountry(countryName);
        climateTypeText.text = $"{labelToDisplay}";

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 隐藏整块legend panel
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private string GetClimateTypeForCountry(string countryName)
    {


        if (countryData == null || countryData.countries == null)
        {
            return "No climate data available.";
        }

        // simple linear search; you could optimize with a Dictionary if you want.
        for (int i = 0; i < countryData.countries.Count; i++)
        {
            if (countryData.countries[i].name == countryName)
            {
                // if found but climateType is empty/null, still handle gracefully
                if (!string.IsNullOrEmpty(countryData.countries[i].climateType))
                {
                    return countryData.countries[i].climateType;
                }
                else
                {
                    return "No detailed climate type description.";
                }
            }
        }

        return "No climate type found for this country.";
    }
}
