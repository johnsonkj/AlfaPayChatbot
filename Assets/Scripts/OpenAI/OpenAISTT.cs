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

    // Event to notify when transcription is complete
    public event Action<string> OnTranscriptionComplete;

    void Awake()
    {
        //openAIKey = KeyManager.GetApiKey();
       // openAIKey = "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";

        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ProcessAudioClip(AudioClip clip)
    {
        StartCoroutine(SendAudioToWhisper(clip));
    }

    // New method to process audio bytes directly (for WebGL)
    public void ProcessAudioBytes(byte[] audioData)
    {
        Debug.Log($"Processing audio bytes, size: {audioData.Length} bytes");
        StartCoroutine(SendAudioBytesToWhisper(audioData));
    }

    IEnumerator SendAudioToWhisper(AudioClip clip)
    {
        Debug.Log("Converting AudioClip to WAV...");
        byte[] wavData = SaveWav.ToBytes(clip);
        if (wavData == null)
        {
            Debug.LogError("WAV conversion failed.");
            yield break;
        }

        yield return StartCoroutine(SendAudioBytesToWhisper(wavData));
    }

    // Separate method to handle sending bytes to the API
    IEnumerator SendAudioBytesToWhisper(byte[] audioData)
    {
        var form = new WWWForm();
        form.AddBinaryData("file", audioData, "recording.wav", "audio/wav");
        form.AddField("model", whisperModel);
        // Always translate audio into English
        form.AddField("language", "en");

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

                var response = JsonConvert.DeserializeObject<WhisperResponse>(responseJson);
                string transcript = response.text;
                recognizedSpeech.text = transcript;

                // Invoke the event
                OnTranscriptionComplete?.Invoke(transcript);

                // Send to GPT for processing
                gptQuery.AskQuestion(transcript);
            }
        }
    }

    [Serializable]
    private class WhisperResponse
    {
        public string text;
    }
}