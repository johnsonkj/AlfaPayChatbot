using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Collections.Generic;

public class OpenAITTS : MonoBehaviour
{
    public static OpenAITTS Instance;

    [Header("OpenAI Settings")]
    public string openAIKey = KeyManager.GetApiKey();
    public AudioSource audioSource;

    // OpenAI tts-1 model voices
    private Dictionary<string, string> voiceMap = new Dictionary<string, string>
    {
        { "en", "alloy" },     // English
        { "hi", "echo" },      // Hindi (closest match)
        { "ar", "fable" },     // Arabic (closest match)
        { "zh", "onyx" },      // Mandarin (closest match)
        { "ru", "shimmer" }    // Russian (closest match)
    };

    [System.Serializable]
    public class TTSRequest
    {
        public string model;
        public string input;
        public string voice;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void SpeakText(string text)
    {
        StartCoroutine(SendTextToTTS(text));
    }

    IEnumerator SendTextToTTS(string inputText)
    {
        Debug.Log("Sending text to OpenAI TTS: " + inputText);
        string uri = "https://api.openai.com/v1/audio/speech";

        string lang = LanguageSelector.selectedLanguageCode;
        string voice = voiceMap.ContainsKey(lang) ? voiceMap[lang] : "alloy"; // default fallback

        // Prepare the request object
        TTSRequest ttsRequest = new TTSRequest
        {
            model = "tts-1",
            input = inputText,
            voice = voice
        };

        string jsonBody = JsonUtility.ToJson(ttsRequest);

        using (UnityWebRequest www = new UnityWebRequest(uri, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            www.SetRequestHeader("Authorization", $"Bearer {openAIKey}");
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("OpenAI TTS Error: " + www.error);
            }
            else
            {
                Debug.Log("TTS audio received");
                byte[] mp3Data = www.downloadHandler.data;
                StartCoroutine(PlayMp3(mp3Data));
            }
        }
    }

    IEnumerator PlayMp3(byte[] data)
    {
        string tempPath = Path.Combine(Application.persistentDataPath, "speech.mp3");
        File.WriteAllBytes(tempPath, data);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Failed to load MP3 audio: " + www.error);
            }
        }
    }
}
