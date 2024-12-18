using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameState
{
    public string currentEpisode;
    public string currentScene;             // ������� �����
    public string currentDialogue;          // ������� ������
    public int textCounter;                 // ������� ������
    public bool episodeNameShowed;
    public Dictionary<string, bool> flags;  // ����� ����
    public int hairIndex;                   // ������ ����� ���������
    public int clothesIndex;                // ������ ������ ���������
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
            DontDestroyOnLoad(gameObject); // ������ ����������� ����� �������
            currentState = new GameState();
            Debug.Log("GameStateManager ���������������.");
        }
        else
        {
            Debug.LogWarning("GameStateManager ��� ����������. ������� ��������.");
            Destroy(gameObject);
        }
    }

    private static string saveFilePath => Path.Combine(Application.persistentDataPath, "game_state.json");

   
    private GameState currentState;



    #region ���������� ����������

    public void UpdateSceneState(string episode, string scene, string dialogue, int textIndex, bool episodeNameShowed)
    {
        if (string.IsNullOrEmpty(scene) || string.IsNullOrEmpty(dialogue))
        {
            Debug.LogWarning("������� �������� ��������� � ������� ����������.");
            return;
        }
        currentState.currentEpisode = episode;
        currentState.currentScene = scene;
        currentState.currentDialogue = dialogue;
        currentState.textCounter = textIndex;
        currentState.episodeNameShowed = episodeNameShowed;

        Debug.Log($"��������� ���������: Scene={scene}, Dialogue={dialogue}, TextCounter={textIndex}, EpisodeNameShowe={episodeNameShowed}");
    }




    public GameState GetGameState()
    {
        return currentState;
    }

    #endregion

    #region ���������� � ��������

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(currentState, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"���� ��������� �: {saveFilePath}");
    }

    public bool LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            currentState = JsonUtility.FromJson<GameState>(json);
            Debug.Log("�������� ���� ��������.");
            return true;
        }

        Debug.LogWarning("���� ���������� �� ������! ������� ��������� ���������.");
        currentState = new GameState
        {
            currentEpisode = "1",
            currentScene = "1",
            currentDialogue = "0",
            textCounter = 0,
            flags = new Dictionary<string, bool>(),
            hairIndex = 0,
            clothesIndex = 0,
            episodeNameShowed = false // �� ��������� ������ �� �������
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
        Debug.Log($"������� ��� ��������: ������={hairIndex}, ������={clothesIndex}");
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
        Debug.Log($"��������� ����� ����������: ����� = {leftCharacter}, ������ = {rightCharacter}");
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

        Debug.Log($"������� �������� ���������: {animationName}");
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
