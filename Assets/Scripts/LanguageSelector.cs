using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LanguageSelector : MonoBehaviour
{
    public Button englishButton;
    public Button arabicButton;
    public Button hindiButton;
    public Button mandarinButton;
    public Button russianButton;

    public static string selectedLanguageCode { get; private set; }
    public delegate void LanguageSelectedEvent(string languageCode);
    public static event LanguageSelectedEvent OnLanguageSelected;

    void Start()
    {
        englishButton.onClick.AddListener(() => SetLanguage("en"));
        arabicButton.onClick.AddListener(() => SetLanguage("ar"));
        hindiButton.onClick.AddListener(() => SetLanguage("hi"));
        mandarinButton.onClick.AddListener(() => SetLanguage("zh")); // Corrected Mandarin code
        russianButton.onClick.AddListener(() => SetLanguage("ru")); // Corrected Russian code
    }

    void SetLanguage(string languageCode)
    {
        selectedLanguageCode = languageCode;
        Debug.Log("Selected Language: " + selectedLanguageCode);
        OnLanguageSelected?.Invoke(selectedLanguageCode);
        // Optionally, disable the language selection UI after selection
        gameObject.SetActive(false);
    }
}