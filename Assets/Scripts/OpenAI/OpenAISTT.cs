using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using System;
using TMPro;
using Newtonsoft.Json;

public class OpenAISTT : MonoBehaviour
{
    public static OpenAISTT Instance;
    public GPTQuery gptQuery;

    [Header("OpenAI Settings")]
    public string openAIKey;
    public string whisperModel = "whisper-1";
    public TextMeshProUGUI recognizedSpeech;

    void Awake()
    {
        openAIKey = KeyManager.GetApiKey();
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ProcessAudioClip(AudioClip clip)
    {
        
        StartCoroutine(SendAudioToWhisper(clip));
    }

    IEnumerator SendAudioToWhisper(AudioClip clip)
    {
        Debug.Log("Converting AudioClip to WAV...");
        string selectedLanguageCode = LanguageSelector.selectedLanguageCode;
        byte[] wavData = SaveWav.ToBytes(clip);
        if (wavData == null)
        {
            Debug.LogError("WAV conversion failed.");
            yield break;
        }

        var form = new WWWForm();
        form.AddBinaryData("file", wavData, "recording.wav", "audio/wav");
        form.AddField("model", whisperModel);

        // Always translate audio into English
        form.AddField("language", "en");

       /* // Optional: specify language to improve accuracy
        string selectedLanguageCode = LanguageSelector.selectedLanguageCode;
        if (!string.IsNullOrEmpty(selectedLanguageCode))
        {
            form.AddField("language", selectedLanguageCode); // e.g., "hi" for Hindi
        }*/

        using (UnityWebRequest www = UnityWebRequest.Post("https://api.openai.com/v1/audio/translations", form))
        {
            www.SetRequestHeader("Authorization", $"Bearer {openAIKey}");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Whisper STT Error: " + www.error);
            }
            else
            {
                var responseJson = www.downloadHandler.text;
                Debug.Log("Whisper response: " + responseJson);
                //string transcript = JsonUtility.FromJson<WhisperResponse>(responseJson).text;
                //recognizedSpeech.text = transcript;
                var response = JsonConvert.DeserializeObject<WhisperResponse>(responseJson);
                string transcript = response.text;
                recognizedSpeech.text = transcript;

                // Instead of speaking it directly, send it to GPT for processing
                gptQuery.AskQuestionInEnglish(transcript);
            }
        }
    }

    [Serializable]
    private class WhisperResponse
    {
        public string text;
    }
}
