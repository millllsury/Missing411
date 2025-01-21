using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;



[System.Serializable]
public class GameState
{
    public string currentEpisode;
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
    public float animationFrameDelay;
    public int animationRepeatCount;
    public bool animationKeepLastFrame;
    public List<DialogueState> dialogueHistory = new List<DialogueState>(); //для dialogueHistory стека
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
    public static GameStateManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Объект сохраняется между сценами
            currentState = new GameState();
            Debug.Log("GameStateManager has been initialized.");
        }
        else
        {
            Debug.LogWarning("GameStateManager already exists. Delete duplicate.");
            Destroy(gameObject); // Удаляем дубликат
        }
    }


    private SaveSlots saveSlots = new SaveSlots();

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
            Debug.LogError($"Индекс слота {slotIndex} вне диапазона.");
            return;
        }

        var selectedSlot = saveSlots.slots[slotIndex];
        if (selectedSlot.gameState == null)
        {
            Debug.LogWarning($"Слот {slotIndex + 1} пуст.");
            return;
        }

        selectedSlotIndex = slotIndex;
        currentState = selectedSlot.gameState; // Устанавливаем текущее состояние
        Debug.Log($"Слот {slotIndex + 1} выбран. Состояние игры загружено.");
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
            Debug.Log("Слоты сохранений загружены.");
        }
        else
        {
            saveSlots = new SaveSlots { slots = new List<SaveSlot>() };
            // Создаём пустые слоты
            for (int i = 1; i <= 6; i++)
            {
                saveSlots.slots.Add(new SaveSlot
                {
                    slotName = $"Слот {i}",
                    saveDate = null,
                    gameState = null
                });
            }
            SaveSlotsToFile();
            Debug.Log("Созданы пустые слоты сохранений.");
        }
    }

   

    public void SaveGameToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= saveSlots.slots.Count)
        {
            Debug.LogError("Invalid save slot index.");
            return;
        }

        var slot = saveSlots.slots[slotIndex];
        slot.gameState = currentState; // Сохраняем текущее состояние игры
        slot.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        SaveSlotsToFile();
        Debug.Log($"The game has been saved to the slot. {slot.slotName}.");
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
        Debug.Log($"Game loaded from slot {slot.slotName}.");
    }


   

    private static string saveFilePath => Path.Combine(Application.persistentDataPath, "game_state.json");

   
    private GameState currentState;



    #region Управление состоянием

    public void UpdateSceneState(/*string episode,*/ string scene, string dialogue, int textIndex/*, bool episodeNameShowed*/)
    {
        if (string.IsNullOrEmpty(scene) || string.IsNullOrEmpty(dialogue))
        {
            Debug.LogWarning("Попытка обновить состояние с пустыми значениями.");
            return;
        }
        //currentState.currentEpisode = episode;
        currentState.currentScene = scene;
        currentState.currentDialogue = dialogue;
        currentState.textCounter = textIndex;
        /// currentState.episodeNameShowed = episodeNameShowed;

        Debug.Log($"Сохранено состояние: Scene={scene}, Dialogue={dialogue}, TextCounter={textIndex}, EpisodeNameShowe=");
    }

    public GameState GetGameState()
    {
        return currentState;
    }

    #endregion

    #region Сохранение и загрузка

    public bool LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("Save file not found! Creating a default state.");
            currentState = new GameState
            {
                currentScene = "1",
                currentDialogue = "1",
                textCounter = 0,
                flags = new Dictionary<string, bool>(),
                hairIndex = 0,
                clothesIndex = 0,
                episodeNameShowed = false
            };
            return false;
        }

        string json = File.ReadAllText(saveFilePath); // Чтение JSON из файла
        currentState = JsonConvert.DeserializeObject<GameState>(json); // Десериализация JSON в объект GameState
        Debug.Log($"Game progress has been loaded.\nJSON:\n{json}");
        return true;
    }

    /*Функция может быть использована через сериализацию:

    Если она связана с объектом, который загружается или сохраняется, Unity или JSON-файлы могут использовать её для восстановления данных.
    */

    public (int, int) LoadAppearance()
    {
        return (currentState.hairIndex >= 0 ? currentState.hairIndex : 0,
                currentState.clothesIndex >= 0 ? currentState.clothesIndex : 0);
    }

    public (string, string) LoadCharacterNames()
    {
        return (currentState.leftCharacterName ?? "", currentState.rightCharacterName ?? "");
    }

    public (string, float, int, bool) LoadBackgroundAnimation()
    {
        return (
            currentState.currentBackgroundAnimation,
            currentState.animationFrameDelay,
            currentState.animationRepeatCount,
            currentState.animationKeepLastFrame
        );
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
    }

    public void SaveBackground(string backgroundName)
    {
        currentState.currentBackgroundName = backgroundName;
        Debug.Log($"Фон сохранён: {backgroundName}");
    }

    public string LoadBackground()
    {
        return currentState.currentBackgroundName;
    }


    public void SaveBackgroundAnimation(string animationName, float frameDelay, int repeatCount, bool keepLastFrame)
    {
        currentState.currentBackgroundAnimation = animationName;
        currentState.animationFrameDelay = frameDelay;
        currentState.animationRepeatCount = repeatCount;
        currentState.animationKeepLastFrame = keepLastFrame;

        Debug.Log($"Background Animation Saved: {animationName}");
    }

   

    #endregion
}
