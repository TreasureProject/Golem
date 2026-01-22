using System;
using UnityEngine;

public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] wavFile, string clipName)
    {
        if (wavFile == null || wavFile.Length < 44) return null;
        int channels = BitConverter.ToInt16(wavFile, 22);
        int sampleRate = BitConverter.ToInt32(wavFile, 24);
        int byteRate = BitConverter.ToInt32(wavFile, 28);
        int bitsPerSample = BitConverter.ToInt16(wavFile, 34);
        int dataStartIndex = 44;
        int samples = (wavFile.Length - dataStartIndex) / (bitsPerSample / 8);
        float[] floatData = new float[samples];
        if (bitsPerSample == 16)
        {
            for (int i = 0; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(wavFile, dataStartIndex + i * 2);
                floatData[i] = sample / 32768f;
            }
        }
        else if (bitsPerSample == 8)
        {
            for (int i = 0; i < samples; i++)
            {
                floatData[i] = (wavFile[dataStartIndex + i] - 128) / 128f;
            }
        }
        else
        {
            Debug.LogError("Unsupported WAV bit depth: " + bitsPerSample);
            return null;
        }
        AudioClip audioClip = AudioClip.Create(clipName, samples / channels, channels, sampleRate, false);
        audioClip.SetData(floatData, 0);
        return audioClip;
    }
}