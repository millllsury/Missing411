using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;



[System.Serializable]
public class GameState
{
    public string currentEpisode;
    public string currentScene = "1";             // ������� �����
    public string currentDialogue = "1";          // ������� ������
    public int textCounter = 0;                 // ������� ������
    public bool episodeNameShowed;
    public Dictionary<string, bool> flags;  // ����� ����
    public int hairIndex;                   // ������ ����� ���������
    public int clothesIndex;                // ������ ������ ���������
    public string leftCharacterName;
    public string rightCharacterName;

    public string currentBackgroundName; 
    public string currentBackgroundAnimation;
    public float animationFrameDelay;
    public int animationRepeatCount;
    public bool animationKeepLastFrame;
    public List<DialogueState> dialogueHistory = new List<DialogueState>(); //��� dialogueHistory �����
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
    public string slotName;      // �������� ����� (��������, "���� 1")
    public string saveDate;      // ���� ����������
    public GameState gameState;  // ��������� ���� � ���� �����
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
            DontDestroyOnLoad(gameObject); // ������ ����������� ����� �������
            currentState = new GameState();
            Debug.Log("GameStateManager has been initialized.");
        }
        else
        {
            Debug.LogWarning("GameStateManager already exists. Delete duplicate.");
            Destroy(gameObject); // ������� ��������
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


    private int selectedSlotIndex = -1; // ������ ���������� ����� (-1, ���� ���� �� ������)

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= saveSlots.slots.Count)
        {
            Debug.LogError($"������ ����� {slotIndex} ��� ���������.");
            return;
        }

        var selectedSlot = saveSlots.slots[slotIndex];
        if (selectedSlot.gameState == null)
        {
            Debug.LogWarning($"���� {slotIndex + 1} ����.");
            return;
        }

        selectedSlotIndex = slotIndex;
        currentState = selectedSlot.gameState; // ������������� ������� ���������
        Debug.Log($"���� {slotIndex + 1} ������. ��������� ���� ���������.");
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
            Debug.Log("����� ���������� ���������.");
        }
        else
        {
            saveSlots = new SaveSlots { slots = new List<SaveSlot>() };
            // ������ ������ �����
            for (int i = 1; i <= 6; i++)
            {
                saveSlots.slots.Add(new SaveSlot
                {
                    slotName = $"���� {i}",
                    saveDate = null,
                    gameState = null
                });
            }
            SaveSlotsToFile();
            Debug.Log("������� ������ ����� ����������.");
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
        slot.gameState = currentState; // ��������� ������� ��������� ����
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

        currentState = slot.gameState; // ��������������� ��������� ����
        Debug.Log($"Game loaded from slot {slot.slotName}.");
    }


   

    private static string saveFilePath => Path.Combine(Application.persistentDataPath, "game_state.json");

   
    private GameState currentState;



    #region ���������� ����������

    public void UpdateSceneState(/*string episode,*/ string scene, string dialogue, int textIndex/*, bool episodeNameShowed*/)
    {
        if (string.IsNullOrEmpty(scene) || string.IsNullOrEmpty(dialogue))
        {
            Debug.LogWarning("������� �������� ��������� � ������� ����������.");
            return;
        }
        //currentState.currentEpisode = episode;
        currentState.currentScene = scene;
        currentState.currentDialogue = dialogue;
        currentState.textCounter = textIndex;
        /// currentState.episodeNameShowed = episodeNameShowed;

        Debug.Log($"��������� ���������: Scene={scene}, Dialogue={dialogue}, TextCounter={textIndex}, EpisodeNameShowe=");
    }

    public GameState GetGameState()
    {
        return currentState;
    }

    #endregion

    #region ���������� � ��������

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

        string json = File.ReadAllText(saveFilePath); // ������ JSON �� �����
        currentState = JsonConvert.DeserializeObject<GameState>(json); // �������������� JSON � ������ GameState
        Debug.Log($"Game progress has been loaded.\nJSON:\n{json}");
        return true;
    }

    /*������� ����� ���� ������������ ����� ������������:

    ���� ��� ������� � ��������, ������� ����������� ��� �����������, Unity ��� JSON-����� ����� ������������ � ��� �������������� ������.
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
        Debug.Log($"AppearanceSaved: ������={hairIndex}, ������={clothesIndex}");
    }

    public void SaveCharacterNames(string leftCharacter, string rightCharacter)
    {
        currentState.leftCharacterName = leftCharacter;
        currentState.rightCharacterName = rightCharacter;
        Debug.Log($"Names Saved: ����� = {leftCharacter}, ������ = {rightCharacter}");
    }

    public void SaveBackground(string backgroundName)
    {
        currentState.currentBackgroundName = backgroundName;
        Debug.Log($"��� �������: {backgroundName}");
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
