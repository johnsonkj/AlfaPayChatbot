using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Collections.Generic;

public class OpenAITTS : MonoBehaviour
{
    public static OpenAITTS Instance;

    [Header("OpenAI Settings")]
    public string openAIKey;
    public AudioSource audioSource;

    public AnimationManager animManager;

    // Track the current TTS request to cancel if needed
    private Coroutine currentTTSRequest = null;
    private UnityWebRequest activeRequest = null;

    // OpenAI tts-1 model voices
    private Dictionary<string, string> voiceMap = new Dictionary<string, string>
    {
        { "en", "onyx" },     // English
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
        public string response_format;
    }

    void Awake()
    {
        //openAIKey = "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";
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void SpeakText(string text)
    {
        // Cancel any existing TTS request
        CancelCurrentTTS();
        // Start a new TTS request
        currentTTSRequest = StartCoroutine(SendTextToTTS(text));
    }

    /// <summary>
    /// Cancels any currently playing audio and in-progress TTS requests
    /// </summary>
    public void CancelCurrentTTS()
    {
        // Stop any audio that's currently playing
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        animManager.TriggerCharlieTalkAnimation(false);
        // Cancel any in-progress web request
        if (activeRequest != null)
        {
            activeRequest.Abort();
            activeRequest = null;
        }

        // Stop the coroutine if it's running
        if (currentTTSRequest != null)
        {
            StopCoroutine(currentTTSRequest);
            currentTTSRequest = null;
        }

        Debug.Log("Canceled previous TTS request");
    }

    public float EstimateSpeechDuration(string text)
    {
        int wordCount = text.Split(' ').Length;

        float wordsPerSecond = 2.5f; // average speaking rate
        return wordCount / wordsPerSecond;
    }

    IEnumerator SendTextToTTS(string inputText)
    {
        Debug.Log("Sending text to OpenAI TTS: " + inputText);
        Debug.Log("Audio duration " + EstimateSpeechDuration(inputText));
        string uri = "https://api.openai.com/v1/audio/speech";

        string lang = LanguageSelector.selectedLanguageCode;
        string voice = voiceMap.ContainsKey(lang) ? voiceMap[lang] : "alloy"; // default fallback

        // Prepare the request object
        TTSRequest ttsRequest = new TTSRequest
        {
            model = "tts-1",
            input = inputText,
            voice = voice,
            response_format = "mp3"
        };

        string jsonBody = JsonUtility.ToJson(ttsRequest);

        using (UnityWebRequest www = new UnityWebRequest(uri, "POST"))
        {
            // Store reference to the active request so we can cancel it if needed
            activeRequest = www;

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            www.SetRequestHeader("Authorization", $"Bearer {openAIKey}");
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            // If this request was aborted, exit early
            if (www.result == UnityWebRequest.Result.ConnectionError && www.error.Contains("aborted"))
            {
                Debug.Log("TTS request was aborted");
                activeRequest = null;
                yield break;
            }

            // Clear the active request reference
            activeRequest = null;

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("OpenAI TTS Error: " + www.error);
            }
            else
            {
                Debug.Log("TTS audio received");
                byte[] mp3Data = www.downloadHandler.data;

#if UNITY_WEBGL && !UNITY_EDITOR
                PlayAudioInWebGL(mp3Data);
                animManager.TriggerCharlieTalkAnimation(true);
                float duration = EstimateSpeechDuration(inputText);
                StartCoroutine(StopTalkingAfterDelay(duration));
#else
                StartCoroutine(PlayMp3Fallback(mp3Data));
#endif
            }
        }
    }

    IEnumerator StopTalkingAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        animManager.TriggerCharlieTalkAnimation(false);
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL specific method using JavaScript interop
    void PlayAudioInWebGL(byte[] audioData)
    {
        string base64Audio = System.Convert.ToBase64String(audioData);
        PlayAudioFromBase64JS(base64Audio);
    }

    // JavaScript function to play audio in browser
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void PlayAudioFromBase64JS(string base64Data);
#endif


    // Fallback method for Android/iOS/Desktop
    IEnumerator PlayMp3Fallback(byte[] data)
    {
        // Check if we've been canceled before playing
        if (currentTTSRequest == null)
        {
            yield break;
        }

        string tempPath = Path.Combine(Application.persistentDataPath, "speech.mp3");
        File.WriteAllBytes(tempPath, data);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            // Check if we've been canceled before playing
            if (currentTTSRequest == null)
            {
                yield break;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                animManager.TriggerCharlieTalkAnimation(true);
                audioSource.Play();
                //animManager.EnableTalkSequence();

                yield return new WaitForSeconds(clip.length); // Wait for audio to finish playing
                animManager.TriggerCharlieTalkAnimation(false);
                //animManager.ResetTalk();
            }
            else
            {
                Debug.LogError("Failed to load MP3 audio: " + www.error);
            }
        }
    }
}