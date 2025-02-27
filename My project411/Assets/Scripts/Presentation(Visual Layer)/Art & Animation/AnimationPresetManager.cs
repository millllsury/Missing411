using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class AnimationPreset
{
    public float frameDelay;
    public bool keepLastFrame;
    public int repeatCount;
    public string soundName;
    public string type;
}
[System.Serializable]
public class AnimationPresetDictionary
{
    public List<AnimationPresetEntry> presets;
}

[System.Serializable]
public class AnimationPresetEntry
{
    public string name;
    public AnimationPreset preset;
}

public class AnimationPresetManager : MonoBehaviour
{
    private Dictionary<string, AnimationPreset> animationPresets;

    void Awake()
    {
        LoadPresets();
    }

    private void LoadPresets()
    {
        // Загружаем JSON-файл из папки Resources
        TextAsset jsonFile = Resources.Load<TextAsset>("AnimationPresets");

        if (jsonFile == null)
        {
            Debug.LogError("Файл AnimationPresets.json не найден в Resources!");
            animationPresets = new Dictionary<string, AnimationPreset>();
            return;
        }

        string json = jsonFile.text;
        AnimationPresetDictionary presetDictionary = JsonUtility.FromJson<AnimationPresetDictionary>(json);

        animationPresets = new Dictionary<string, AnimationPreset>();
        foreach (var entry in presetDictionary.presets)
        {
            animationPresets[entry.name] = entry.preset;
        }

        Debug.Log("Animation presets were loaded successfully.");
    }



    public AnimationPreset GetPreset(string animationFolder)
    {
        if (animationPresets.TryGetValue(animationFolder, out AnimationPreset preset))
        {
            return preset;
        }

        Debug.LogWarning($"Пресет для анимации '{animationFolder}' не найден!");
        return null;
    }
}

