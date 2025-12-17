using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using TMPro;
using UnityEngine;

public class LayerMenu : MonoBehaviour
{
    [Header("Target")]
    public EarthLayerSwitcher target;

    [Header("Panels")]
    public GameObject panelRoot;    // top: Nature / Climate / Human Activity
    public GameObject panelNature;  // First: Terrain / Forest / Exit
    public GameObject panelClimate; // Second: precipitation / Temperature / Exit
    public GameObject panelHumanActivity; // Third: Population/ Agriculture / Exit
    public InfoPanelController infoPanel;  //  


    [Header("Layer Indices (corresponding EarthLayerSwitcher.layerMaterials)")]
    public int terrainIndex = 0;        // Nature correspond
    public int terrain1Index = 1;       // Terrain
    public int terrain2Index = 2;       // Forest
    public int climateIndex = 3;        // Climate 
    public int precipitationIndex = 4;  // Second menu Precipitation
    public int temperatureIndex = 5;    // Second menu Temperature
    public int humanActivityIndex = 6;  // Human Activity
    public int populationIndex = 7;     // population
    public int GPDIndex = 8;    // GDP

    [Header("Legend Panels")]
    public LegendPanelController LegendPanel;  // drag your prefab instance here
    public ColorbarPanelController colorbarPanel;


    private string currentCountryName = "Unknown";

    [Header("Country Control")]
    public CountryClickController countryController;


    private void Awake()
    {
        ShowRoot();  // default to show the top menu 3 buttons

        if (!countryController)
        {
            countryController = FindObjectOfType<CountryClickController>();
            if (countryController)
                Debug.Log($"[LayerMenu] Auto-bound CountryClickController: {countryController.name}");
            else
                Debug.LogWarning("[LayerMenu] CountryClickController not found in scene!");
        }

}

    public void SetCountryName(string name)
    {
        currentCountryName = name;
    }


    // ========== top menu ==========
    public void OnClickNature()
    {
        if (target) target.SetLayer(terrainIndex);
        if (countryController) countryController.SetCurrentCountryAlpha(0.0f);
        OpenNature();
    }

    public void OnClickClimate()   
    {
        if (target) target.SetLayer(climateIndex);
        if (countryController) countryController.SetCurrentCountryAlpha(0.0f);
        OpenClimate();
    }

    public void OnClickHumanActivity()
    {
        if (target) target.SetLayer(humanActivityIndex);
        if (countryController) countryController.SetCurrentCountryAlpha(0.0f);
        OpenHumanActivity();
    }

    // ========== First (Nature Child Menu) ==========
    public void OnClickTerrain1()
    {
        if (target) target.SetLayer(terrain1Index);
        OpenTerrain();
    }

    public void OnClickTerrain2()
    {
        if (target) target.SetLayer(terrain2Index);
        OpenForest();
    }

    public void OnClickExitandBackNature() // back to the top menu
    {
        ShowRoot();
        if (target) target.SetLayer(terrainIndex);
    }

    // ========== Second Climate Child Menu==========
    public void OnClickPrecipitation()
    {
        if (target) target.SetLayer(precipitationIndex);
        OpenPrecipitation();
    }

    public void OnClickTemperature()
    {
        if (target) target.SetLayer(temperatureIndex);
        OpenTemperature();
    }

    public void OnClickExitandBackClimate() // back to the top menu
    {
        ShowRoot();
        if (target) target.SetLayer(climateIndex);
    }

    // ========== Third (Human Activity Child Menu) =========
    public void OnClickPopulation()
    {
        if (target) target.SetLayer(populationIndex);
        OpenPopulation();   
    }

    public void OnClickAgriculture()
    {
        if (target) target.SetLayer(GPDIndex);
        OpenGDP();
    }

    public void OnClickExitandBackHA() // back to the top menu
    {
        ShowRoot();
        if (target) target.SetLayer(humanActivityIndex);
    }

    // ========== menu switch ==========
    public void ShowRoot()
    {
        if (panelRoot) panelRoot.SetActive(true);
        if (panelNature) panelNature.SetActive(false);
        if (panelClimate) panelClimate.SetActive(false);
        if (panelHumanActivity) panelHumanActivity.SetActive(false);
        if (infoPanel) infoPanel.Hide();

        if (LegendPanel) LegendPanel.Hide();
        if (colorbarPanel) colorbarPanel.Hide();

    }

    public void OpenNature()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (panelNature) panelNature.SetActive(true);

        if (infoPanel)
        {
            infoPanel.SetCountry(currentCountryName);
            infoPanel.Show("Nature");  
        }

        if (colorbarPanel!= null)
        {
            colorbarPanel.Show("nature");
        }
    }

    public void OpenTerrain()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (panelNature) panelNature.SetActive(true);

        if (infoPanel)
        {
            infoPanel.SetCountry(currentCountryName);
            infoPanel.Show("Terrain");
        }

        if (colorbarPanel != null)
        {
            colorbarPanel.Show("terrain");
        }
    }

    public void OpenForest()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (panelNature) panelNature.SetActive(true);

        if (infoPanel)
        {
            infoPanel.SetCountry(currentCountryName);
            infoPanel.Show("Forest");
        }

        if (colorbarPanel != null)
        {
            colorbarPanel.Show("forest");
        }
    }

    public void OpenClimate()
    {

        if (panelRoot) panelRoot.SetActive(false);
        if (panelClimate) panelClimate.SetActive(true);

        if (infoPanel)
        {
            infoPanel.SetCountry(currentCountryName);
            infoPanel.Show("Climate");  
        }

        if (LegendPanel != null)
        {
            LegendPanel.Show("climate");  
        }
        else
        {
            Debug.LogWarning("[LayerMenu] LegendPanel is not assigned!");
        }
    }

    public void OpenTemperature()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (panelClimate) panelClimate.SetActive(true);
        if (LegendPanel) LegendPanel.Hide();

        if (infoPanel)
        {
            infoPanel.SetCountry(currentCountryName);
            infoPanel.Show("Temperature");
        }

        if (colorbarPanel != null)
        {
            colorbarPanel.Show("temperature");
        }
    }

    public void OpenPrecipitation()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (panelClimate) panelClimate.SetActive(true);
        if (LegendPanel) LegendPanel.Hide();

        if (infoPanel)
        {
            infoPanel.SetCountry(currentCountryName);
            infoPanel.Show("Precipitation");
        }

        if (colorbarPanel != null)
        {
            colorbarPanel.Show("precipitation");
        }
    }

    public void OpenHumanActivity()
    {
        if (panelRoot) panelRoot.SetActive(false);
        panelHumanActivity.SetActive(true);

        if (infoPanel)
        {
            infoPanel.SetCountry(currentCountryName);
            infoPanel.Show("HumanActivity");  
        }

        if (colorbarPanel != null)
        {
            colorbarPanel.Show("humanactivity");
        }
    }

    public void OpenGDP()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (panelHumanActivity) panelHumanActivity.SetActive(true);

        if (infoPanel)
        {
            infoPanel.SetCountry(currentCountryName);
            infoPanel.Show("gdp");
        }
        if (LegendPanel != null)
        {
            LegendPanel.Hide();
        }
        if (colorbarPanel != null)
        {
            colorbarPanel.Show("gdp");
        }
    }

    public void OpenPopulation()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (panelHumanActivity) panelHumanActivity.SetActive(true);

        if (colorbarPanel != null)
        {
            colorbarPanel.Hide();
        }
        if (infoPanel)
        {
            infoPanel.SetCountry(currentCountryName);
            infoPanel.Show("Population");
        }

        if (LegendPanel != null)
        {
            LegendPanel.Show("population");
        }
    }



    public void OnClickSetLayer(int index)
    {
        if (target) target.SetLayer(index);
    }

    public void CloseSelf() => Destroy(gameObject);
}

