using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RayTapCountrySelector : MonoBehaviour
{
    public CountryClickController controller;

    private XRRayInteractor[] rays;
    private float clickThreshold = 0.25f;   // 小于这个时间算“点击”
    private float pressStartTime = -1f;

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
                    controller.FocusCountryWrapper(go);
                    return;
                }
            }
        }
    }
}
