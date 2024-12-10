using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;          // ��� �����, ��� � JSON
        public AudioClip clip;       // �������� ����
        public bool loop;            // ����� �� ����������� ����
    }

    public List<Sound> sounds;       // ������ ������
    private Dictionary<string, AudioSource> activeSounds; // �������� �����

    private void Awake()
    {
        activeSounds = new Dictionary<string, AudioSource>();
    }

    /// <summary>
    /// ������������ ������� ��������������� � ���������� ������.
    /// </summary>
    /// <param name="soundTrigger">��� �������� (��������, "play:rain" ��� "mute:rain").</param>
    public void HandleSoundTrigger(string soundTrigger)
    {
        if (string.IsNullOrEmpty(soundTrigger))
        {
            Debug.LogWarning("Sound trigger is null or empty.");
            return;
        }

        // ��������� ������� � ��� �����
        string[] parts = soundTrigger.Split(':');
        string command = parts[0].ToLower();
        string soundName = parts.Length > 1 ? parts[1] : null;

        switch (command)
        {
            case "play":
                PlaySoundByName(soundName);
                break;

            case "mute":
                MuteSoundByName(soundName);
                break;

            case "stop":
                StopSoundByName(soundName);
                break;

            default:
                Debug.LogWarning($"Unknown sound command: {command}");
                break;
        }
    }

    private void PlaySoundByName(string soundName)
    {
        if (string.IsNullOrEmpty(soundName))
        {
            Debug.LogWarning("Sound name is null or empty.");
            return;
        }

        Sound sound = sounds.Find(s => s.name == soundName);
        if (sound == null)
        {
            Debug.LogWarning($"Sound '{soundName}' not found in the sound list.");
            return;
        }

        if (activeSounds.ContainsKey(sound.name))
        {
            Debug.Log($"Sound '{sound.name}' is already playing.");
            return;
        }

        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = sound.clip;
        source.loop = sound.loop;
        source.Play();

        activeSounds[sound.name] = source;
        Debug.Log($"Playing sound: {sound.name}");
    }

    private void MuteSoundByName(string soundName)
    {
        if (string.IsNullOrEmpty(soundName))
        {
            Debug.LogWarning("Sound name is null or empty for mute command.");
            return;
        }

        if (activeSounds.TryGetValue(soundName, out var source))
        {
            source.mute = true;
            Debug.Log($"Muted sound: {soundName}");
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' is not currently playing.");
        }
    }

    private void StopSoundByName(string soundName)
    {
        if (string.IsNullOrEmpty(soundName))
        {
            Debug.LogWarning("Sound name is null or empty for stop command.");
            return;
        }

        if (activeSounds.TryGetValue(soundName, out var source))
        {
            source.Stop();
            Destroy(source);
            activeSounds.Remove(soundName);
            Debug.Log($"Stopped sound: {soundName}");
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' is not currently playing.");
        }
    }
}
