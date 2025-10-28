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
    public GameObject panelClimate; // Second：Precipitation / Temperature / Exit
    public GameObject panelHumanActivity; // Third: Population/ Agriculture / Exit
    public InfoPanelController infoPanel;  //  引用新脚本


    [Header("Layer Indices (corresponding EarthLayerSwitcher.layerMaterials)")]
    public int terrainIndex = 0;        // Nature correspond
    public int terrain1Index = 1;       // First
    public int terrain2Index = 2;
    public int climateIndex = 3;        // Climate（when tap “Climate”, change to it first）
    public int precipitationIndex = 4;  // Second menu Precipitation
    public int temperatureIndex = 5;    // Second menu Temperature
    public int humanActivityIndex = 6;  // Human Activity
    public int populationIndex = 7;     // Third
    public int agricultureIndex = 8;

    [Header("Legend Panels")]
    public ClimateLegendPanelController climateLegendPanel;  // ← drag your prefab instance here


    private string currentCountryName = "Unknown";


    private void Awake()
    {
        ShowRoot();  // default to show the top menu 3 buttons
    }

    // =============================
    // 外部接口（ContextMenuSpawner 会调用）
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

    public void OnClickExit() // back to the top menu
    {
        ShowRoot();
    }

    // ========== Second（Climate Child Menu）==========
    public void OnClickPrecipitation()
    {
        if (target) target.SetLayer(precipitationIndex);
    }

    public void OnClickTemperature()
    {
        if (target) target.SetLayer(temperatureIndex);
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

    // ========== 面板切换工具 ==========
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
            infoPanel.Show("Nature");  // 用新脚本显示
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
            infoPanel.Show("Climate");  // 用新脚本显示
        }

        if (climateLegendPanel != null)
        {
            // 这里假设你在点击国家时已经记录了它的气候类型（来自 JSON）
            // 如果暂时没有，可以先写 "Unknown climate type"
            climateLegendPanel.Show(currentCountryName);  // ← added
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
            infoPanel.Show("HumanActivity");  // 用新脚本显示
        }
    }

    // 仍保留你原先给 LayerButtonBinder 用的接口（不想改 Binder 的话）
    public void OnClickSetLayer(int index)
    {
        if (target) target.SetLayer(index);
    }

    public void CloseSelf() => Destroy(gameObject);
}

