using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json.Linq;
using TMPro;

public class GPTQuery : MonoBehaviour
{
    public string openAIKey;
    public TextMeshProUGUI GPTanswer;
    public TextMeshProUGUI gptanswerTextPad;

    private void Awake()
    {
        // ⚠️ In production WebGL, do NOT expose API key in client
        // openAIKey = KeyManager.GetApiKey();
    }

    public void AskQuestion(string question)
    {
        Debug.Log("User question: " + question);
        StartCoroutine(SendToGPT(question));
    }

    IEnumerator SendToGPT(string userQuestion)
    {
        string apiUrl = "https://api.openai.com/v1/responses";

        string language = GetLanguageName(LanguageSelector.selectedLanguageCode);

        // ✅ SIMPLE & RELIABLE FORMAT (matches Postman)
        JObject jsonBody = new JObject
        {
            ["model"] = "gpt-4.1",

            ["input"] =
            $@"Answer the following question in the speaking style of Charlie Kirk.
Speak confidently, directly, and conversationally like a political commentator.
Do NOT say you are an AI or roleplaying.
Respond ONLY in {language}.

Question: {userQuestion}"
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

            // 🔊 Send to Text-to-Speech
            OpenAITTS.Instance.SpeakText(reply);
        }
    }

    // ✅ Correct parsing for Responses API
    string ExtractReply(string json)
    {
        try
        {
            JObject jObject = JObject.Parse(json);
            return jObject["output"][0]["content"][0]["text"].ToString();
        }
        catch
        {
            Debug.LogError("Error parsing GPT response.");
            return "Sorry, I couldn't understand that.";
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