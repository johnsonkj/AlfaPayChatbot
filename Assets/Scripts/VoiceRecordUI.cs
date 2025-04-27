using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using System.IO;

public class VoiceRecordUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RectTransform micButton;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI slideToCancelText;
    public AudioSource audioSource;

    private float holdTime;
    private bool isRecording = false;
    private Coroutine timerCoroutine;

    private Vector2 initialTouchPosition;

#if !UNITY_WEBGL
    private AudioClip recordedClip;
    private const int sampleRate = 16000;
    private string micDevice;
#endif

    void Start()
    {
        timerText.gameObject.SetActive(false);
        slideToCancelText.gameObject.SetActive(false);

#if !UNITY_WEBGL
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
        }
        else
        {
            Debug.LogWarning("No microphone devices found.");
        }
#endif
    }

    public void OnPointerDown(PointerEventData eventData)
    {
#if !UNITY_WEBGL
        if (micDevice == null) return;

        initialTouchPosition = eventData.position;
        isRecording = true;

        OpenAISTT.Instance.recognizedSpeech.text = "";
        micButton.LeanScale(Vector3.one * 1.4f, 0.2f).setEaseOutBack();

        timerText.gameObject.SetActive(true);
        slideToCancelText.gameObject.SetActive(true);
        timerCoroutine = StartCoroutine(StartTimer());

        StartCoroutine(DelayedStartRecording()); // Delay recording slightly
#endif
    }

#if !UNITY_WEBGL
    private IEnumerator DelayedStartRecording()
    {
        yield return new WaitForSeconds(0.05f); // 50ms delay
        StartRecording();
    }
#endif

    public void OnPointerUp(PointerEventData eventData)
    {
#if !UNITY_WEBGL
        StopRecordingAndSend();
#endif
    }

    public void OnDrag(PointerEventData eventData)
    {
#if !UNITY_WEBGL
        float dragDistance = Vector2.Distance(eventData.position, initialTouchPosition);
        if (dragDistance > 100f)
        {
            CancelRecording();
        }
#endif
    }

#if !UNITY_WEBGL
    private void StartRecording()
    {
        recordedClip = Microphone.Start(micDevice, false, 30, sampleRate);
        Debug.Log("Recording started...");
    }

    private void StopRecordingAndSend()
    {
        if (!isRecording) return;
        isRecording = false;

        micButton.LeanScale(Vector3.one, 0.2f).setEaseOutBack();
        timerText.gameObject.SetActive(false);
        slideToCancelText.gameObject.SetActive(false);
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        if (Microphone.IsRecording(micDevice))
        {
            Microphone.End(micDevice);
            Debug.Log("Recording stopped.");

            if (recordedClip != null)
            {
                OpenAISTT.Instance.ProcessAudioClip(recordedClip);
            }
        }
    }

    private void CancelRecording()
    {
        Debug.Log("Recording cancelled.");
        if (Microphone.IsRecording(micDevice))
        {
            Microphone.End(micDevice);
        }
        isRecording = false;

        micButton.LeanScale(Vector3.one, 0.2f).setEaseOutBack();
        timerText.gameObject.SetActive(false);
        slideToCancelText.gameObject.SetActive(false);
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
    }
#endif

    private IEnumerator StartTimer()
    {
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime;
            int seconds = Mathf.FloorToInt(timer);
            timerText.text = "0:" + seconds.ToString("00");
            yield return null;
        }
    }
}
