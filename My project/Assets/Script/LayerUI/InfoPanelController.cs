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
        // Load JSON file from Resources
        TextAsset jsonFile = Resources.Load<TextAsset>("country_info"); // Assets/Resources/country_info.json
        if (jsonFile != null)
        {
            dataCollection = JsonUtility.FromJson<CountryDataCollection>(jsonFile.text);
            //UnityEngine.Debug.Log("[InfoPanelController] JSON loaded successfully.");
        }
        else
        {
            UnityEngine.Debug.LogError("[InfoPanelController] Failed to load country_info.json from Resources.");
        }

        // Optional: hide panel by default
        // if (panelRoot)
        //     panelRoot.SetActive(false);
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
            default:
                return $"{category} info for {currentCountry} not found.";
        }
    }
}
