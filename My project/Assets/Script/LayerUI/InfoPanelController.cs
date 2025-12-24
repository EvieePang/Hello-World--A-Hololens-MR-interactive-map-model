using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class CountryData
{
    public string name;
    public string climate;
    public string nature;
    public string humanactivity;
    public string temperature;
    public string precipitation;
    public string terrain;
    public string forest;
    public string gdp;
    public string population;
}

[System.Serializable]
public class CountryDataCollection
{
    public List<CountryData> countries;
}

public class InfoPanelController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text countryNameText;
    public TMP_Text infoText;
    public GameObject panelRoot;

    private string currentCountry;
    private CountryDataCollection dataCollection;

    private void Awake()
    {
        // Load all country data from json file in Resources
        TextAsset jsonFile = Resources.Load<TextAsset>("country_info_all"); 
        if (jsonFile != null)
        {
            dataCollection = JsonUtility.FromJson<CountryDataCollection>(jsonFile.text);
        }
    }

    public void SetCountry(string name)
    {
        currentCountry = name;
    }

    public void Show(string infoCategory)
    {
        if (!panelRoot) return;
        panelRoot.SetActive(true);

        if (countryNameText)
            countryNameText.text = currentCountry;

        if (infoText)
            infoText.text = GenerateInfo(infoCategory);
    }

    public void Hide()
    {
        if (panelRoot)
            panelRoot.SetActive(false);
    }

    // load description of every country in all topics
    private string GenerateInfo(string category = "climate")
    {
        if (dataCollection == null)
            return " Data not loaded.";

        CountryData match = dataCollection.countries.Find(c => c.name == currentCountry);
        if (match == null)
            return $"No info found for {currentCountry}.";

        switch (category.ToLower())
        {
            case "climate":
                return $" <b>Climate Information</b>:\n{match.climate}";
            case "nature":
                return $" <b>Nature Information</b>:\n{match.nature}";
            case "humanactivity":
                return $" <b>Human Activity Information</b>:\n{match.humanactivity}";
            case "terrain":
                return $" <b>Terrain Information</b>:\n{match.terrain}";
            case "forest":
                return $" <b>Forest Information</b>:\n{match.forest}";
            case "temperature":
                return $" <b>Temperature Information</b>:\n{match.temperature}";
            case "precipitation":
                return $" <b>Precipitationn Information</b>:\n{match.precipitation}";
            case "gdp":
                return $" <b>GDP Information</b>:\n{match.gdp}";
            case "population":
                return $" <b>Population Information</b>:\n{match.population}";
            default:
                return $"{category} info for {currentCountry} not found.";
        }
    }
}
