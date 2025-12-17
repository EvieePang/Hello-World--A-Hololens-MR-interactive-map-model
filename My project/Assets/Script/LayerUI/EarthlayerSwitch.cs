using UnityEngine;

public class EarthLayerSwitcher : MonoBehaviour
{
    public Material[] layerMaterials;
    public Renderer earthRenderer;

    int current = -1;

    private void Reset() => earthRenderer = GetComponent<Renderer>();

    public void SetLayer(int index)
    {
        Debug.Log($"[EarthLayerSwitcher] try to change to {index}");
        if (earthRenderer == null || layerMaterials == null || layerMaterials.Length == 0) return;
        index = Mathf.Clamp(index, 0, layerMaterials.Length - 1);
        if (current == index) return;

        var mats = earthRenderer.sharedMaterials;
        if (mats == null || mats.Length == 0)
            earthRenderer.sharedMaterial = layerMaterials[index];
        else { mats[0] = layerMaterials[index]; earthRenderer.sharedMaterials = mats; }
        current = index;
    }
}
