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
    public GameObject panelNature;  // First: 
    public GameObject panelClimate; // Second precipitation / Temperature / Exit
    public GameObject panelHumanActivity; // Third: Population/ Agriculture / Exit
    public InfoPanelController infoPanel;  //  


    [Header("Layer Indices (corresponding EarthLayerSwitcher.layerMaterials)")]
    public int terrainIndex = 0;        // Nature correspond
    public int terrain1Index = 1;       // First
    public int terrain2Index = 2;
    public int climateIndex = 3;        // Climate 
    public int precipitationIndex = 4;  // Second menu Precipitation
    public int temperatureIndex = 5;    // Second menu Temperature
    public int humanActivityIndex = 6;  // Human Activity
    public int populationIndex = 7;     // Third
    public int agricultureIndex = 8;

    [Header("Legend Panels")]
    public ClimateLegendPanelController climateLegendPanel;  // drag your prefab instance here


    private string currentCountryName = "Unknown";


    private void Awake()
    {
        ShowRoot();  // default to show the top menu 3 buttons
    }

    // =============================

    // =============================
    public void SetCountryName(string name)
    {
        currentCountryName = name;
    }


    // ========== top menu ==========
    public void OnClickNature()
    {
        if (target) target.SetLayer(terrainIndex);
        OpenNature();
    }

    public void OnClickClimate()   
    {
        if (target) target.SetLayer(climateIndex);
        OpenClimate();
    }

    public void OnClickHumanActivity()
    {
        if (target) target.SetLayer(humanActivityIndex);
        OpenHumanActivity();
    }

    // ========== First (Nature Child Menu) ==========
    public void OnClickTerrain1()
    {
        if (target) target.SetLayer(terrain1Index);
    }

    public void OnClickTerrain2()
    {
        if (target) target.SetLayer(terrain2Index);
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
    }

    public void OnClickTemperature()
    {
        if (target) target.SetLayer(temperatureIndex);
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
    }

    public void OnClickAgriculture()
    {
        if (target) target.SetLayer(agricultureIndex);
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

        if (climateLegendPanel) climateLegendPanel.Hide();
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
    }

    public void OpenClimate()
    {
        if (infoPanel) Debug.Log($"[LayerMenu] infoPanel.activeSelf(before)={infoPanel.gameObject.activeSelf}");
        else Debug.LogWarning("[LayerMenu] infoPanel is NULL!!");
        if (panelRoot) panelRoot.SetActive(false);
        if (panelClimate) panelClimate.SetActive(true);

        if (infoPanel)
        {
            infoPanel.SetCountry(currentCountryName);
            infoPanel.Show("Climate");  
        }

        if (climateLegendPanel != null)
        {
            climateLegendPanel.Show(currentCountryName);  
        }
        else
        {
            Debug.LogWarning("[LayerMenu] climateLegendPanel is not assigned!");
        }
    }

    public void OpenHumanActivity()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (panelHumanActivity) panelHumanActivity.SetActive(true);

        if (infoPanel)
        {
            infoPanel.SetCountry(currentCountryName);
            infoPanel.Show("HumanActivity");  
        }
    }

    public void OnClickSetLayer(int index)
    {
        if (target) target.SetLayer(index);
    }

    public void CloseSelf() => Destroy(gameObject);
}

