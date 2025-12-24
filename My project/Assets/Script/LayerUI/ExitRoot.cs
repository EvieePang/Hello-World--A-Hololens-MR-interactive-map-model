using UnityEngine;
using MixedReality.Toolkit.UX;

public class RootMenuExit : MonoBehaviour
{
    private ContextMenuSpawner contextMenuSpawner;
    private CountryClickController countryClickController;
    private EarthLayerSwitcher earthLayerSwitcher;
    [Header("Optional: Default material when exiting")]
    public Material defaultEarthMaterial;

    [Header("Optional: Button binding")]
    public PressableButton button;

    void Start()
    {
        if (button == null)
            button = GetComponent<PressableButton>();

        if (button)
            button.OnClicked.AddListener(OnExitClicked);

        if (!contextMenuSpawner)
            contextMenuSpawner = FindObjectOfType<ContextMenuSpawner>();

        if (!countryClickController)
            countryClickController = FindObjectOfType<CountryClickController>();
    }

    public void InjectEarthReference(EarthLayerSwitcher e)
    {
        earthLayerSwitcher = e;
        
    }


    public void OnExitClicked()
    {
        // recover country
        if (countryClickController)
        {
            countryClickController.ClearHighlight();
        }

        // hide menu
        if (contextMenuSpawner)
        {
            contextMenuSpawner.HideMenu();
        }

        // auto find the water material
        if (earthLayerSwitcher)
        {
            Renderer earthRenderer = earthLayerSwitcher.GetComponent<Renderer>();

            Material waterMat = Resources.Load<Material>("water");
            if (!waterMat)
            {
                foreach (var mat in Resources.FindObjectsOfTypeAll<Material>())
                {
                    if (mat.name.ToLower().Contains("water"))
                    {
                        waterMat = mat;
                        break;
                    }
                }
            }

            if (waterMat && earthRenderer)
            {
                earthRenderer.sharedMaterial = waterMat;
            }
        }

        //var follow = FindObjectOfType<NonOccludingFollow>();
        //if (follow)
        //{
        //    follow.ResetFollowToHand();
        //}
    }
}
