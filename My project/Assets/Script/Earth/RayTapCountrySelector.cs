using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Handles ray-based tap and hover interaction for country selection using XR ray interactors.
public class RayTapCountrySelector : MonoBehaviour
{
    public CountryClickController controller;
    private XRRayInteractor[] rays;
    private float clickThreshold = 0.3f;  
    private float pressStartTime = -1f;
    private GameObject currentHover = null;
    private GameObject lastHover = null;


    // Initialize and register all XR ray interactors for select events
    void Start()
    {
        // Find all XRRayInteractors in the scene
        rays = FindObjectsOfType<XRRayInteractor>(true);
       

        // Subscribe to select enter and exit events for each ray
        foreach (var ray in rays)
        {
            ray.selectEntered.AddListener(OnSelectEntered);
            ray.selectExited.AddListener(OnSelectExited);
        }
    }

    // Per-frame update to detect hover changes
    void Update()
    {
        DetectHover();
    }


    // Called when a select action begins; records press start time
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        pressStartTime = Time.time;
    }

    // Called when a select action ends; triggers a click if press duration is short
    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (Time.time - pressStartTime < clickThreshold)
        {
            TryClickCountry();
        }
    }

    // Attempts to select a country under any active ray and trigger focus logic
    private void TryClickCountry()
    {
        foreach (var ray in rays)
        {
            // Check current raycast hit and verify the object is a country
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

    // Detects hover enter and exit events for countries using raycasts
    private void DetectHover()
    {
        // Reset current hover before performing ray checks
        currentHover = null;

        // Iterate through rays to find the first country being pointed at
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

        // Handle hover exit when no country is currently hovered
        if (currentHover == null)
        {
            if (lastHover != null)
            {
                controller.HoverExitCountry(lastHover);
                lastHover = null;
            }
            return;
        }

        // Handle hover transition between different countries
        if (currentHover != lastHover)
        {
            if (lastHover != null)
                controller.HoverExitCountry(lastHover);

            controller.HoverEnterCountry(currentHover);
            lastHover = currentHover;
        }
    }

}
