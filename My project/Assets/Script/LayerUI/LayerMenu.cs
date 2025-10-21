using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        ShowRoot();  // default to show the top menu 3 buttons
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
    }

    public void OpenNature()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (panelNature) panelNature.SetActive(true);
    }

    public void OpenClimate()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (panelClimate) panelClimate.SetActive(true);
    }

    public void OpenHumanActivity()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (panelHumanActivity) panelHumanActivity.SetActive(true);
    }

    // 仍保留你原先给 LayerButtonBinder 用的接口（不想改 Binder 的话）
    public void OnClickSetLayer(int index)
    {
        if (target) target.SetLayer(index);
    }

    public void CloseSelf() => Destroy(gameObject);
}

