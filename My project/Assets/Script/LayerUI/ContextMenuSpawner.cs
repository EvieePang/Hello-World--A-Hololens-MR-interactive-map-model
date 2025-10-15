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

    // XRSimpleInteractable.SelectEntered Event activate
    public void ShowMenu()
    {
        Debug.Log("[ContextMenuSpawner] ShowMenu() ±»µ÷ÓÃ");
        if (!menuPrefab || !earth) return;

        // delete old one
        //if (activeMenu) Destroy(activeMenu);

        // come to world space
        activeMenu = Instantiate(menuPrefab, uiRoot ? uiRoot : null);

        // LayerMenu 
        var menu = activeMenu.GetComponentInChildren<LayerMenu>(true);
        if (menu) menu.target = earth;

        // set to follow the components (orientate to camera and avoid overlay)
        var follow = activeMenu.GetComponentInChildren<NonOccludingFollow>(true);
        if (follow)
        {
            
            if (xrCamera) follow.SetCamera(xrCamera);
            follow.occlusionMask = earthMask;

            Transform cam = xrCamera ? xrCamera : (Camera.main ? Camera.main.transform : null);
            if (cam)
            {
                Vector3 dir = (transform.position - cam.position).normalized;
                if (Physics.Raycast(cam.position, dir, out var hit, 50f, ~0, QueryTriggerInteraction.Collide))
                {
                    Vector3 normal = (hit.collider.transform == transform)
                        ? hit.normal
                        : (transform.position - earth.transform.position).normalized;

                    follow.PlaceAt(hit.point, normal);
                }
                else
                {
                    Vector3 fallbackPoint = transform.position;
                    Vector3 fallbackNormal = (transform.position - earth.transform.position).normalized;
                    follow.PlaceAt(fallbackPoint, fallbackNormal);
                }
            }
        }
    }

    public void HideMenu()
    {
        if (activeMenu) Destroy(activeMenu);
    }
}
