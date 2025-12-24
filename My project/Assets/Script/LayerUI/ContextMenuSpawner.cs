using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.Subsystems;
using MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.XR.OpenXR.Features.Interactions.PalmPoseInteraction;

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

    // automatically find CountryClickController
    void Start()
    {
        if (countryController == null)
        {
            countryController = FindObjectOfType<CountryClickController>();
         
        }
    }

    // detect left hand
    private bool IsLeftHandVisible()
    {
        var handsSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();
        //if (handsSubsystem == null)
        //{
        //    Debug.Log("[HandCheck] HandsAggregatorSubsystem == null");
        //    return false;
        //}

        bool hasPalm = handsSubsystem.TryGetJoint(TrackedHandJoint.Palm, XRNode.LeftHand, out HandJointPose palmPose);
        bool isTracked = handsSubsystem.TryGetEntireHand(XRNode.LeftHand, out var leftHand);
        bool palmValid = hasPalm && palmPose.Position.sqrMagnitude > 0.0001f;

        bool result = hasPalm && isTracked && palmValid;
        //Debug.Log($"[HandCheck] hasPalm={hasPalm}, isTracked={isTracked}, palmValid={palmValid}, result={result}, palmPos={palmPose.Position}");
        return result;
    }

    void Update()
    {
        if (!countryController) return;

        bool anyActive = countryController.isAnyCountryActive;
        bool leftHandVisible = IsLeftHandVisible();

        // select country ,then wait for left hand and record current hand state to avoid immediately trigger on
        if (anyActive && activeMenu == null && !waitingForHand)
        {
            wasLeftHandVisible = leftHandVisible;     // record current state
            waitingForHand = true;
        }

        // detect the first show up of left hand 
        if (waitingForHand && leftHandVisible && !wasLeftHandVisible && activeMenu == null)
        {
            ShowMenu();
            waitingForHand = false;
        }

        wasLeftHandVisible = leftHandVisible;
    }

    // Menu show up 
    private void ShowMenu()
    {
        activeMenu = Instantiate(menuPrefab, uiRoot ? uiRoot : null);
        activeMenu.SetActive(true);

        // interactable components
        //if (activeMenu.GetComponent<ObjectManipulator>() == null)
        //    activeMenu.AddComponent<ObjectManipulator>();
        //if (activeMenu.GetComponent<BoxCollider>() == null)
        //{
        //    var col = activeMenu.AddComponent<BoxCollider>();
        //    col.size = new Vector3(0.2f, 0.2f, 0.02f);
        //}

        // binding to LayerMenu
        var menu = activeMenu.GetComponentInChildren<LayerMenu>(true);
        if (menu)
        {
            if (!earth)
                earth = FindObjectOfType<EarthLayerSwitcher>();
            menu.target = earth;
            menu.SetCountryName(CountryClickController.selectedCountry);
        }

        //  binding to Exit button
        var exit = activeMenu.GetComponentInChildren<RootMenuExit>(true);
        if (exit)
            exit.InjectEarthReference(earth);

        // initially put beside the hands
        follow = activeMenu.GetComponentInChildren<NonOccludingFollow>(true);
        if (follow)
            follow.ResetFollowToHand();

    }

    // Menu destroy logic
    public void HideMenu()
    {
        // no matter the activeMenu exist or not, to reset state
        waitingForHand = false;
        wasLeftHandVisible = false;
        countryController.isAnyCountryActive = false;

        if (activeMenu != null)
        {
            Destroy(activeMenu);
            activeMenu = null;
            follow = null;
        }

    }
}
