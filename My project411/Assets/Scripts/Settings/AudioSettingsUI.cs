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

    [SerializeField]  private FeedbackManager feedbackManager;

    private void Start()
    {
        feedbackManager = FindFirstObjectByType<FeedbackManager>();
        if (SoundManager.Instance == null)
        {
           
            return;
        }


        RemoveListeners();

        SetSavedValues();

        uisoundSlider.value = GameStateManager.Instance.uiVolume > 0 ? 1 : 0;

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
        masterVolumeSlider.SetValueWithoutNotify(GameStateManager.Instance.masterVolume);
        characterVolumeSlider.SetValueWithoutNotify(GameStateManager.Instance.characterVolume);
        backgroundMusicVolumeSlider.SetValueWithoutNotify(GameStateManager.Instance.backgroundVolume);
        backgroundEffectsVolumeSlider.SetValueWithoutNotify(GameStateManager.Instance.backgroundEffectsVolume);
        uisoundSlider.SetValueWithoutNotify(GameStateManager.Instance.uiVolume);
    }

    private void AddListeners()
    {
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        characterVolumeSlider.onValueChanged.AddListener(value => OnCategoryVolumeChanged("characters", value));
        backgroundMusicVolumeSlider.onValueChanged.AddListener(value => OnCategoryVolumeChanged("background", value));
        uisoundSlider.onValueChanged.AddListener(value => OnCategoryVolumeChanged("ui", value));
        backgroundEffectsVolumeSlider.onValueChanged.AddListener(value => OnCategoryVolumeChanged("backgroundeffects", value));
    }

    

    public void OnMasterVolumeChanged(float value)
    {
        if (Mathf.Approximately(GameStateManager.Instance.masterVolume, value))
        {
            Debug.Log("OnMasterVolumeChanged: Значение не изменилось, не сохраняем.");
            return;
        }

        Debug.Log($"OnMasterVolumeChanged вызван с значением: {value}");
        GameStateManager.Instance.SetMasterVolume(value);
        SoundManager.Instance.UpdateAllVolumes();
    }

    public void OnCharacterVolumeChanged(float value)
    {
        Debug.Log($"Изменение громкости персонажей: {value}");
        GameStateManager.Instance.SetCategoryVolume("Characters", value);
        SoundManager.Instance.PlaySoundByName("Alice_happy");
    }

    public void OnBackgroundVolumeChanged(float value)
    {
        Debug.Log($"Изменение громкости фона: {value}");
        GameStateManager.Instance.SetCategoryVolume("Background", value);
    }

    public void OnBackgroundEffectsVolumeChanged(float value)
    {
        Debug.Log($"Изменение громкости фона: {value}");
        GameStateManager.Instance.SetCategoryVolume("BackgroundEffects", value);
    }

    public void OnClickVolumeChanged(float value)
    {
        Debug.Log($"Изменение громкости интерфейса: {value}");
        GameStateManager.Instance.SetCategoryVolume("UI", value);
    }


    public void OnCategoryVolumeChanged(string category, float value)
    {
        GameStateManager.Instance.SetCategoryVolume(category, value);
        SoundManager.Instance.UpdateAllVolumes();
    }

    public void ResetAudioSettings()
    {
        GameStateManager.Instance.DefaultSoundSettingsValue();
        SetSavedValues();
        uisoundSlider.SetValueWithoutNotify(GameStateManager.Instance.uiVolume);
        GameStateManager.Instance.SaveGlobalSettings();
        SoundManager.Instance.UpdateAllVolumes();
        feedbackManager.ShowMessage("Audio settings are reset to default values.");
        Debug.Log("Audio settings are reset to default values.");
    }
}
