using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Refs")]
    //public Transform cameraTransform;          // 拖 Main Camera
    public EarthLayerSwitcher layerSwitcher;   // 拖 Earth 上的那个脚本

    [Header("Panels")]
    public GameObject controlPanel;            // 拖场景里的 ControlPanel 实例
    public GameObject infoPanel;               // 拖场景里的 InfoPanel 实例

    [Header("InfoPanel UI")]
    public TextMeshProUGUI countryNameText;    // 拖 InfoPanel/Text_CountryName
    public TextMeshProUGUI climateInfoText;    // 拖 InfoPanel/Text_ClimateInfo

    [Header("Layout")]
    //public float panelDistance = 1.2f;         // 面板离相机的距离（1~2）
    //public float horizontalSeparation = 0.5f;  // 左右分开距离（越大越开）

    private string lastSelectedCountry;

    void Start()
    {
        // 一开始隐藏
        HidePanels();
    }

    //void LateUpdate()
    //{
    //    // 如果面板是打开状态，我们每帧都重新把它们贴在相机前方，保持固定HUD效果
    //    if (controlPanel.activeSelf || infoPanel.activeSelf)
    //    {
    //        PositionPanels();
    //    }
    //}

    //void PositionPanels()
    //{
    //    // 相机正前方一个基准点
    //    Vector3 basePos = cameraTransform.position + cameraTransform.forward * panelDistance;

    //    // 左边是控制面板
    //    Vector3 leftPos = basePos - cameraTransform.right * horizontalSeparation;
    //    // 右边是信息面板
    //    Vector3 rightPos = basePos + cameraTransform.right * horizontalSeparation;

    //    controlPanel.transform.position = leftPos;
    //    infoPanel.transform.position = rightPos;

    //    // 两个面板都朝向相机
    //    Quaternion faceCameraRot = Quaternion.LookRotation(
    //        controlPanel.transform.position - cameraTransform.position
    //    );
    //    controlPanel.transform.rotation = faceCameraRot;
    //    infoPanel.transform.rotation = faceCameraRot;
    //}

    // ==== 外部调用：国家被点击时 ====
    public void ShowControlPanels(string countryName)
    {
        // 打开面板
        lastSelectedCountry = countryName;
        controlPanel.SetActive(true);
        infoPanel.SetActive(false);// ← 先不显示右侧


        // 立即摆一次，避免第一帧闪一下
        //PositionPanels();
    }

    public void HidePanels()
    {
        controlPanel.SetActive(false);
        infoPanel.SetActive(false);
    }

    // ==== 下面是按钮事件（拖到 Button.onClick） ====
    public void OnClimateClicked()
    {
        // 例如 index=1 表示 global climate layer
        layerSwitcher.SetLayer(1);

        // 更新文字（硬编码版本，不读JSON）
        countryNameText.text = lastSelectedCountry; // 或直接传参数
        climateInfoText.text = $"Climate info for {lastSelectedCountry}:\nWarm & humid.\n(placeholder)";

        // 在点击 Climate 后再弹出右侧信息面板
        infoPanel.SetActive(true);
        //PositionPanels();  // 更新位置
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
