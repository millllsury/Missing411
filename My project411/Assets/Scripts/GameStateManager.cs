using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;



[System.Serializable]
public class GameState
{
    public string currentEpisode;
    public string currentScene;             // Текущая сцена
    public string currentDialogue;          // Текущий диалог
    public int textCounter;                 // Счетчик текста
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
    public List<DialogueState> dialogueHistory = new List<DialogueState>();
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

    private SaveSlots saveSlots = new SaveSlots();

    public List<SaveSlot> GetSaveSlots()
    {
        return saveSlots.slots;
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

    public void SaveSlotsToFile()
    {
        string json = JsonConvert.SerializeObject(saveSlots, Formatting.Indented);
        File.WriteAllText(slotsFilePath, json);
        Debug.Log($"Слоты сохранений записаны в файл: {slotsFilePath}");
    }

    public void SaveGameToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= saveSlots.slots.Count)
        {
            Debug.LogError("Неверный индекс слота сохранения.");
            return;
        }

        var slot = saveSlots.slots[slotIndex];
        slot.gameState = currentState; // Сохраняем текущее состояние игры
        slot.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        SaveSlotsToFile();
        Debug.Log($"Игра сохранена в слот {slot.slotName}.");
    }

    public void LoadGameFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= saveSlots.slots.Count)
        {
            Debug.LogError("Неверный индекс слота сохранения.");
            return;
        }

        var slot = saveSlots.slots[slotIndex];
        if (slot.gameState == null)
        {
            Debug.LogWarning($"Слот {slot.slotName} пуст.");
            return;
        }

        currentState = slot.gameState; // Восстанавливаем состояние игры
        Debug.Log($"Игра загружена из слота {slot.slotName}.");
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Объект сохраняется между сценами
            currentState = new GameState();
            Debug.Log("GameStateManager инициализирован.");
        }
        else
        {
            Debug.LogWarning("GameStateManager уже существует. Удаляем дубликат.");
            Destroy(gameObject);
        }
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
                currentDialogue = "0",
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

    public void SaveGame()
    {
        string json = JsonConvert.SerializeObject(currentState, Formatting.Indented); // Сериализация с форматированием
        File.WriteAllText(saveFilePath, json); // Запись JSON в файл
        Debug.Log($"Game saved: {saveFilePath}\nJSON:\n{json}");
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
