using System.IO;
using System.Collections.Generic;
using UnityEngine;

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

    public string currentBackgroundAnimation;
    public float animationFrameDelay;
    public int animationRepeatCount;
    public bool animationKeepLastFrame;
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

        Debug.Log($"Сохранено состояние: Scene={scene}, Dialogue={dialogue}, TextCounter={textIndex}, EpisodeNameShowe={episodeNameShowed}");
    }




    public GameState GetGameState()
    {
        return currentState;
    }

    #endregion

    #region Сохранение и загрузка

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(currentState, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Игра сохранена в: {saveFilePath}");
    }

    public bool LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            currentState = JsonUtility.FromJson<GameState>(json);
            Debug.Log("Прогресс игры загружен.");
            return true;
        }

        Debug.LogWarning("Файл сохранения не найден! Создаем дефолтное состояние.");
        currentState = new GameState
        {
            currentEpisode = "1",
            currentScene = "1",
            currentDialogue = "0",
            textCounter = 0,
            flags = new Dictionary<string, bool>(),
            hairIndex = 0,
            clothesIndex = 0,
            episodeNameShowed = false // По умолчанию эпизод не показан
        };

        return false;
    }

    public void UpdateFlags(Dictionary<string, bool> flags)
    {
        currentState.flags = new Dictionary<string, bool>(flags);
    }

    public void SaveAppearance(int hairIndex, int clothesIndex)
    {
        currentState.hairIndex = hairIndex;
        currentState.clothesIndex = clothesIndex;
        Debug.Log($"Внешний вид сохранен: Волосы={hairIndex}, Одежда={clothesIndex}");
    }

    public (int, int) LoadAppearance()
    {
        return (currentState.hairIndex >= 0 ? currentState.hairIndex : 0,
                currentState.clothesIndex >= 0 ? currentState.clothesIndex : 0);
    }
    public void SaveCharacterNames(string leftCharacter, string rightCharacter)
    {
        currentState.leftCharacterName = leftCharacter;
        currentState.rightCharacterName = rightCharacter;
        Debug.Log($"Сохранены имена персонажей: Левый = {leftCharacter}, Правый = {rightCharacter}");
    }


    public (string, string) LoadCharacterNames()
    {
        return (currentState.leftCharacterName ?? "DefaultLeft", currentState.rightCharacterName ?? "DefaultRight");
    }


    public void SaveBackgroundAnimation(string animationName, float frameDelay, int repeatCount, bool keepLastFrame)
    {
        currentState.currentBackgroundAnimation = animationName;
        currentState.animationFrameDelay = frameDelay;
        currentState.animationRepeatCount = repeatCount;
        currentState.animationKeepLastFrame = keepLastFrame;

        Debug.Log($"Фоновая анимация сохранена: {animationName}");
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

    #endregion
}
