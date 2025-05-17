using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using System.IO;
using System;
using System.Text;

public class VoiceRecordUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RectTransform micButton;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI slideToCancelText;
    public TextMeshProUGUI GPTanswer;
    public AudioSource audioSource;

    private float holdTime;
    private bool isRecording = false;
    private Coroutine timerCoroutine;
    private Vector2 initialTouchPosition;
    private AudioClip recordedClip;
    private const int sampleRate = 16000;
    private string micDevice;

    private WebGLMicrophone webGLMic;
    private bool isWebGL;

    void Start()
    {
        timerText.gameObject.SetActive(false);
        slideToCancelText.gameObject.SetActive(false);

        // Check if we're running on WebGL
        isWebGL = false;
#if UNITY_WEBGL && !UNITY_EDITOR
        isWebGL = true;
#endif

        if (isWebGL)
        {
            SetupWebGLMicrophone();
        }
        else
        {
            SetupStandardMicrophone();
        }
    }

    private void SetupWebGLMicrophone()
    {
        // Find or create WebGLMicrophone instance
        webGLMic = FindObjectOfType<WebGLMicrophone>();
        if (webGLMic == null)
        {
            GameObject webGLMicObj = new GameObject("WebGLMicrophone");
            webGLMic = webGLMicObj.AddComponent<WebGLMicrophone>();
        }

        webGLMic.Initialize();

        // Set up event handlers
        webGLMic.PermissionGranted += () => Debug.Log("Microphone permission granted");
        webGLMic.PermissionDenied += () => Debug.LogError("Microphone permission denied");
        webGLMic.RecordingStarted += () => Debug.Log("WebGL recording started");
        webGLMic.RecordingError += () => Debug.LogError("WebGL recording error");
        webGLMic.RecordingComplete += HandleWebGLRecordingComplete;

        // Request permission
        webGLMic.RequestPermission();
    }

    private void SetupStandardMicrophone()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
        }
        else
        {
            Debug.LogWarning("No microphone devices found.");
        }
#else
    Debug.Log("Standard microphone not available in WebGL");
#endif
    }

    private void HandleWebGLRecordingComplete(string base64AudioData)
    {
        Debug.Log("WebGL recording complete, processing audio...");

        // Convert base64 to WAV
        byte[] audioData = Convert.FromBase64String(base64AudioData);

        // Now process the audio with your OpenAI STT
        OpenAISTT.Instance.ProcessAudioBytes(audioData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        initialTouchPosition = eventData.position;
        isRecording = true;

        if (OpenAISTT.Instance != null)
        {
            OpenAISTT.Instance.recognizedSpeech.text = "";
        }
        GPTanswer.text = "";
        micButton.LeanScale(Vector3.one * 1.4f, 0.2f).setEaseOutBack();
        timerText.gameObject.SetActive(true);
        slideToCancelText.gameObject.SetActive(true);
        timerCoroutine = StartCoroutine(StartTimer());

        StartCoroutine(DelayedStartRecording());
    }

    private IEnumerator DelayedStartRecording()
    {
        yield return new WaitForSeconds(0.05f); // 50ms delay
        StartRecording();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopRecordingAndSend();
    }

    public void OnDrag(PointerEventData eventData)
    {
        float dragDistance = Vector2.Distance(eventData.position, initialTouchPosition);
        if (dragDistance > 100f)
        {
            CancelRecording();
        }
    }

    private void StartRecording()
    {
        if (isWebGL)
        {
            webGLMic.StartMicrophoneRecording();
        }
        else
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            recordedClip = Microphone.Start(micDevice, false, 30, sampleRate);
#endif
        }
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

        if (isWebGL)
        {
            webGLMic.StopMicrophoneRecording();
        }
        else
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (Microphone.IsRecording(micDevice))
            {
                Microphone.End(micDevice);
                Debug.Log("Recording stopped.");
                if (recordedClip != null)
                {
                    OpenAISTT.Instance.ProcessAudioClip(recordedClip);
                }
            }
#endif
        }
    }

    private void CancelRecording()
    {
        Debug.Log("Recording cancelled.");

        if (isWebGL)
        {
            webGLMic.StopMicrophoneRecording();
        }
        else
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (Microphone.IsRecording(micDevice))
            {
                Microphone.End(micDevice);
            }
#endif
        }

        isRecording = false;
        micButton.LeanScale(Vector3.one, 0.2f).setEaseOutBack();
        timerText.gameObject.SetActive(false);
        slideToCancelText.gameObject.SetActive(false);

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
    }

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