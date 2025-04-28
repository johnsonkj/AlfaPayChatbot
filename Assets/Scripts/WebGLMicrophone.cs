using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class WebGLMicrophone : MonoBehaviour
{
    public static WebGLMicrophone Instance { get; private set; }

    // Events
    public event Action PermissionGranted;
    public event Action PermissionDenied;
    public event Action RecordingStarted;
    public event Action RecordingError;
    public event Action<string> RecordingComplete;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool InitMicrophone();
    
    [DllImport("__Internal")]
    private static extern bool RequestMicrophonePermission();
    
    [DllImport("__Internal")]
    private static extern bool StartRecording();
    
    [DllImport("__Internal")]
    private static extern string StopRecording();
    
    [DllImport("__Internal")]
    private static extern bool IsRecording();
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            gameObject.name = "WebGLMicrophone";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        InitMicrophone();
#endif
    }

    public void RequestPermission()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        RequestMicrophonePermission();
#else
        // For non-WebGL platforms, assume permission is granted
        PermissionGranted?.Invoke();
#endif
    }

    public void StartMicrophoneRecording()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StartRecording();
#endif
    }

    public void StopMicrophoneRecording()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StopRecording();
#endif
    }

    public bool IsRecordingActive()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return IsRecording();
#else
        return false;
#endif
    }

    // Called from JavaScript
    public void OnMicrophonePermissionGranted()
    {
        PermissionGranted?.Invoke();
    }

    // Called from JavaScript
    public void OnMicrophonePermissionDenied()
    {
        PermissionDenied?.Invoke();
    }

    // Called from JavaScript
    public void OnRecordingStarted()
    {
        RecordingStarted?.Invoke();
    }

    // Called from JavaScript
    public void OnRecordingError()
    {
        RecordingError?.Invoke();
    }

    // Called from JavaScript
    public void OnRecordingComplete(string base64AudioData)
    {
        RecordingComplete?.Invoke(base64AudioData);
    }
}