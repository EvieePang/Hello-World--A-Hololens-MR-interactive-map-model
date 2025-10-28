using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit.UX;
using UnityEngine;

public class ContextMenuSpawner : MonoBehaviour
{
    [Header("Refs")]
    public GameObject menuPrefab;            
    public EarthLayerSwitcher earth;         
    public Transform uiRoot;                 
    public LayerMask earthMask;              
    public Transform xrCamera;               

    private static GameObject activeMenu;
    [SerializeField] private LayerMask countryMask; // only include countries
    [SerializeField] private LayerMask globeMask;   // only include EarthLayer (Sphere/Mesh Collider)

    // XRSimpleInteractable.SelectEntered Event activate
    public void ShowMenu()
    {
        if (!menuPrefab || !earth) return;
        Debug.Log("[Spawner] ShowMenu() called!");
        if (activeMenu) Destroy(activeMenu);

        activeMenu = Instantiate(menuPrefab, uiRoot ? uiRoot : null);
        activeMenu.SetActive(true); //  «ø÷∆∆Ù”√

        var menu = activeMenu.GetComponent<LayerMenu>();
        if (menu == null) menu = activeMenu.GetComponentInChildren<LayerMenu>(true);
        if (menu)
        {
            menu.target = earth;
            Debug.Log($"[ContextMenuSpawner] menu.target set to {earth.name}");
            menu.SetCountryName(gameObject.name);
            Debug.Log($"[ContextMenuSpawner] Country name set to {gameObject.name}");
        }
        else
        {
            Debug.LogError("[ContextMenuSpawner] LayerMenu component not found on spawned menu prefab.");
        }

        var follow = activeMenu.GetComponentInChildren<NonOccludingFollow>(true);
        if (xrCamera) follow.SetCamera(xrCamera);
        follow.occlusionMask = globeMask;   // consider to ward off the earth sphere

        Transform cam = xrCamera ? xrCamera : (Camera.main ? Camera.main.transform : null);
        if (!cam) return;

        // 1) collide country to make sure of direction and distance
        Vector3 dir = (transform.position - cam.position).normalized;
        Ray ray1 = new Ray(cam.position, dir);

        if (Physics.Raycast(ray1, out var hitCountry, 200f, countryMask, QueryTriggerInteraction.Collide))
        {
            // 2) do Raycast to earth sphere to get sphere's normal line
            float maxDist = hitCountry.distance + 0.5f;
            if (Physics.Raycast(ray1, out var hitGlobe, maxDist, globeMask, QueryTriggerInteraction.Collide))
            {
                follow.PlaceAt(hitGlobe.point, hitGlobe.normal); 
            }
            else
            {
                Vector3 center = earth.transform.position;
                Vector3 n = (hitCountry.point - center).normalized;
                follow.PlaceAt(hitCountry.point, n);
            }
        }
        else
        {
            // directly to toward the pivot direction of this country to send a line to the earth and obtain the normal line (when user tap oceans)
            if (Physics.Raycast(ray1, out var hitGlobe, 200f, globeMask, QueryTriggerInteraction.Collide))
                follow.PlaceAt(hitGlobe.point, hitGlobe.normal);
        }
    }

    public void HideMenu()
    {
        if (activeMenu) Destroy(activeMenu);
    }
}
