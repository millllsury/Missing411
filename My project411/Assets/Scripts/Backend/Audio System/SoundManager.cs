using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SoundCategory
{
    UI,
    BackgroundEffects,
    Characters,
    Background
}


[System.Serializable]
public class Sound
{
    public string name;          // Имя звука
    public AudioClip clip;       // Аудиофайл
    public bool loop;            // Зацикливать ли звук
    public SoundCategory category; // Категория звука
    [HideInInspector] public AudioSource source; // Аудиоисточник
}
public class SoundManager : MonoBehaviour
{
    
    public float uiVolume = 1f;
    public float backgroundEffectsVolume = 1f;
    public float backgroundVolume = 1f;
    public float characterVolume = 1f;

    public static SoundManager Instance { get; private set; } // Singleton

    public List<Sound> sounds;       // Список звуков

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //Debug.Log("SoundManager initialized");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (var sound in sounds)
        {
            //Debug.Log($"Initializing sound: {sound.name}");
        }

        InitializeSounds();
    }


    private void InitializeSounds()
    {
        foreach (var sound in sounds)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = sound.clip;
      
            source.loop = sound.loop;
            source.volume = GetVolumeForSound(sound);
            sound.source = source;
        }
    }


    public void ResumePlayingTracks()
    {
        var playingTracks = GameStateManager.Instance.GetPlayingTracks();
        foreach (var track in playingTracks)
        {
            PlaySoundByName(track); // Воспроизводим все сохраненные треки
            Debug.Log($"Resumed track: {track}");
        }
    }



    public void HandleSoundTrigger(string soundTrigger)
    {
        if (string.IsNullOrEmpty(soundTrigger))
        {
            Debug.LogWarning("Sound trigger is null or empty.");
            return;
        }

        // Разделяем команду и имя звука
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

    public void PlaySoundByName(string soundName)
    {
        Sound newSound = sounds.Find(s => s.name == soundName);
        if (newSound == null)
        {
            Debug.LogWarning($"Sound '{soundName}' not found.");
            return;
        }

        if (newSound.source == null)
        {
            Debug.LogError($"AudioSource for sound '{soundName}' is null!");
            return;
        }

        if (newSound.source.isPlaying)
        {
            Debug.Log($"Sound '{soundName}' is already playing.");
            return;
        }

        if (newSound.category == SoundCategory.Background)
        {
            Sound currentlyPlayingBg = sounds.Find(s => s.category == SoundCategory.Background && s.source.isPlaying);

            if (currentlyPlayingBg != null && currentlyPlayingBg != newSound)
            {
                StartCoroutine(FadeOut(currentlyPlayingBg.source, 1f, currentlyPlayingBg.name));
                Debug.Log($"currentlyPlayingBg.source : {currentlyPlayingBg.source} and currentlyPlayingBg.name: {currentlyPlayingBg.name}");
            }
        }



        if (newSound.category == SoundCategory.Background || newSound.category == SoundCategory.BackgroundEffects)
        {
            StartCoroutine(FadeIn(newSound, 4f)); // Плавное начало
        }
        else
        {
            newSound.source.volume = GetVolumeForSound(newSound);
            newSound.source.Play();
        }

        if (newSound.category == SoundCategory.Background || newSound.category == SoundCategory.BackgroundEffects)
        {
            
         GameStateManager.Instance.AddPlayingTrack(newSound.name);
 
                
        }
        
        Debug.Log($"Playing sound: {soundName}");
    }

    private IEnumerator FadeIn(Sound sound, float fadeTime)
    {
        AudioSource audioSource = sound.source;
        float targetVolume = GetVolumeForSound(sound); // Целевая громкость
        audioSource.volume = 0f; // Начинаем с 0

        audioSource.Play();

        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / fadeTime);
            yield return null;
        }

        audioSource.volume = targetVolume; // Убедиться, что достигнута целевая громкость
    }



    private IEnumerator FadeOut(AudioSource audioSource, float fadeTime, string soundName)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.Stop();
        GameStateManager.Instance.RemovePlayingTrack(soundName);
        Debug.Log($"audioSource.name: {soundName}");
        audioSource.volume = startVolume; // Восстановление начальной громкости для будущих воспроизведений
    }


    private void MuteSoundByName(string soundName)
    {
        Sound sound = sounds.Find(s => s.name == soundName);
        if (string.IsNullOrEmpty(soundName))
        {
            Debug.LogWarning("Sound name is null or empty for mute command.");
            return;
        }

        if(!sound.source.isPlaying)
        {
            sound.source.volume = GetVolumeForSound(sound);
            sound.source.mute = true;
            Debug.Log($"Muted sound: {soundName}");
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' is not currently playing.");
        }
    }

    public void StopSoundByName(string soundName)
    {
        if (string.IsNullOrEmpty(soundName))
        {
            Debug.LogWarning("Sound name is null or empty for mute command.");
            return;
        }

        Sound sound = sounds.Find(s => s.name == soundName);
        if (sound == null)
        {
            Debug.LogWarning($"Sound '{soundName}' not found in the sound list.");
            return;
        }

        //Исправлена логика: теперь проверяем, играет ли звук
        if (sound.source.isPlaying)
        {
            sound.source.Stop();
            GameStateManager.Instance.RemovePlayingTrack(sound.name);
            Debug.Log($"Stopped sound: {soundName}");
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' is not currently playing.");
        }
    }


    public void StopAllSounds()
    {
        foreach (var sound in sounds)
        {
            if (sound.source != null && sound.source.isPlaying)
            {
                sound.source.Stop();
                //GameStateManager.Instance.RemovePlayingTrack(sound.name);
            }
        }
        Debug.Log("All sounds have been stopped.");
    }

    public void MuteAllSounds()
    {
        foreach (var sound in sounds)
        {
            if (sound.source != null && sound.source.isPlaying)
            {
                sound.source.mute = true;
            }
        }
        Debug.Log("All sounds have been stopped.");
    }

    public void UnmuteAllSounds()
    {
        foreach (var sound in sounds)
        {
            if (sound.source != null && sound.source.isPlaying)
            {
                sound.source.mute = false;
            }
        }
        Debug.Log("All sounds have been unmuted.");
    }

    private float GetVolumeForSound(Sound sound)
    {
        if (GameStateManager.Instance == null)
        {
            Debug.Log("GameStateManager.Instance не инициализирован. Используется значение по умолчанию.");
            return 1f; // Значение по умолчанию
        }

        var gameState = GameStateManager.Instance;

        switch (sound.category)
        {
            case SoundCategory.UI:
                return gameState.masterVolume * gameState.uiVolume;
            case SoundCategory.BackgroundEffects:
                return gameState.masterVolume * gameState.backgroundEffectsVolume;
            case SoundCategory.Characters:
                return gameState.masterVolume * gameState.characterVolume;
            case SoundCategory.Background: 
                return gameState.masterVolume * gameState.backgroundVolume;
            default:
                return gameState.masterVolume;
        }
    }


    public void UpdateAllVolumes()
    {
        foreach (var sound in sounds)
        {
            if (sound.source != null)
            {
                sound.source.volume = GetVolumeForSound(sound);
            }
        }
        Debug.Log("Все громкости обновлены.");
    }


    public void UIClickSound()
    {
        Debug.Log("UIClickSound вызван!");
        PlaySoundByName("UIClick");
    }


}
