using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;

public class CountryVoiceController : MonoBehaviour
{
    private KeywordRecognizer recognizer;

    public CountryClickController clickController;
    public Transform countriesParent; // 让它和你的 clickController 的一致

    void Start()
    {
        // 自动从国家 GameObject 里生成关键词列表（最重要！）
        string[] countryNames = CollectCountryNames();

        recognizer = new KeywordRecognizer(countryNames);
        recognizer.OnPhraseRecognized += OnRecognized;
        recognizer.Start();

        Debug.Log("[Voice] Started KeywordRecognizer with " + countryNames.Length + " countries.");
    }

    private string[] CollectCountryNames()
    {
        List<string> names = new List<string>();

        foreach (Transform c in countriesParent)
        {
            // KeywordRecognizer 区分大小写，用真实名字即可
            names.Add(c.name);
        }
        return names.ToArray();
    }

    private void OnRecognized(PhraseRecognizedEventArgs args)
    {
        string spoken = args.text;

        Debug.Log("[Voice] Recognized: " + spoken);

        // 直接把识别到的国家名字传进你的函数
        clickController.FocusCountryByName(spoken);
    }
}
