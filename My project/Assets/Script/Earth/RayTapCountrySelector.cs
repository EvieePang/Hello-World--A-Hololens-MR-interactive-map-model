using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RayTapCountrySelector : MonoBehaviour
{
    public CountryClickController controller;

    private XRRayInteractor[] rays;
    private float clickThreshold = 0.3f;   // 小于这个时间算“点击”
    private float pressStartTime = -1f;
    private GameObject currentHover = null;
    private GameObject lastHover = null;


    void Start()
    {
        rays = FindObjectsOfType<XRRayInteractor>(true);
        if (rays.Length == 0)
        {
            Debug.LogError("[RayTapCountrySelector] No XRRayInteractors found!");
        }

        foreach (var ray in rays)
        {
            ray.selectEntered.AddListener(OnSelectEntered);
            ray.selectExited.AddListener(OnSelectExited);
        }
    }

    void Update()
    {
        DetectHover();
    }


    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        pressStartTime = Time.time;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        // 判断是否是一次“轻触点击”
        if (Time.time - pressStartTime < clickThreshold)
        {
            TryClickCountry();
        }
    }

    private void TryClickCountry()
    {
        foreach (var ray in rays)
        {
            if (ray.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                var go = hit.collider.gameObject;
                if (go.CompareTag("Country"))
                {
                    controller.FocusCountry(go);
                    return;
                }
            }
        }
    }

    private void DetectHover()
    {
        currentHover = null;

        // 找出射线打到的国家
        foreach (var ray in rays)
        {
            if (ray.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                GameObject go = hit.collider.gameObject;
                if (go.CompareTag("Country"))
                {
                    currentHover = go;
                    break;
                }
            }
        }

        // 没指到国家 → 清除 hover
        if (currentHover == null)
        {
            if (lastHover != null)
            {
                controller.HoverExitCountry(lastHover);
                lastHover = null;
            }
            return;
        }

        // 当前 hover 与上一次不同 → 更新 hover 状态
        if (currentHover != lastHover)
        {
            if (lastHover != null)
                controller.HoverExitCountry(lastHover);

            controller.HoverEnterCountry(currentHover);
            lastHover = currentHover;
        }
    }

}
