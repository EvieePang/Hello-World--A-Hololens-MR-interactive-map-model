using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GetChildNamesToJson : MonoBehaviour
{
    [System.Serializable]
    public class CountryList
    {
        public List<string> countries = new List<string>();
    }

    void Start()
    {
        CountryList data = new CountryList();

        foreach (Transform child in transform)
        {
            data.countries.Add(child.name);
        }

        // 转成 JSON
        string json = JsonUtility.ToJson(data, true);

        // 保存路径（Assets/Resources/countries.json）
        string folder = Path.Combine(Application.dataPath, "Resources");
        string path = Path.Combine(folder, "countries.json");

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(path, json);

        Debug.Log("国家名字已保存到: " + path);
    }
}


