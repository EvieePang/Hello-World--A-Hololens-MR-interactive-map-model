using UnityEngine;
using TMPro;

public class InfoPanelController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text countryNameText;
    public TMP_Text infoText;
    public GameObject panelRoot;  // PanelInfo 根物体

    private string currentCountry;

    //private void Awake()
    //{
    //    Debug.Log("[InfoPanel] Awake() on " + gameObject.name + ", activeSelf=" + gameObject.activeSelf);
        
    //    panelRoot.SetActive(true); // 默认隐藏
    //}

    public void SetCountry(string name)
    {
        currentCountry = name;
    }

    public void Show(string infoCategory = "Climate")
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

    // 可扩展成不同类别信息
    private string GenerateInfo(string category)
    {
        switch (category)
        {
            case "Climate":
                return $"Climate information of {currentCountry}\nWarm & humid (placeholder)";
            case "Population":
                return $"Population data for {currentCountry}\n(placeholder)";
            default:
                return $"{category} info for {currentCountry}";
        }
    }
}
