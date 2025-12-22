using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Collects the names of all child GameObjects and exports them to a JSON file.
public class GetChildNamesToJson : MonoBehaviour
{
    // Serializable container for storing country names
    [System.Serializable]
    public class CountryList
    {
        public List<string> countries = new List<string>();
    }

    // Called on start: gathers child names and writes them to a JSON file in Assets/Resources
    void Start()
    {
        CountryList data = new CountryList();

        // Iterate through all child transforms and record their names
        foreach (Transform child in transform)
        {
            data.countries.Add(child.name);
        }

        // Convert data to formatted JSON
        string json = JsonUtility.ToJson(data, true);

        // Save JSON to Assets/Resources/countries.json
        string folder = Path.Combine(Application.dataPath, "Resources");
        string path = Path.Combine(folder, "countries.json");

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(path, json);

        Debug.Log("Country names have been saved to: " + path);
    }
}
