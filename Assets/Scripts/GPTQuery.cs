using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json.Linq;
using TMPro;

public class GPTQuery : MonoBehaviour
{
    public string openAIKey = KeyManager.GetApiKey();
    public OpenAITTS tts;
    public TextMeshProUGUI GPTanswer;
    public TextMeshProUGUI gptanswerTextPad;

    [TextArea(5, 15)]
    public string predefinedContent = ""; // All predefined content pasted here

    private Dictionary<string, string> predefinedQnA = new Dictionary<string, string>()
    {
        { "tell me about alfapay", "AlfaPay is Al Fardan Exchange’s exclusive all-in-one digital financial app..." },
        { "tell me about yourself", "Think of me as your smart assistant..." },
        { "how can i send money with alfapay", "Sending money with AlfaPay is simple..." },
        { "can i send money to any country", "Yes! AlfaPay lets you send money to over 190 countries..." },
        { "what more can i do with alfapay", "AlfaPay goes beyond money transfers..." },
    };

    
    public void AskQuestionInEnglish(string englishQuestion)
    {
        string userLanguageCode = LanguageSelector.selectedLanguageCode;
        string match = FindPredefinedAnswer(englishQuestion.ToLower());
        Debug.Log("The language selected by user for GPT Query is " + GetLanguageName(userLanguageCode));
        if (!string.IsNullOrEmpty(match))
        {

            // Ask GPT to just translate it
            string prompt = $"Translate the following answer to {GetLanguageName(userLanguageCode)}:\n\n{match}";
            StartCoroutine(SendToGPT(prompt));
        }
        else
        {
            string prompt = $@"
You are a helpful assistant. Your task is to understand the meaning of the user's question and see if the answer exists in the provided content, even if the wording is different.

1. If the content already contains an answer that matches the meaning of the question, return **that answer**.
2. If the answer is not in the content, use your own knowledge to answer the question.
3. Return the answer translated into {GetLanguageName(userLanguageCode)}.

Question: {englishQuestion}

Predefined Content:
{predefinedContent}

Answer:";



            StartCoroutine(SendToGPT(prompt));
        }
    }

    string FindPredefinedAnswer(string question)
    {
        foreach (var entry in predefinedQnA)
        {
            if (question.Contains(entry.Key.ToLower()))
                return entry.Value;
        }
        return null;
    }

    IEnumerator SendToGPT(string prompt)
    {
        string apiUrl = "https://api.openai.com/v1/chat/completions";

        JObject jsonBody = new JObject
        {
            ["model"] = "gpt-4",
            ["temperature"] = 0.7,
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
            case "en":
            default: return "English";
        }
    }
}
