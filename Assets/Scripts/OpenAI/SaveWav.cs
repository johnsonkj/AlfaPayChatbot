using UnityEngine;
using System.IO;
using System;

public static class SaveWav
{
    public static byte[] ToBytes(AudioClip clip)
    {
        if (clip.samples == 0) return null;

        MemoryStream stream = new MemoryStream();
        int headerSize = 44;

        int samples = clip.samples;
        int channels = clip.channels;
        int frequency = clip.frequency;

        float[] samplesData = new float[samples * channels];
        clip.GetData(samplesData, 0);

        byte[] wavData = new byte[samplesData.Length * 2]; // 16-bit
        int pos = 0;
        foreach (var sample in samplesData)
        {
            short val = (short)(sample * short.MaxValue);
            byte[] bytes = BitConverter.GetBytes(val);
            wavData[pos++] = bytes[0];
            wavData[pos++] = bytes[1];
        }

        int byteRate = frequency * channels * 2;

        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(headerSize + wavData.Length - 8);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((ushort)1);
            writer.Write((ushort)channels);
            writer.Write(frequency);
            writer.Write(byteRate);
            writer.Write((ushort)(channels * 2));
            writer.Write((ushort)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(wavData.Length);
            writer.Write(wavData);
        }

        return stream.ToArray();
    }
}
