using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public GPTQuery gptQuery;
    public TextMeshProUGUI gptAnswer;
    public TextMeshProUGUI speechOutput;
    public TextMeshProUGUI gptAnswerTextpad;
    public AudioSource audioSource;
    public GameObject GPTAnswer;

    public Image HideAnswerArrowImage;   // UI Image to change
    public Sprite rightArrowSprite;      // e.g., 
    public Sprite downArrowSprite;

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

    public void OnToggleAnswerClick()
    { 
        GPTAnswer.SetActive(!GPTAnswer.activeSelf);
        HideAnswerArrowImage.sprite = GPTAnswer.activeSelf ? downArrowSprite: rightArrowSprite;
    }
}
