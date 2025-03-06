using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Runtime.CompilerServices;

[System.Serializable]
public class GameState
{
    public string currentEpisode = "1";
    public string currentScene = "1";             // Текущая сцена
    public string currentDialogue = "1";          // Текущий диалог
    public int textCounter = 0;                 // Счетчик текста
    public bool episodeNameShowed;
    public Dictionary<string, bool> flags;  // Флаги игры
    public int hairIndex;                   // Индекс волос персонажа
    public int clothesIndex;                // Индекс одежды персонажа
    public string leftCharacterName;
    public string rightCharacterName;

    public string currentBackgroundName;

    public string currentBackgroundAnimation;
    public string currentForegroundAnimation;

    public float animationFrameDelay;
    public float foregroundFrameDelay;

    public int animationRepeatCount;
    public int foregroundRepeatCount;


    public bool animationKeepLastFrame;
    public bool foregroundKeepLastFrame;

    public List<DialogueState> dialogueHistory = new(); //для dialogueHistory стека

    public List<string> playingTracks = new();
    public int keys;
    public List<int> unlockedHairstyles = new List<int>();
    public List<int> unlockedClothes = new List<int>();
    public bool isLeftDoorOpened = false;
    public bool isRightDoorOpened = false;
    public List<string> collectedKeys = new List<string>();
    public bool clothesReceived = false;

    public string lastBackgroundFrame;
}

public class DialogueState
{
    public int dialogueId;
    public int textCounter;

    public DialogueState(int dialogueId, int textCounter)
    {
        this.dialogueId = dialogueId;
        this.textCounter = textCounter;
    }
}

[System.Serializable]
public class SaveSlot
{
    public string slotName;      // Название слота (например, "Слот 1")
    public string saveDate;      // Дата сохранения
    public GameState gameState;  // Состояние игры в этом слоте
}

[System.Serializable]
public class SaveSlots
{
    public List<SaveSlot> slots = new List<SaveSlot>();
}



public class GameStateManager : MonoBehaviour
{
    #region Volume Settings
    private class AudioSettings
    {
        public float masterVolume;
        public float characterVolume;
        public float backgroundEffectsVolume;
        public float backgroundVolume;
        public float uiVolume;
    }

    // global settings
    public float masterVolume = 1f;
    public float characterVolume = 1f;
    public float backgroundEffectsVolume = 1f;
    public float backgroundVolume = 1f;
    public float uiVolume = 1f;

    public GameState originalState; // To store the original state
    public bool isNewGame = false;
    public static GameStateManager Instance { get; private set; }

    public bool hasBeenTransited = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Объект сохраняется между сценами
            currentState = new GameState();
            LoadGlobalSettings();
        }
        else
        {

            Destroy(gameObject); // Удаляем дубликат
        }

        if (!hasSaved)
        {
            SetHasSaved(true);
        }

        if (!hasBeenTransited)
        {
            SetHasTransited(true);
        }

        backgroundController = FindFirstObjectByType<BackgroundController>();
    }


    private BackgroundController backgroundController;

    #region Global Settings

    private static string SettingsFilePath => Path.Combine(Application.persistentDataPath, "audio_settings.json");


    private void LoadGlobalSettings()
    {
        if (File.Exists(SettingsFilePath))
        {
            string json = File.ReadAllText(SettingsFilePath);
            var settings = JsonConvert.DeserializeObject<AudioSettings>(json);
            masterVolume = settings.masterVolume;
            //Debug.Log($"masterVolume: {masterVolume}, settings.masterVolume: {settings.masterVolume} ");
            characterVolume = settings.characterVolume;
            backgroundEffectsVolume = settings.backgroundEffectsVolume;
            uiVolume = settings.uiVolume;
            backgroundVolume = settings.backgroundVolume;
            //Debug.Log("Настройки громкости успешно загружены.");
        }
        else
        {
            DefaultSoundSettingsValue();
            SaveGlobalSettings();
        }
    }

    public void DefaultSoundSettingsValue()
    {
        // Устанавливаем значения по умолчанию
        masterVolume = 0.5f;
        characterVolume = 0.5f;
        backgroundEffectsVolume = 0.5f;
        backgroundVolume = 0.5f;
        uiVolume = 1f;
        //Debug.Log("Настройки громкости созданы с значениями по умолчанию.");
    }

    public void SaveGlobalSettings()
    {
        Debug.Log($"masterVolume1: {masterVolume}");

        var settings = new AudioSettings
        {
            masterVolume = masterVolume,
            characterVolume = characterVolume,
            backgroundEffectsVolume = backgroundEffectsVolume,
            uiVolume = uiVolume,
            backgroundVolume = backgroundVolume
        };

        //Debug.Log($"masterVolume2: {masterVolume}");

        string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText(SettingsFilePath, json);

        //Debug.Log("Настройки громкости сохранены.");
    }

    public void SetMasterVolume(float value)
    {
        if (Mathf.Approximately(masterVolume, value))
        {
            Debug.Log("SetMasterVolume: Значение не изменилось, не сохраняем.");
            return;
        }

        //Debug.Log($"SetMasterVolume вызван с значением: {value}");
        masterVolume = value;

        SaveGlobalSettings();
    }



    public void SetCategoryVolume(string category, float value)
    {
        switch (category.ToLower())
        {
            case "characters":
                characterVolume = value;
                break;
            case "backgroundeffects":
                backgroundEffectsVolume = value;
                break;
            case "ui":
                uiVolume = value;

                // Находим объект даже если он выключен
                GameObject uiClicksObject = FindInactiveGameObject("UIClicks");
                if (uiClicksObject != null)
                {
                    uiClicksObject.SetActive(value > 0);
                    Debug.Log($"Объект 'UIClicks' {(value > 0 ? "включен" : "выключен")}");
                }
                else
                {
                    Debug.LogWarning("Объект 'UIClicks' не найден в сцене, даже если выключен!");
                }
                break;

            case "background":
                backgroundVolume = value;
                break;
            default:
                Debug.LogWarning($"Неизвестная категория звука: {category}");
                return;
        }

        SaveGlobalSettings();
    }

 
    private GameObject FindInactiveGameObject(string name)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == name)
            {
                return obj; 
            }
        }
        return null; 
    }


    #endregion



    #endregion

    private HashSet<string> playingTracks = new();

    private SaveSlots saveSlots = new();

    public List<SaveSlot> GetSaveSlots()
    {
        if (saveSlots == null || saveSlots.slots == null)
        {
            Debug.LogWarning("The slot list is not initialized. An empty list is returned.");
            return new List<SaveSlot>();
        }
        return saveSlots.slots;
    }


    private int selectedSlotIndex = -1; // Индекс выбранного слота (-1, если слот не выбран)

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= saveSlots.slots.Count)
        {
            return;
        }

        var selectedSlot = saveSlots.slots[slotIndex];
        if (selectedSlot.gameState == null)
        {
            Debug.LogWarning($"Slot {slotIndex + 1} is empty.");
            return;
        }
        SaveOriginalState(slotIndex);
        selectedSlotIndex = slotIndex;
        currentState = selectedSlot.gameState;

    }


    public int GetSelectedSlotIndex()
    {
        return selectedSlotIndex;
    }

    public bool HasSelectedSlot()
    {
        return selectedSlotIndex >= 0;
    }
    private static string slotsFilePath => Path.Combine(Application.persistentDataPath, "save_slots.json");

    public void LoadSaveSlots()
    {
        if (File.Exists(slotsFilePath))
        {
            string json = File.ReadAllText(slotsFilePath);
            saveSlots = JsonConvert.DeserializeObject<SaveSlots>(json);
            //Debug.Log("Слоты сохранений загружены.");
        }
        else
        {
            saveSlots = new SaveSlots { slots = new List<SaveSlot>() };
            // Создаём пустые слоты
            for (int i = 1; i <= 6; i++)
            {
                saveSlots.slots.Add(new SaveSlot
                {
                    slotName = $"Slot {i}",
                    saveDate = null,
                    gameState = null
                });
            }
            SaveSlotsToFile();
            //Debug.Log("Созданы пустые слоты сохранений.");
        }
    }



    public void SaveGameToSlot(int slotIndex)
    {

        var slot = saveSlots.slots[slotIndex];
        slot.gameState = currentState; // Сохраняем текущее состояние игры
        slot.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        SaveSlotsToFile();
    }


    private bool hasSaved = false; //Flag indicating whether a save was made

    public void SetHasSaved(bool value)
    {
        hasSaved = value;
    }

    public bool HasSaved()
    {
        return hasSaved;
    }

    public void SetHasTransited(bool value)
    {
        hasBeenTransited = value;
    }

    public bool HasBeenTransited()
    {
        return hasBeenTransited;
    }

    public void SaveSlotsToFile()
    {
        string json = JsonConvert.SerializeObject(saveSlots, Formatting.Indented);
        File.WriteAllText(slotsFilePath, json);
        Debug.Log($"Save slots are written to file: {slotsFilePath}");
    }

    public void LoadGameFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= saveSlots.slots.Count)
        {
            Debug.LogError("Invalid save slot index.");
            return;
        }

        var slot = saveSlots.slots[slotIndex];
        if (slot.gameState == null)
        {
            Debug.LogWarning($"Slot {slot.slotName} is empty.");
            return;
        }
        currentState = slot.gameState; // Восстанавливаем состояние игры
        CurrencyManager.Instance.SetKeys(currentState.keys);
        //Debug.Log($"Game loaded from slot {slot.slotName}.");
    }

    public void SaveOriginalState(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= saveSlots.slots.Count)
        {

            return;
        }

        var slot = saveSlots.slots[slotIndex];
        if (slot.gameState != null)
        {

            originalState = JsonConvert.DeserializeObject<GameState>(
                JsonConvert.SerializeObject(slot.gameState));


        }
        else
        {
            originalState = null;

        }
    }



    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= saveSlots.slots.Count)
        {
            //Debug.LogError("Индекс слота для очистки недействителен.");
            return;
        }

        var slot = saveSlots.slots[slotIndex];
        slot.gameState = null; // Удаляем данные состояния игры
        slot.saveDate = null;  // Удаляем дату сохранения
        slot.slotName = $"Slot {slotIndex + 1}"; // Сбрасываем название слота

        SaveSlotsToFile();
        Debug.Log($"Slot {slotIndex + 1} was cleared.");
    }



    ///
    public void SaveCurrentState(
     int episodeId, int sceneId, int dialogueId, int textCounter, bool episodeNameShowed,
     Dictionary<string, bool> flags, string leftCharacter, string rightCharacter)
    {
        UpdateSceneState(episodeId.ToString(), sceneId.ToString(), dialogueId.ToString(), textCounter, episodeNameShowed);
        UpdateFlags(flags);
        SaveCharacterNames(leftCharacter, rightCharacter);

    }



    #region Управление состоянием

    public void UpdateSceneState(string episode, string scene, string dialogue, int textIndex, bool episodeNameShowed)
    {
        if (string.IsNullOrEmpty(scene) || string.IsNullOrEmpty(dialogue))
        {
            Debug.LogWarning("Попытка обновить состояние с пустыми значениями.");
            return;
        }
        currentState.currentEpisode = episode;
        currentState.currentScene = scene;
        currentState.currentDialogue = dialogue;
        currentState.textCounter = textIndex;
        currentState.episodeNameShowed = episodeNameShowed;

        Debug.Log($"Сохранено состояние: Episode ={episode} Scene={scene}, Dialogue={dialogue}, TextCounter={textIndex}, EpisodeNameShowe=");
    }

    private GameState currentState;
    public GameState GetGameState()
    {
        return currentState;
    }

    #endregion

    #region Сохранение и загрузка


    public (int, int) LoadAppearance()
    {
        return (currentState.hairIndex >= 0 ? currentState.hairIndex : 0,
                currentState.clothesIndex >= 0 ? currentState.clothesIndex : 0);
    }

    public (string, string) LoadCharacterNames()
    {
        return (currentState.leftCharacterName ?? "", currentState.rightCharacterName ?? "");
    }



    public void UpdateFlags(Dictionary<string, bool> flags)
    {
        currentState.flags = new Dictionary<string, bool>(flags);
    }



    public void SaveAppearance(int hairIndex, int clothesIndex)
    {
        currentState.hairIndex = hairIndex;
        currentState.clothesIndex = clothesIndex;
        Debug.Log($"AppearanceSaved: Волосы={hairIndex}, Одежда={clothesIndex}");
    }

    public void SaveCharacterNames(string leftCharacter, string rightCharacter)
    {
        currentState.leftCharacterName = leftCharacter;
        currentState.rightCharacterName = rightCharacter;
        Debug.Log($"Names Saved: Левый = {leftCharacter}, Правый = {rightCharacter}");

        int selectedSlotIndex = GetSelectedSlotIndex();
        if (selectedSlotIndex != -1)
        {
            SaveGameToSlot(selectedSlotIndex);

        }
    }


 
    private static readonly Dictionary<string, int> characterPositions = new Dictionary<string, int>
    {
        { "Alice", 1 },
        { "Father", 1 }
    };

    // Метод для получения позиции персонажа
    public static int GetCharacterPosition(string character)
    {
        return characterPositions.TryGetValue(character, out int place) ? place : 2; // По умолчанию ставим справа
    }

    public void SaveBackground(string backgroundName)
    {
        currentState.currentBackgroundName = backgroundName;
        Debug.Log($"Фон сохранён: {backgroundName}");

        int selectedSlotIndex = GetSelectedSlotIndex();
        if (selectedSlotIndex != -1)
        {
            SaveGameToSlot(selectedSlotIndex);

        }
    }

    public string LoadBackground()
    {
        return currentState.currentBackgroundName;
    }

    public string LoadSceneID()
    {
        return currentState.currentScene;
    }


    public void SaveBackgroundAnimation(string animationName, float frameDelay, int repeatCount, bool keepLastFrame)
    {
        currentState.currentBackgroundAnimation = animationName;
        currentState.animationFrameDelay = frameDelay;
        currentState.animationRepeatCount = repeatCount;
        currentState.animationKeepLastFrame = keepLastFrame;


        Debug.Log($"Background Animation Saved: {animationName}");
    }

    public void SaveForegroundAnimation(string animationName, float frameDelay, int repeatCount, bool keepLastFrame)
    {
        currentState.currentForegroundAnimation = animationName;
        currentState.foregroundFrameDelay = frameDelay;
        currentState.foregroundRepeatCount = repeatCount;
        currentState.foregroundKeepLastFrame = keepLastFrame;


        Debug.Log($"Foreground Animation Saved: {animationName}");
    }

    public void SaveLastBackgroundFrame(string frameName)
    {
        currentState.lastBackgroundFrame = frameName;
        Debug.Log($"Сохранён последний кадр анимации заднего плана: {frameName}");
        int selectedSlotIndex = GetSelectedSlotIndex();
        SaveGameToSlot(selectedSlotIndex);
        SaveSlotsToFile();
    }

    public string LoadLastBackgroundFrame()
    {
        return currentState.lastBackgroundFrame;
    }


    public void ClearBackgroundAnimation()
    {
        //Debug.LogError($" ClearBackgroundAnimation() вызван! Текущий фон перед очисткой: {currentState.currentBackgroundName}");

        currentState.currentBackgroundAnimation = null;
        currentState.animationFrameDelay = 0f;
        currentState.animationRepeatCount = 0;
        currentState.animationKeepLastFrame = false;


        Debug.Log("Background animation data cleared from save.");

        int selectedSlotIndex = GetSelectedSlotIndex();
        if (selectedSlotIndex != -1)
        {
            SaveGameToSlot(selectedSlotIndex);
            Debug.Log($"Background animation data removed from slot {selectedSlotIndex + 1}.");
        }
    }


    public void ClearForegroundAnimation()
    {
        currentState.currentForegroundAnimation = null;
        currentState.foregroundFrameDelay = 0f;
        currentState.foregroundRepeatCount = 0;
        currentState.foregroundKeepLastFrame = false;

        Debug.Log("Foreground animation data cleared from save.");

        // Принудительно сохраняем состояние после очистки
        int selectedSlotIndex = GetSelectedSlotIndex();
        if (selectedSlotIndex != -1)
        {
            SaveGameToSlot(selectedSlotIndex);
            Debug.Log($"Foreground animation data removed from slot {selectedSlotIndex + 1}.");
        }
    }


    public (string, float, int, bool) LoadBackgroundAnimation()
    {
        return (
            currentState.currentBackgroundAnimation ?? "",
            currentState.animationFrameDelay,
            currentState.animationRepeatCount,
            currentState.animationKeepLastFrame


        );
    }

    public (string, float, int, bool) LoadForegroundAnimation()
    {
        return (
            currentState.currentForegroundAnimation ?? "",
            currentState.foregroundFrameDelay,
            currentState.foregroundRepeatCount,
            currentState.foregroundKeepLastFrame

        );
    }



    public void SavePlayingTracks()
    {
        // Проверяем текущую сцену и исключаем WardrobeScene и MainMenu
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "WardrobeScene" || currentSceneName == "MainMenu")
        {
            Debug.Log($"Skipping saving tracks for scene: {currentSceneName}");
            return;
        }

        currentState.playingTracks = GetPlayingTracks();
        Debug.Log("Сохранены текущие треки: " + string.Join(", ", currentState.playingTracks));
    }

    public List<string> GetPlayingTracks()
    {
        return new List<string>(playingTracks);
    }


    public void LoadPlayingTracks()
    {
        Debug.Log("LoadPlayingTracks() вызван!");
        foreach (var track in currentState.playingTracks)
        {
            if (!SoundManager.Instance.sounds.Any(s => s.name == track && s.source.isPlaying))
            {
                SoundManager.Instance.PlaySoundByName(track);
            }
        }
        Debug.Log("Загружены треки: " + string.Join(", ", currentState.playingTracks));
    }



    public void AddPlayingTrack(string trackName)
    {
        if (!string.IsNullOrEmpty(trackName) && !playingTracks.Contains(trackName))
        {
            playingTracks.Add(trackName);
            Debug.Log($"Track added to playing list: {trackName}");
        }
        else
        {
            Debug.Log($"Track '{trackName}' is already in the playing list.");
        }
    }

    public void ClearTracksOnSceneChange()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenu" || currentScene == "WardrobeScene")
        {
            RemoveAllPlayingTracks();
            Debug.Log($"All tracks have been cleared after leaving '{currentScene}'.");
        }
    }


    public void RemovePlayingTrack(string trackName)
    {
        if (playingTracks.Contains(trackName))
        {
            playingTracks.Remove(trackName);
            Debug.Log($"Track removed from playing list: {trackName}");
        }
    }

    public void RemoveAllPlayingTracks()
    {
        var tracksToRemove = playingTracks.ToList(); // Теперь будет работать корректно

        foreach (var trackName in tracksToRemove)
        {
            SoundManager.Instance.StopSoundByName(trackName); // Остановка звука
            playingTracks.Remove(trackName);                  // Удаление из списка
            Debug.Log($"Track '{trackName}' stopped and removed from playing list.");
        }

        Debug.Log("All tracks have been stopped and removed from the playing list.");
    }

    #region keys
    public void SetKeys(int amount)
    {
        currentState.keys = amount;
    }

    public int GetKeys()
    {
        return currentState.keys;
    }

    public void AddKeys(int amount)
    {
        currentState.keys += amount;

    }

    public bool SpendKeys(int amount)
    {
        if (currentState.keys >= amount)
        {
            currentState.keys -= amount;
            return true;
        }
        return false;
    }

    private Dictionary<int, HashSet<string>> collectedKeys = new Dictionary<int, HashSet<string>>();

    // Сохранение информации о собранном ключе
    public void SaveKeyCollected(string keyID)
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= saveSlots.slots.Count)
        {
            Debug.LogError($"[SaveKeyCollected] Некорректный слот: {selectedSlotIndex}");
            return;
        }

        var slot = saveSlots.slots[selectedSlotIndex];
        if (slot.gameState == null)
        {
            slot.gameState = new GameState();
        }

        if (!slot.gameState.collectedKeys.Contains(keyID))
        {
            slot.gameState.collectedKeys.Add(keyID);
            Debug.Log($"[SaveKeyCollected] Ключ {keyID} сохранен в слот {selectedSlotIndex}");

            SaveGameToSlot(selectedSlotIndex);
            SaveSlotsToFile();
        }
    }


    // Проверка, был ли ключ уже собран
    public bool IsKeyCollected(string keyID)
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= saveSlots.slots.Count)
            return false;

        var slot = saveSlots.slots[selectedSlotIndex];
        return slot.gameState != null && slot.gameState.collectedKeys.Contains(keyID);
    }


    public HashSet<string> LoadCollectedKeys()
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= saveSlots.slots.Count)
            return new HashSet<string>();

        var slot = saveSlots.slots[selectedSlotIndex];
        return slot.gameState != null ? new HashSet<string>(slot.gameState.collectedKeys) : new HashSet<string>();
    }


    #endregion

    #region Unlock clothes


    public void UnlockNextItem()
    {
        int nextHairIndex = currentState.unlockedHairstyles.Max() + 1;
        int nextClothesIndex = currentState.unlockedClothes.Max() + 1;

        if (!currentState.unlockedHairstyles.Contains(nextHairIndex))
        {
            currentState.unlockedHairstyles.Add(nextHairIndex);
            Debug.Log($"Открыта новая прическа: {nextHairIndex}");
        }

        if (!currentState.unlockedClothes.Contains(nextClothesIndex))
        {
            currentState.unlockedClothes.Add(nextClothesIndex);
            Debug.Log($"Открыта новая одежда: {nextClothesIndex}");
        }


        SaveGameToSlot(GetSelectedSlotIndex()); // Сохраняем прогресс только в выбранный слот

    }

    public List<int> GetUnlockedHairstyles() => currentState.unlockedHairstyles;
    public List<int> GetUnlockedClothes() => currentState.unlockedClothes;


    #endregion


    public void SetLeftDoorOpened(bool state)
    {
        currentState.isLeftDoorOpened = state;
        SaveGameToSlot(GetSelectedSlotIndex());
        SaveSlotsToFile();
    }

    public void SetRightDoorOpened(bool state)
    {
        currentState.isRightDoorOpened = state;
        SaveGameToSlot(GetSelectedSlotIndex());
        SaveSlotsToFile();
    }

    public bool GetLeftDoorOpened()
    {
        return currentState.isLeftDoorOpened;
    }

    public bool GetRightDoorOpened()
    {
        return currentState.isRightDoorOpened;
    }

    public void SetClothesReceived(bool state)
    {
        currentState.clothesReceived = state;
        SaveGameToSlot(GetSelectedSlotIndex());
        SaveSlotsToFile();
    }

    public bool GetClothesReceived()
    {
        return currentState.clothesReceived;
    }

    #endregion
}