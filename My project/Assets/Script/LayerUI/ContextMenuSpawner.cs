<<<<<<< HEAD
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
        activeMenu.SetActive(true); //  force to start

        var exit = activeMenu.GetComponentInChildren<RootMenuExit>(true);
        if (exit)
        {
            exit.Init(this, FindObjectOfType<CountryClickController>());
            Debug.Log("[ContextMenuSpawner] RootMenuExit references initialized.");
        }

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
=======
﻿using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.Subsystems;
using MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.XR;

public class ContextMenuSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject menuPrefab;
    public EarthLayerSwitcher earth;
    public CountryClickController countryController;
    public Transform uiRoot;

    private GameObject activeMenu;
    private NonOccludingFollow follow;

    private bool wasLeftHandVisible = false;
    private bool waitingForHand = false;

    // 自动寻找 CountryClickController
    void Start()
    {
        if (countryController == null)
        {
            countryController = FindObjectOfType<CountryClickController>();
            if (countryController)
                Debug.Log($"[Init] Auto-assigned CountryClickController: {countryController.name}");
            else
                Debug.LogWarning("[Init] No CountryClickController found in scene!");
        }
    }

    // 检测左手可见性
    private bool IsLeftHandVisible()
    {
        var handsSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();
        if (handsSubsystem == null)
        {
            Debug.Log("[HandCheck] HandsAggregatorSubsystem == null");
            return false;
        }

        bool hasPalm = handsSubsystem.TryGetJoint(TrackedHandJoint.Palm, XRNode.LeftHand, out HandJointPose palmPose);
        bool isTracked = handsSubsystem.TryGetEntireHand(XRNode.LeftHand, out var leftHand);
        bool palmValid = hasPalm && palmPose.Position.sqrMagnitude > 0.0001f;

        bool result = hasPalm && isTracked && palmValid;
        Debug.Log($"[HandCheck] hasPalm={hasPalm}, isTracked={isTracked}, palmValid={palmValid}, result={result}, palmPos={palmPose.Position}");
        return result;
    }

    void Update()
    {
        if (!countryController) return;

        bool anyActive = countryController.isAnyCountryActive;
        bool leftHandVisible = IsLeftHandVisible();

        // Step 1️⃣ 点击国家后 → 开始等待手出现，但要记录当前手状态，避免立刻触发
        if (anyActive && activeMenu == null && !waitingForHand)
        {
            wasLeftHandVisible = leftHandVisible;     // 记录当前状态
            waitingForHand = true;
            Debug.Log($"[State] Country activated → Start waiting for left hand. (initial wasLeftHandVisible={wasLeftHandVisible})");
        }

        // Step 2️⃣ 检测左手首次出现（从 false → true）
        if (waitingForHand && leftHandVisible && !wasLeftHandVisible && activeMenu == null)
        {
            Debug.Log("[Trigger] Left hand first detected → ShowMenu()");
            ShowMenu();
            waitingForHand = false;
        }

        wasLeftHandVisible = leftHandVisible;
    }

    // 菜单生成逻辑
    private void ShowMenu()
    {
        if (!menuPrefab)
        {
            Debug.LogWarning("[ContextMenuSpawner] No menuPrefab assigned!");
            return;
        }

        if (activeMenu != null)
        {
            Debug.LogWarning("[ContextMenuSpawner] Menu already exists, skipping spawn.");
            return;
        }

        activeMenu = Instantiate(menuPrefab, uiRoot ? uiRoot : null);
        activeMenu.SetActive(true);

        // 可捏取组件
        if (activeMenu.GetComponent<ObjectManipulator>() == null)
            activeMenu.AddComponent<ObjectManipulator>();
        if (activeMenu.GetComponent<BoxCollider>() == null)
        {
            var col = activeMenu.AddComponent<BoxCollider>();
            col.size = new Vector3(0.2f, 0.2f, 0.02f);
        }

        // 绑定 LayerMenu
        var menu = activeMenu.GetComponentInChildren<LayerMenu>(true);
        if (menu)
        {
            if (!earth)
                earth = FindObjectOfType<EarthLayerSwitcher>();
            menu.target = earth;
            menu.SetCountryName(gameObject.name);
        }

        // 绑定 Exit
        var exit = activeMenu.GetComponentInChildren<RootMenuExit>(true);
        if (exit)
            exit.InjectEarthReference(earth);

        // 初次放置在手边
        follow = activeMenu.GetComponentInChildren<NonOccludingFollow>(true);
        if (follow)
            follow.ResetFollowToHand();

        Debug.Log("[ShowMenu] Menu placed near left hand and locked.");
    }

    // 菜单销毁逻辑
    public void HideMenu()
    {
        // 无论 activeMenu 存不存在，都 reset 状态
        waitingForHand = false;
        wasLeftHandVisible = false;
        countryController.isAnyCountryActive = false;

        if (activeMenu != null)
        {
            Destroy(activeMenu);
            activeMenu = null;
            follow = null;
        }

        Debug.Log("[ContextMenuSpawner] Full reset completed.");
>>>>>>> earth_menu_lefthand0
    }
}
