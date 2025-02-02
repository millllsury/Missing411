using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static SoundManager;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider characterVolumeSlider;
    [SerializeField] private Slider backgroundEffectsVolumeSlider;
    [SerializeField] private Slider backgroundMusicVolumeSlider;
    [SerializeField] private Slider uisoundSlider;

    private GameStateManager gameStateManager;
    private SoundManager soundManager;
    

    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
        soundManager = SoundManager.Instance;

        if (soundManager == null)
        {
            Debug.LogError("SoundManager.Instance не найден.");
            return;
        }

        Debug.Log($"Загруженные настройки перед установкой слайдеров: " +
                  $"masterVolume = {gameStateManager.masterVolume}, " +
                  $"characterVolume = {gameStateManager.characterVolume}, " +
                  $"backgroundVolume = {gameStateManager.backgroundEffectsVolume}, " +
                  $"uiVolume = {gameStateManager.uiVolume}");

        RemoveListeners();

        SetSavedValues();

        uisoundSlider.value = gameStateManager.uiVolume > 0 ? 1 : 0;

        Debug.Log($"После установки: masterVolumeSlider.value = {masterVolumeSlider.value}");

        AddListeners();
    }


    private  void RemoveListeners()
    {
        // Отключаем обработку изменений перед установкой значений
        masterVolumeSlider.onValueChanged.RemoveAllListeners();
        characterVolumeSlider.onValueChanged.RemoveAllListeners();
        backgroundMusicVolumeSlider.onValueChanged.RemoveAllListeners();
        backgroundEffectsVolumeSlider.onValueChanged.RemoveAllListeners();
        uisoundSlider.onValueChanged.RemoveAllListeners();
    }

    private void SetSavedValues()
    {
        // Устанавливаем сохранённые значения без вызова `OnValueChanged`
        masterVolumeSlider.SetValueWithoutNotify(gameStateManager.masterVolume);
        characterVolumeSlider.SetValueWithoutNotify(gameStateManager.characterVolume);
        backgroundMusicVolumeSlider.SetValueWithoutNotify(gameStateManager.backgroundVolume);
        backgroundEffectsVolumeSlider.SetValueWithoutNotify(gameStateManager.backgroundEffectsVolume);
        uisoundSlider.SetValueWithoutNotify(gameStateManager.uiVolume);
    }

    private void AddListeners()
    {
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        characterVolumeSlider.onValueChanged.AddListener(value => OnCategoryVolumeChanged("characters", value));
        backgroundMusicVolumeSlider.onValueChanged.AddListener(value => OnCategoryVolumeChanged("background", value));
        uisoundSlider.onValueChanged.AddListener(value => OnCategoryVolumeChanged("ui", value));
        backgroundEffectsVolumeSlider.onValueChanged.AddListener(value => OnCategoryVolumeChanged("backgroundeffects", value));
    }

    private void OnSoundSliderChanged(float value)
    {
        bool isOn = value > 0.5f; // Если больше 0.5, считаем включенным
        float newVolume = isOn ? 1f : 0f;

        gameStateManager.uiVolume = newVolume;
        gameStateManager.SaveGlobalSettings();
        soundManager.UpdateAllVolumes();

        // Автоматически ставим слайдер в 0 или 1
        uisoundSlider.value = newVolume;

        Debug.Log($"Звук: {(isOn ? "On" : "Off")}");
    }

    public void OnMasterVolumeChanged(float value)
    {
        if (Mathf.Approximately(gameStateManager.masterVolume, value))
        {
            Debug.Log("OnMasterVolumeChanged: Значение не изменилось, не сохраняем.");
            return;
        }

        Debug.Log($"OnMasterVolumeChanged вызван с значением: {value}");
        gameStateManager.SetMasterVolume(value);
        soundManager.UpdateAllVolumes();
    }

    public void OnCharacterVolumeChanged(float value)
    {
        Debug.Log($"Изменение громкости персонажей: {value}");
        gameStateManager.SetCategoryVolume("Characters", value);
        soundManager.PlaySoundByName("Alice_happy");
    }

    public void OnBackgroundVolumeChanged(float value)
    {
        Debug.Log($"Изменение громкости фона: {value}");
        gameStateManager.SetCategoryVolume("Background", value);
    }

    public void OnBackgroundEffectsVolumeChanged(float value)
    {
        Debug.Log($"Изменение громкости фона: {value}");
        gameStateManager.SetCategoryVolume("BackgroundEffects", value);
    }

    public void OnClickVolumeChanged(float value)
    {
        Debug.Log($"Изменение громкости интерфейса: {value}");
        gameStateManager.SetCategoryVolume("UI", value);
    }


    public void OnCategoryVolumeChanged(string category, float value)
    {
        gameStateManager.SetCategoryVolume(category, value);
        soundManager.UpdateAllVolumes();
    }

    public void ResetAudioSettings()
    {
        gameStateManager.DefaultSoundSettingsValue();
        SetSavedValues();
        uisoundSlider.SetValueWithoutNotify(gameStateManager.uiVolume);
        gameStateManager.SaveGlobalSettings();
        soundManager.UpdateAllVolumes();

        Debug.Log("Аудио-настройки сброшены к значениям по умолчанию.");
    }
}
