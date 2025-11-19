using UnityEngine;

public class RootMenuExit : MonoBehaviour
{
    private ContextMenuSpawner contextMenuSpawner;
    private CountryClickController countryClickController;

    [Header("Optional: Reset Earth Material")]
    public MeshRenderer earthRenderer;       // Earth layer
    public Material defaultEarthMaterial;    // Earth water

    // initialization ( ContextMenuSpawner)
    public void Init(ContextMenuSpawner spawner, CountryClickController controller)
    {
        contextMenuSpawner = spawner;
        countryClickController = controller;
        // auto find EarthLayers as MeshRenderer
        var earthLayerObj = GameObject.Find("EarthLayers");
        if (earthLayerObj)
            earthRenderer = earthLayerObj.GetComponent<MeshRenderer>();
    }

    public void OnClickExitRootMenu()
    {
        Debug.Log("[RootMenuExit] Exit clicked ¡ú Hide Root Menu + Reset Country + Reset EarthLayer");

        //  hidden the menu
        if (contextMenuSpawner)
            contextMenuSpawner.HideMenu();

        // recover country showed
        if (countryClickController)
            countryClickController.ClearHighlight();

        // recover to original base material
        if (earthRenderer && defaultEarthMaterial)
        {
            earthRenderer.sharedMaterial = defaultEarthMaterial;
            Debug.Log("[RootMenuExit] Earth material reset to default.");
        }
        else
        {
            Debug.LogWarning("[RootMenuExit] Missing earthRenderer or defaultEarthMaterial reference.");
        }
    }
}
