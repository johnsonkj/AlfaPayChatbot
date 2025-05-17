using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public GPTQuery gptQuery;
    public TextMeshProUGUI gptAnswer;
    public TextMeshProUGUI speechOutput;
    public TextMeshProUGUI gptAnswerTextpad;
    public AudioSource audioSource;

    public void SubmitToGPTQuery()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            gptQuery.AskQuestion(inputField.text);
            Debug.Log("Submitted following question : " + inputField.text + " to GPT");
        }
        else
        {
            Debug.Log("Input Field is empty.");
        }
    }

    public void ClearText()
    {
        gptAnswer.text = "";
        speechOutput.text = "";
        gptAnswerTextpad.text = "";
    }

    public void StopAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

}
