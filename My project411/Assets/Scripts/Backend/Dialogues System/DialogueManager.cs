using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using Unity.VisualScripting;


public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject dialogueTextPanel;
    [SerializeField] private Button[] optionButtons;
   
    [SerializeField] private Button backButton;

    public UIManager UIManager;
    [SerializeField] private Animations animations;

    private DataLoader dataLoader;
    private BackgroundController backgroundController;
    private CharacterManager characterManager;
    private GameFlagsManager flagsManager;


    private VisualNovelData visualNovelData;
    private Episode currentEpisode;
    private SceneData currentScene;

    private int currentDialogueId=1;
    private int textCounter = 0;

    public bool isChoosing = false;
    public bool isEpisodeScreenActive = false;
    public bool inputUnavailable = false;

    private bool EpisodeNameShowed = false; // �� ��������� �� ��������


    private bool canGoBack = true; // ���������� �� �������
    private Stack<(int? currentDialogueId, int textCounter)> dialogueHistory = new Stack<(int?, int)>();    // ���� ��� ������������ ��������

    void Start()
    {
        InitializeComponents();
        LoadInitialData();
        LoadProgress(); // ������������� ��������� ���������, ���� ������ ����
        
    }

    private void InitializeComponents()
    {
        dataLoader = FindComponent<DataLoader>("DataLoader");
        backgroundController = FindComponent<BackgroundController>("BackgroundController");
        characterManager = FindComponent<CharacterManager>("CharacterManager");
        flagsManager = FindComponent<GameFlagsManager>("FlagsManager");

        if (UIManager == null)
        {
            Debug.LogError("�� ������� ����� UIManager.");
        }
    }

    private void LoadInitialData()
    {
        visualNovelData = dataLoader.LoadData("Episode 1");
        if (visualNovelData == null || visualNovelData.episodes == null)
        {
            Debug.LogError("������ �������� JSON! ���������, ��� ���� ����������.");
            return;
        }

        Debug.Log($"Num of episodes: {visualNovelData.episodes.Count}");
        foreach (var episode in visualNovelData.episodes)
        {
            Debug.Log($"Episode {episode.episodeId} contains {episode.scenes.Count} scene(s).");
            foreach (var scene in episode.scenes)
            {
                Debug.Log($"Scene {scene.sceneId}, Dialogues: {scene.dialogues?.Count ?? 0}");
            }
        }
        characterManager.HideAvatars();
        LoadEpisode(1);
    }

    private T FindComponent<T>(string componentName) where T : Component
    {
        T component = FindFirstObjectByType<T>();
        if (component == null)
        {
            Debug.LogError($"{componentName} hasn't been found!");
        }
        return component;
    }


    void Update()
    {
        // ��������� ����� �������� ��� �������� ������ �������, ������ ��� ������������� �����
        if (isEpisodeScreenActive || isChoosing || backgroundController.IsTransitioning || inputUnavailable)
            return;

        // ��������� ������� ������
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextDialogueText();
        }

        // ��������� ����� ����� �� ������� ������
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            ShowNextDialogueText();
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void LoadEpisode(int episodeId)
    {
        currentEpisode = visualNovelData.episodes.Find(e => e.episodeId == episodeId);
        if (currentEpisode == null)
        {
            Debug.LogError($"������ � ID {episodeId} �� ������!");
            return;
        }

        LoadScene(currentEpisode.scenes[0].sceneId);
    }

    public void LoadScene(int sceneId)
    {

        currentScene = currentEpisode.scenes.Find(scene => scene.sceneId == sceneId);
        if (currentScene == null)
        {
            Debug.LogError($"����� � ID {sceneId} �� �������!");
            return;
        }

        // ���� ������� ������ ��� ����������, ���������� ���
        if (currentDialogueId > 0)
        {
            InitializeDialogue(currentDialogueId, textCounter);
        }
        else
        {
            // ���� ������� ������ �� �����, �������� � �������
            currentDialogueId = currentScene.dialogues[0].id;
            textCounter = 0;
            InitializeDialogue(currentDialogueId, textCounter);
            //ShowEpisodeName();
            HandleSceneBackground();
        }
        HideChoices();
    }


    private void ShowEpisodeName()
    {
        if (EpisodeNameShowed) return; // ���� ��� ����������, �� ���������� �����

        if (isEpisodeScreenActive) return; // ���������, ������� �� ��� �����

        Sprite backgroundImage = Resources.Load<Sprite>(currentEpisode.backgroundImage);
        isEpisodeScreenActive = true;
        UIManager.ShowEpisodeScreen(currentEpisode.episodeName, backgroundImage);

        EpisodeNameShowed = true; // ������������� ����, ��� ����� ��� �������
    }


    public void SetEpisodeScreenActive(bool isActive)
    {
        isEpisodeScreenActive = isActive;
    }

    private void HandleSceneBackground()
    {
        backgroundController.SetBackground(currentScene.background);
    }

    private void InitializeDialogue(int startingDialogueId, int startingTextCounter)
    {
        if (currentScene.dialogues == null || !currentScene.dialogues.Any())
        {
            Debug.LogError("� ����� ��� ��������.");
            return;
        }

        // ������������� ������� ������ � ������� ������
        currentDialogueId = startingDialogueId;
        textCounter = startingTextCounter;
        isChoosing = false;

        var dialogue = currentScene.dialogues.FirstOrDefault(d => d.id == currentDialogueId);
        if (dialogue != null)
        {
            if (textCounter >= dialogue.texts.Count)
            {
                Debug.LogWarning($"TextCounter ({textCounter}) ������� �� ������� ������ �������. ���������� �� 0.");
                textCounter = 0;
            }

            // ������������ ����� � ��������, ���� ��� �������
            //HandleDialogueSound(dialogue);
            HandleDialogueAnimation(dialogue);
            DisplayDialogueText(dialogue); // ���������� ������� �����
        }
        else
        {
            Debug.LogWarning($"������ � ID {startingDialogueId} �� ������.");
        }
    }

    public void ShowNextDialogueText()
    {
        var dialogue = GetCurrentDialogue();
        if (dialogue == null) return;
        Debug.Log($"Dialogue: {currentDialogueId} and textCounter: {textCounter}");
        DisplayDialogueText(dialogue);
    }

    private void HandleDialogueAnimation(Dialogue dialogue)
    {
        if (dialogue.stopBackgroundAnimation == true)
        {
            backgroundController.StopBackgroundAnimation();
        }

        if (!string.IsNullOrEmpty(dialogue.backgroundAnimation))
        {
            StartBackgroundAnimation(dialogue);
        }
        else if (!string.IsNullOrEmpty(dialogue.background))
        {
            backgroundController.SetBackgroundSmooth(dialogue.background, dialogue.smoothBgReplacement);
        }

        // ��������, ���� �� ��� ��������� ��������
        if (!string.IsNullOrEmpty(dialogue.animation) && !dialogue.isAnimationPlayed)
        {
            animations.PlayAnimation(GetCharacterPosition(dialogue.place), dialogue.animation, dialogue.character);

            Debug.Log($"HandleDialogueAnimation ������ � animationName: {dialogue.animation}");

            // ������������� ����, ��� �������� ���������
            dialogue.isAnimationPlayed = true;
        }

    }

    // ���� ����� �������� ���� � ������ �����, ��������, ����� ���������� �������
    public void ResetAnimationFlag(Dialogue dialogue)
    {
        dialogue.isAnimationPlayed = false;
    }


    private void StartBackgroundAnimation(Dialogue dialogue)
    {
        AnimationPresetManager presetManager = FindFirstObjectByType<AnimationPresetManager>();
        AnimationPreset preset = presetManager?.GetPreset(dialogue.backgroundAnimation);

        float frameDelay = preset?.frameDelay ?? dialogue.frameDelay;
        int repeatCount = preset?.repeatCount ?? dialogue.repeatCount;
        bool keepLastFrame = preset?.keepLastFrame ?? dialogue.keepLastFrame ?? false;
        string soundName = preset?.soundName ?? null; 

        backgroundController.StartBackgroundAnimation(dialogue.backgroundAnimation, frameDelay, repeatCount, keepLastFrame, soundName);
    }

    private Dialogue GetCurrentDialogue()
    {
        return currentScene.dialogues.FirstOrDefault(d => d.id == currentDialogueId);
    }


    private string GetCharacterPosition(int place)
    {
        return place == 1 ? "left" : "right";
    }

    bool goBackButtonActivated = false;

    private void DisplayDialogueText(Dialogue dialogue)
    {
        characterManager.SetCharacter(dialogue.speaker, dialogue.place, dialogue.isNarration, dialogue.character);

        if (dialogue.texts != null && dialogue.texts.Count > 0)
        {
            if (textCounter < dialogue.texts.Count)
            {

                if (goBackButtonActivated == true)
                {
                    if (textCounter >= 0 && textCounter < dialogue.texts.Count)
                    {
                        dialogueText.text = dialogue.texts[textCounter++];
                    }
                    else
                    {
                        HandleDialogueEnd(dialogue);
                        return;
                    }
                }
                else if (textCounter == 0)
                {

                    dialogueText.text = dialogue.texts[textCounter];
                    textCounter++;

                }
                else
                {
                   // Debug.Log($"���� 1.2 textCounter: {textCounter}");
                    dialogueHistory.Push((currentDialogueId, textCounter - 1));
                    //Debug.Log($"����������� � ����: previousDialogueId={currentDialogueId}, textCounter={textCounter - 1}");
                    dialogueText.text = dialogue.texts[textCounter];
                    textCounter++;
                    UpdateBackButtonState(true);
                   // Debug.Log($"���� 1.2 textCounter: {textCounter}");
                }
            }
            else
            {
                //Debug.Log($"����2 - dialogue.texts.Count: {dialogue.texts.Count} and textCounter: {textCounter}");
                // ��������� ��������� ����� ��������� � ���������� �������
                dialogueHistory.Push((currentDialogueId, textCounter - 1));
                //Debug.Log($"����������� � ����: previousDialogueId={currentDialogueId}, textCounter={textCounter - 1}");

                HandleDialogueEnd(dialogue);
            }
        }
        else if (dialogue.choices != null && dialogue.choices.Count > 0)
        {
            ShowChoices(dialogue.choices);
        }

        canGoBack = true; // ��������� ������� ����� ��������� ������
        goBackButtonActivated = false;
    }


    private void HandleDialogueEnd(Dialogue dialogue)
    {
        textCounter = 0;
        ResetAnimationFlag(dialogue);

        if (dialogue.choices != null && dialogue.choices.Any())
        {
            ShowChoices(dialogue.choices);
        }
        else
        {
            AdvanceToNextDialogue();
        }
        goBackButtonActivated = false;
    }

    private void AdvanceToNextDialogue()
    {
        var nextDialogue = FindNextDialogue();
        if (nextDialogue != null)
        {

            // ���������� ��������� ������� ��� ������ �������
            textCounter = 0;

            // ������������� ����� ������� ������
            currentDialogueId = nextDialogue.id;

            // ���������� ���� �������� ��� ���������� �������
            nextDialogue.isAnimationPlayed = false;

            // ������������ �������� ����� ������� ������
            HandleDialogueAnimation(nextDialogue);

            HandleDialogueSound(nextDialogue);
            UpdateBackButtonState(true);

            // ���������� ����� ���������� �������
            ShowNextDialogueText();
        }
        else
        {
            Debug.LogWarning("�� ������ ��������� ������. ������� � ��������� �����.");
            var currentSceneIndex = currentEpisode.scenes.IndexOf(currentScene);

            if (currentSceneIndex >= 0 && currentSceneIndex < currentEpisode.scenes.Count - 1)
            {
                // ����� ��������� �����
                var nextScene = currentEpisode.scenes[currentSceneIndex + 1];
                // ��������� �������� ��� ��������
                GameStateManager.Instance.UpdateFlags(flagsManager.GetAllFlags());
                var (currentHairIndex, currentClothesIndex) = GameStateManager.Instance.LoadAppearance();
                GameStateManager.Instance.SaveAppearance(currentHairIndex, currentClothesIndex);
                GameStateManager.Instance.SaveCharacterNames(
                    null,
                    null
                    );
                GameStateManager.Instance.UpdateSceneState(
                    nextScene.sceneId.ToString(),  // ��������� �����
                    "1",                          // ������ ���������� � 1
                    0                             // ����� ���������� ��������
                );

                dialogueHistory.Clear(); // ������� ����
                var dialogueHistoryList = new List<DialogueState>(); // ������ ����� ������ ������
                GameStateManager.Instance.GetGameState().dialogueHistory = dialogueHistoryList; // ����������� ����� ������ ������

                GameStateManager.Instance.GetGameState().currentBackgroundAnimation = null;
                GameStateManager.Instance.SaveBackground(null);
                int selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();
                GameStateManager.Instance.SaveGameToSlot(selectedSlotIndex);
                // ��� ��� �������
                Debug.Log($"��������� ��������� ��� ��������. Scene={nextScene.sceneId}, Dialogue=0, TextCounter=0");

                characterManager.HideAvatars();
                // ������� � ��������� �����
                SceneManager.LoadScene("Scene" + nextScene.sceneId);
                LoadScene(nextScene.sceneId);
            }
            else
            {
                Debug.LogWarning("��� ��������� ����� �������. ���������� �������.");
                EndEpisode();
            }
        }
    }

    private void EndEpisode()
    {
        Debug.Log("������ ��������. ������� � ������ ���������� ������� ��� ���������� �������.");

        // ����� ����� �������� ����� ���������� ������� ��� ��������� �������
        // ��������, ������� � ������� ����:
        SceneManager.LoadScene("MainMenu"); // ���������, ��� ����� "MainMenu" ��������� � Build Settings
    }


    private void HandleDialogueSound(Dialogue dialogue)
    {
        if (!string.IsNullOrEmpty(dialogue.soundTrigger))
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.HandleSoundTrigger(dialogue.soundTrigger);
            }
            else
            {
                Debug.LogError("SoundManager.Instance is null!");
            }
        }
    }




    public void GoBackOneStep(GameObject clickedObject)
    {
        if (!canGoBack) return; // ���� ������� ����������, ������ �� ������.

        if (clickedObject == backButton.gameObject)
        {
            if (backgroundController.IsTransitioning || inputUnavailable) return;

            if (isChoosing)
            {
                HideChoices();
                return;
            }

            if (dialogueHistory.Count == 0)
            {
                Debug.Log("������� �����. ������� ����������.");
                UpdateBackButtonState(false);
                return;
            }

            // ��������� ���������� ��������� �� �����
            var previousState = dialogueHistory.Pop();

            // ���������, ��� �� ���������� ������ ������ ����� ������
            if (previousState.currentDialogueId == null && previousState.textCounter == 0)
            {
                Debug.Log("��������� ������ ������ ����� ������. ��������� ������ ��������.");
                UpdateBackButtonState(false);
                return;
            }

            // ��������������� ��������� �������
            currentDialogueId = previousState.currentDialogueId.Value;
            textCounter = previousState.textCounter;

            Debug.Log($"������������� ���������: previousDialogueId={currentDialogueId}, textCounter={textCounter}");

            // �������������� ����� � ����������
            UpdateDialogueText();

            goBackButtonActivated = true;
            // ��������� ��������� ������
            UpdateBackButtonState(dialogueHistory.Count > 0);
        }
    }

    private void UpdateDialogueText()
    {
        var dialogue = GetCurrentDialogue();
        if (dialogue != null)
        {
            dialogueText.text = dialogue.texts[textCounter];
            Debug.Log($"�������������: Dialogue: {currentDialogueId} and textCounter: {textCounter}");
            characterManager.SetCharacter(dialogue.speaker, dialogue.place, dialogue.isNarration, dialogue.character);
            HandleDialogueAnimation(dialogue);
        }
        
    }

    public void UpdateBackButtonState(bool state)
    {
        backButton.interactable = state;
        canGoBack = state;
    }


    private Dialogue FindNextDialogue()
    {
        return currentScene.dialogues
            .Where(d => d.id > currentDialogueId && flagsManager.AreConditionsMet(d.conditions))
            .FirstOrDefault();
    }

    private void ShowChoices(List<Choice> choices)
    {
        dialogueTextPanel.SetActive(false);
        isChoosing = true;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < choices.Count)
            {
                optionButtons[i].Configure(choices[i], OnChoiceSelected);
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void HideChoices()
    {
        foreach (var button in optionButtons)
        {
            button.gameObject.SetActive(false);
        }

        dialogueTextPanel.SetActive(true);
        isChoosing = false;
    }

    public void OnChoiceSelected(Choice choice)
    {
        if (choice.actions != null)
        {
            foreach (var action in choice.actions)
            {
                flagsManager.SetFlag(action.key, action.value);
            }
        }

        // ������� ���� �������
        dialogueHistory.Clear();

        Debug.Log("���������� ����� ����� ������: " + string.Join(" -> ", dialogueHistory.Select(d => $"[ID={d.currentDialogueId}, TextCounter={d.textCounter}]")));


        HideChoices();
        AdvanceToNextDialogue();
    }


    public void SaveProgress()
    {
        // �������� ������� ������ ���������� �����
        int selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();

        if (selectedSlotIndex == -1)
        {
            Debug.LogError("���� ��� ���������� �� ������. ���������� ����������.");
            return;
        }

        // ��������� ��������� ������� �����
        int currentTextCounter = textCounter - 1;
        GameStateManager.Instance.UpdateSceneState(
            currentScene.sceneId.ToString(),
            currentDialogueId.ToString(),
            currentTextCounter 

        );

        // ��������� �����
        GameStateManager.Instance.UpdateFlags(flagsManager.GetAllFlags());

        // ��������� ������� ��� ����������
        var (currentHairIndex, currentClothesIndex) = GameStateManager.Instance.LoadAppearance();
        GameStateManager.Instance.SaveAppearance(currentHairIndex, currentClothesIndex);

        // ��������� ����� ����������
        string leftCharacter = characterManager.GetCurrentLeftCharacter();
        string rightCharacter = characterManager.GetCurrentRightCharacter();
        GameStateManager.Instance.SaveCharacterNames(leftCharacter, rightCharacter);

        // ��������� ��� � ��������
        var animationName = backgroundController.GetCurrentAnimationName();
        var frameDelay = backgroundController.GetCurrentFrameDelay();
        var repeatCount = backgroundController.GetCurrentRepeatCount();
        var keepLastFrame = backgroundController.GetKeepLastFrame();
        var backgroundName = backgroundController.GetCurrentBackgroundName(); // �������� ����� � BackgroundController
        GameStateManager.Instance.SaveBackground(backgroundName); // ��������� ��� ����
        GameStateManager.Instance.SaveBackgroundAnimation(animationName, frameDelay, repeatCount, keepLastFrame);


        if (SceneManager.GetActiveScene().name != "WardrobeScene" && SceneManager.GetActiveScene().name != "MainMenu")
        {
            GameStateManager.Instance.SavePlayingTracks();
        }

        // ��������� ������� �������� �� ������
        var dialogueHistoryList = dialogueHistory.Reverse()
       .Select(item => new DialogueState(item.currentDialogueId ?? -1, item.textCounter))
       .ToList();

        Debug.Log("����������� ���� ������� (����): " +
            string.Join(" -> ", dialogueHistoryList.Select(d => $"(ID={d.dialogueId}, TextCounter={d.textCounter})")));

        GameStateManager.Instance.GetGameState().dialogueHistory = dialogueHistoryList;
        
        GameStateManager.Instance.SaveGameToSlot(selectedSlotIndex);
        Debug.Log($"�������� ���� �������� � ���� {selectedSlotIndex + 1}.");


    }



    public void LoadProgress()
    {

        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager.Instance �� ���������������.");
            return;
        }

        var loadedState = GameStateManager.Instance.GetGameState();
        Debug.Log($"The progress are: Scene={loadedState.currentScene}, Dialogue={loadedState.currentDialogue}, TextCounter={loadedState.textCounter}");


        if (loadedState == null)
        {
            Debug.LogError("GameState is null.");
            return;
        }

        Debug.Log("Loaded path from dialodue. GameState: " +
                (loadedState.dialogueHistory != null
                    ? string.Join(" -> ", loadedState.dialogueHistory.Select(d => $"(ID={d.dialogueId}, TextCounter={d.textCounter})"))
                    : "�����"));

        // ������� ���� ����� ���������������
        dialogueHistory.Clear();

        // ��������������� ���� �� ������� (��� Reverse)
        if (loadedState.dialogueHistory != null)
        {
            foreach (var item in loadedState.dialogueHistory)
            {
                dialogueHistory.Push((item.dialogueId == -1 ? null : (int?)item.dialogueId, item.textCounter));
                Debug.Log($"Stack element was added: ID={item.dialogueId}, TextCounter={item.textCounter}");
            }
        }

        // �������� �������� ���������� �����
        Debug.Log("�������� ���������� ����� ����� ��������������: " +
            string.Join(" -> ", dialogueHistory.Select(d => $"[ID={d.currentDialogueId}, TextCounter={d.textCounter}]")));

        // ��������������� ����� � ������
        Debug.Log($"Scene {loadedState.currentScene} and dialogue {loadedState.currentDialogue} are loading.");
        LoadScene(int.Parse(loadedState.currentScene));
        InitializeDialogue(int.Parse(loadedState.currentDialogue), loadedState.textCounter);

        // �������������� ������ ������, ����� ��� ���, ��������� � �����
        RestoreGameState(loadedState);

        

    }

    private void RestoreGameState(GameState loadedState)
    {
        // �������������� ������
        flagsManager.SetAllFlags(loadedState.flags);

        // �������������� ����
        var backgroundName = GameStateManager.Instance.LoadBackground();
        if (!string.IsNullOrEmpty(backgroundName))
        {
            backgroundController.SetBackground(backgroundName);
        }

        // �������������� ��������
        var (animationName, frameDelay, repeatCount, keepLastFrame) = GameStateManager.Instance.LoadBackgroundAnimation();
        if (!string.IsNullOrEmpty(animationName))
        {
            backgroundController.StartBackgroundAnimation(animationName, frameDelay, repeatCount, keepLastFrame, null);
        }

        // �������������� ����������
        characterManager.LoadCharacters();
        characterManager.LoadAppearance();

        GameStateManager.Instance.LoadPlayingTracks();

    }



}



public static class ButtonExtensions
{
    public static void Configure(this Button button, Choice choice, UnityAction<Choice> onClick)
    {
        button.gameObject.SetActive(true);
        button.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(choice));
    }
}
