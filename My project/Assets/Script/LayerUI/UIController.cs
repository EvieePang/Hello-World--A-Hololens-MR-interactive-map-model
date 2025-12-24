using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Refs")]
    //public Transform cameraTransform;         
    public EarthLayerSwitcher layerSwitcher;   

    [Header("Panels")]
    public GameObject controlPanel;            // ControlPanel instance
    public GameObject infoPanel;               // InforPanel instance

    [Header("InfoPanel UI")]
    public TextMeshProUGUI countryNameText;    // InfoPanel/Text_CountryName
    public TextMeshProUGUI climateInfoText;    // InfoPanel/Text_ClimateInfo

    [Header("Layout")]
    //public float panelDistance = 1.2f;         // Distance between the panel and the camera
    //public float horizontalSeparation = 0.5f;  // Left and right separation distance

    private string lastSelectedCountry;

    void Start()
    {
        HidePanels();
    }

    //void LateUpdate()
    //{
    //    if (controlPanel.activeSelf || infoPanel.activeSelf)
    //    {
    //        PositionPanels();
    //    }
    //}

    //void PositionPanels()
    //{
    //    Vector3 basePos = cameraTransform.position + cameraTransform.forward * panelDistance;

    //    Vector3 leftPos = basePos - cameraTransform.right * horizontalSeparation;
    //    Vector3 rightPos = basePos + cameraTransform.right * horizontalSeparation;

    //    controlPanel.transform.position = leftPos;
    //    infoPanel.transform.position = rightPos;

    //    Quaternion faceCameraRot = Quaternion.LookRotation(
    //        controlPanel.transform.position - cameraTransform.position
    //    );
    //    controlPanel.transform.rotation = faceCameraRot;
    //    infoPanel.transform.rotation = faceCameraRot;
    //}

    // external call, when the country is clicked
    public void ShowControlPanels(string countryName)
    {
        lastSelectedCountry = countryName;
        controlPanel.SetActive(true);
        infoPanel.SetActive(false);// Do not display the right side for now.

        //PositionPanels();
    }

    public void HidePanels()
    {
        controlPanel.SetActive(false);
        infoPanel.SetActive(false);
    }

    // button envent
    public void OnClimateClicked()
    {
        layerSwitcher.SetLayer(1);

        countryNameText.text = lastSelectedCountry; 
        climateInfoText.text = $"Climate info for {lastSelectedCountry}:\nWarm & humid.\n(placeholder)";

        infoPanel.SetActive(true);
        //PositionPanels();  // position update
    }

    public void OnHumanActivityClicked()
    {
        layerSwitcher.SetLayer(2);
    }

    public void OnNatureClicked()
    {
        layerSwitcher.SetLayer(3);
    }

    public void OnPerceptionClicked()
    {
        layerSwitcher.SetLayer(4);
    }

    public void OnTemperatureClicked()
    {
        layerSwitcher.SetLayer(5);
    }
}
