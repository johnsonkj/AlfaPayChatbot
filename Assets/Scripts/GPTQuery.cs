using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json.Linq;
using TMPro;

public class GPTQuery : MonoBehaviour
{
    public string openAIKey;
    public OpenAITTS tts;
    public TextMeshProUGUI GPTanswer;
    public TextMeshProUGUI gptanswerTextPad;

    [TextArea(15, 30)]
    public string predefinedContent = ""; // All predefined content pasted here

    private void Awake()
    {
        //openAIKey = KeyManager.GetApiKey();
      //  openAIKey = "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";
    }

    public void AskQuestion(string question)
    {
        string userLanguageCode = LanguageSelector.selectedLanguageCode;
        Debug.Log($"Processing question in {GetLanguageName(userLanguageCode)}: {question}");

        // Simple prompt for GPT to find answer in the predefined content
        string prompt = $@"
You are a helpful assistant for AlfaPay, a financial app by Al Fardan Exchange. 

TASK:
1. The user has asked: {question}
2.Check if an answer exists in the predefined content below.
3.If you find an answer in the section for { GetLanguageName(userLanguageCode)}, respond with EXACTLY that answer.
4.If you don't find a match in {GetLanguageName(userLanguageCode)} but find it in ENGLISH, translate the English answer to {GetLanguageName(userLanguageCode)}.
5.If no answer exists in any language section, briefly answer based on your knowledge of financial apps.
6.Respond ONLY in { GetLanguageName(userLanguageCode)}.
7.Do not include phrases like Based on the content or explain your process.

PREDEFINED CONTENT: {predefinedContent} ";

        StartCoroutine(SendToGPT(prompt));
    }

    IEnumerator SendToGPT(string prompt)
    {
        string apiUrl = "https://api.openai.com/v1/chat/completions";

        JObject jsonBody = new JObject
        {
            ["model"] = "gpt-4",
            ["temperature"] = 0.2, // Lower temperature for more deterministic responses
            ["messages"] = new JArray
            {
                new JObject
                {
                    ["role"] = "user",
                    ["content"] = prompt
                }
            }
        };

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody.ToString());

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {openAIKey}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("GPT Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
        else
        {
            string reply = ExtractReply(request.downloadHandler.text);
            GPTanswer.text = reply;
            gptanswerTextPad.text = reply;
            Debug.Log("GPT Answer: " + reply);
            OpenAITTS.Instance.SpeakText(reply);
        }
    }

    string ExtractReply(string json)
    {
        try
        {
            JObject jObject = JObject.Parse(json);
            return jObject["choices"][0]["message"]["content"].ToString();
        }
        catch
        {
            return "Error parsing GPT response.";
        }
    }

    string GetLanguageName(string code)
    {
        switch (code)
        {
            case "hi": return "Hindi";
            case "ar": return "Arabic";
            case "zh": return "Mandarin";
            case "ru": return "Russian";
            case "fr": return "French";
            case "en":
            default: return "English";
        }
    }
}