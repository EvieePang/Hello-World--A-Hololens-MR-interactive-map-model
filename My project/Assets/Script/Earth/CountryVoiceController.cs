using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;

public class CountryVoiceController : MonoBehaviour
{
    private KeywordRecognizer recognizer;

    public CountryClickController clickController;
    public Transform countriesParent; // make it same with clickController

    void Start()
    {
        // automatically generate the keywords list from country GameObject
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
            // KeywordRecognizer to distinguish upper or mixed case
            names.Add(c.name);
        }
        return names.ToArray();
    }

    private void OnRecognized(PhraseRecognizedEventArgs args)
    {
        string spoken = args.text;


        // directly return the country name recongnized to the function
        clickController.FocusCountryByName(spoken);
    }
}
