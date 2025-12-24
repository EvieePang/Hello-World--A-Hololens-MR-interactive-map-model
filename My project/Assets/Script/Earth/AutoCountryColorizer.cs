using UnityEngine;

// Automatically assigns colors to all child country objects at runtime.
public class AutoCountryColorizer : MonoBehaviour
{
    // Unity lifecycle method: called once when the script starts
    void Start()
    {
        ColorizeAllCountries();
    }

    // Iterates through all child transforms and applies a uniform color to each country
    void ColorizeAllCountries()
    {
        // Loop through each country GameObject under this parent
        foreach (Transform country in transform)
        {
            Renderer renderer = country.GetComponent<Renderer>();
            if (renderer == null) continue;

            // Create a unique material instance to avoid modifying shared materials
            Material matInstance = renderer.material;
            matInstance.color = Color.white;
        }

    }

    // Generates a deterministic color based on the country name (not used in current logic)
    Color GenerateColorFromName(string name)
    {
        // Compute a hash value from the name to ensure consistent color generation
        int hash = Mathf.Abs(name.GetHashCode());

        // Map hash value to hue range [0,1]
        float hue = (hash % 360) / 360f;

        // Add small noise to avoid visually similar neighboring colors
        float noise = ((hash / 7) % 100) / 500f; 
        hue = Mathf.Repeat(hue + noise, 1f);

        // Use controlled saturation and brightness for a soft, readable appearance
        float saturation = 0.35f + (((hash / 13) % 10) / 100f); 
        float value = 0.92f; 

        return Color.HSVToRGB(hue, saturation, value);
    }
}
