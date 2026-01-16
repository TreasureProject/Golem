using UnityEngine;
using System;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System.Security.Cryptography;
using System.Text;

public class EmotePlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private CFConnector connector; // optional reference to CFConnector in inspector

    // guard against duplicate invocations
    private string lastAudioHash = null;
    private float lastAudioTime = 0f;
    private readonly float duplicateWindowSeconds = 1.0f;

    // track active coroutine so we don't have multiple loaders writing/playing at once
    private Coroutine loadCoroutine = null;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (connector == null)
            connector = CFConnector.instance;

        if (connector != null)
        {
            // subscribe only to the C# event. CFConnector invokes both the C# event and the UnityEvent.
            // Subscribing to both can cause the handler to be called twice if the same listener is attached
            // to both. To avoid duplicate invocation, only subscribe to the C# event here.
            connector.OnVoiceEmote += Instance_OnVoiceEmote;
        }
    }

    private void OnDestroy()
    {
        if (connector != null)
        {
            try { connector.OnVoiceEmote -= Instance_OnVoiceEmote; } catch { }
        }
    }

    private void Instance_OnVoiceEmote(CFConnector.VoiceEmoteData obj)
    {
        Debug.Log("Received voice emote: " + (obj != null ? obj.type : "null"));

        if (obj == null || string.IsNullOrEmpty(obj.audioBase64))
        {
            Debug.LogWarning("No audio data in voice emote.");
            return;
        }

        byte[] audioBytes;
        try
        {
            audioBytes = Convert.FromBase64String(obj.audioBase64);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to decode base64 audio: " + e);
            return;
        }

        // compute hash to detect duplicate/emitted twice
        string hash = ComputeHash(audioBytes);
        float now = Time.realtimeSinceStartup;
        if (hash == lastAudioHash && (now - lastAudioTime) <= duplicateWindowSeconds)
        {
            Debug.LogWarning($"Duplicate voice emote received (hash match). Ignoring duplicate. hash={hash}");
            return;
        }

        lastAudioHash = hash;
        lastAudioTime = now;

        // stop any existing loading coroutine and stop audio playback before starting a new one
        if (loadCoroutine != null)
        {
            Debug.Log("Stopping previous load coroutine to prevent multiple concurrent plays.");
            try { StopCoroutine(loadCoroutine); } catch { }
            loadCoroutine = null;
        }

        if (audioSource.isPlaying)
        {
            Debug.Log("Stopping currently playing AudioSource before playing new emote.");
            audioSource.Stop();
        }

        if (IsWav(audioBytes))
        {
            Debug.Log("Audio data detected as WAV format.");
            var clip = WavUtility.ToAudioClip(audioBytes, "VoiceEmote");
            if (clip != null)
            {
                Debug.Log($"AudioClip created (WAV), length: {clip.length}s, samples: {clip.samples}, channels: {clip.channels}");
                audioSource.clip = clip;
                Debug.Log("AudioClip assigned to AudioSource. Playing now.");
                audioSource.Play();
                Debug.Log("AudioSource.Play() called.");
            }
            else
            {
                Debug.LogError("WavUtility failed to create AudioClip.");
            }
        }
        else
        {
            Debug.Log("Audio data detected as compressed format. Starting coroutine to load and play.");
            loadCoroutine = StartCoroutine(LoadCompressedAudioFromBytes(audioBytes));
        }
    }

    private static bool IsWav(byte[] data)
    {
        if (data == null || data.Length < 12) return false;
        return data[0] == (byte)'R' && data[1] == (byte)'I' && data[2] == (byte)'F' && data[3] == (byte)'F';
    }

    private IEnumerator LoadCompressedAudioFromBytes(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            Debug.LogError("Empty audio bytes for compressed audio.");
            yield break;
        }

        string ext = DetectCompressedExtension(bytes);
        AudioType audioType = AudioType.MPEG;
        if (ext == ".m4a" || ext == ".aac") audioType = AudioType.MPEG;

        string fileName = $"voice_emote_{Guid.NewGuid()}{ext}";
        string path = Path.Combine(Application.temporaryCachePath, fileName);

        try
        {
            File.WriteAllBytes(path, bytes);
            Debug.Log($"Wrote compressed audio bytes to temp file: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to write temp audio file: " + e);
            yield break;
        }

        string uri = "file://" + path;
        using (var www = UnityWebRequestMultimedia.GetAudioClip(uri, audioType))
        {
            Debug.Log($"Requesting AudioClip from URI: {uri}, type: {audioType}");
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.LogError($"Failed to load audio clip from temp file: {www.error}");
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    Debug.Log($"AudioClip created (compressed), length: {clip.length}s, samples: {clip.samples}, channels: {clip.channels}");
                    audioSource.clip = clip;
                    Debug.Log("AudioClip assigned to AudioSource. Playing now.");
                    audioSource.Play();
                    Debug.Log("AudioSource.Play() called.");
                }
                else
                {
                    Debug.LogError("Downloaded audio clip is null.");
                }
            }
        }

        try { File.Delete(path); Debug.Log($"Deleted temp audio file: {path}"); } catch { }

        loadCoroutine = null;
    }

    private static string DetectCompressedExtension(byte[] data)
    {
        if (data == null || data.Length < 8) return ".mp3";
        if (data[0] == (byte)'I' && data[1] == (byte)'D' && data[2] == (byte)'3') return ".mp3";
        if ((data[0] & 0xFF) == 0xFF) return ".mp3";
        if (data.Length >= 8 && data[4] == (byte)'f' && data[5] == (byte)'t' && data[6] == (byte)'y' && data[7] == (byte)'p') return ".m4a";
        return ".mp3";
    }

    private static string ComputeHash(byte[] data)
    {
        if (data == null || data.Length == 0) return string.Empty;
        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(data);
            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));
            return sb.ToString();
        }
    }
}
