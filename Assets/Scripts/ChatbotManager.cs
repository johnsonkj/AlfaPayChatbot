using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatbotManager : MonoBehaviour
{
    public Animator chatbotAnimator;
    public AudioSource audioSource;
    public AudioClip chatbotGreetAudioClip;

    private void Start()
    {
        StartCoroutine(TriggerChatbotGreeting());
    }

    IEnumerator TriggerChatbotGreeting()
    {
        chatbotAnimator.SetTrigger("greet");
        if (audioSource != null && chatbotGreetAudioClip != null)
        {
            yield return new WaitForSeconds(0.5f);
            audioSource.PlayOneShot(chatbotGreetAudioClip);
        }
      
        Invoke("StopGreetAnimation", chatbotGreetAudioClip.length);
    }

    void StopGreetAnimation()
    {
        chatbotAnimator.SetTrigger("idle");
    }
}
