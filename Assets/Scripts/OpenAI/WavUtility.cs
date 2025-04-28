using UnityEngine;
using System;

public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] wavFile, int offsetSamples = 0, string name = "wav")
    {
        if (wavFile == null || wavFile.Length < 44)
        {
            Debug.LogError("Invalid WAV file: too small or null.");
            return null;
        }

        int channels = BitConverter.ToInt16(wavFile, 22);
        int sampleRate = BitConverter.ToInt32(wavFile, 24);
        int subchunk2 = BitConverter.ToInt32(wavFile, 40);

        if (sampleRate <= 0 || channels <= 0 || subchunk2 <= 0)
        {
            Debug.LogError($"Invalid WAV header. SampleRate: {sampleRate}, Channels: {channels}, SubChunk2Size: {subchunk2}");
            return null;
        }

        int sampleCount = subchunk2 / 2; // 2 bytes per 16-bit sample

        if (sampleCount <= 0)
        {
            Debug.LogError("Invalid WAV file: no audio samples found.");
            return null;
        }

        float[] data = new float[sampleCount];
        int offset = 44;

        for (int i = 0; i < sampleCount; i++)
        {
            if (offset + 1 >= wavFile.Length) break;

            short sample = BitConverter.ToInt16(wavFile, offset);
            data[i] = sample / 32768f;
            offset += 2;
        }

        AudioClip audioClip = AudioClip.Create(name, sampleCount / channels, channels, sampleRate, false);
        audioClip.SetData(data, offsetSamples);

        return audioClip;
    }
}
