using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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
    [SerializeField] private SoundManager soundManager;


    private VisualNovelData visualNovelData;
    private Episode currentEpisode;
    private SceneData currentScene;

    private int currentDialogueId;
    private int textCounter=-1;

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
        LoadProgress();
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

    private T FindComponent<T>(string componentName) where T : Component
    {
        T component = FindFirstObjectByType<T>();
        if (component == null)
        {
            Debug.LogError($"{componentName} �� ������!");
        }
        return component;
    }
    private void LoadInitialData()
    {
        visualNovelData = dataLoader.LoadData("dialogues");
        characterManager.HideAvatars();
        LoadEpisode(1);
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
            ShowEpisodeName();
            HandleSceneBackground();
        }
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

        var dialogue = GetCurrentDialogue();
        if (dialogue != null)
        {
            if (textCounter >= dialogue.texts.Count)
            {
                Debug.LogWarning($"TextCounter ({textCounter}) ������� �� ������� ������ �������. ���������� �� 0.");
                textCounter = 0;
            }

            // ������������ ����� � ��������, ���� ��� �������
            HandleDialogueSound(dialogue);
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

        backgroundController.StartBackgroundAnimation(dialogue.backgroundAnimation, frameDelay, repeatCount, keepLastFrame);
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
        Debug.Log($"DisplayDialogueText: Dialogue: {currentDialogueId} and textCounter: {textCounter}");
        characterManager.SetCharacter(dialogue.speaker, dialogue.place, dialogue.isNarration, dialogue.character);

        if (dialogue.texts != null && dialogue.texts.Count > 0)
        {  
            if (textCounter < dialogue.texts.Count)
            {
                //Debug.Log($"����1 - dialogue.texts.Count: {dialogue.texts.Count} and textCounter: {textCounter}");

                if(goBackButtonActivated == true)
                {
                    dialogueText.text = dialogue.texts[textCounter++];
                }

                if(textCounter == 0)
                {
                    //Debug.Log($"���� 1.1 textCounter: {textCounter}");
                    dialogueText.text = dialogue.texts[textCounter];
                    textCounter++;
                    //Debug.Log($"���� 1.1 textCounter: {textCounter}");
                }
                else {
                   // Debug.Log($"���� 1.2 textCounter: {textCounter}");
                    dialogueHistory.Push((currentDialogueId, textCounter - 1));
                    Debug.Log($"����������� � ����: previousDialogueId={currentDialogueId}, textCounter={textCounter-1}");
                    dialogueText.text = dialogue.texts[textCounter];
                    textCounter++;
                    UpdateBackButtonState(true);
                    //Debug.Log($"���� 1.2 textCounter: {textCounter}");
                }               
            }
            else
            {
               // Debug.Log($"����2 - dialogue.texts.Count: {dialogue.texts.Count} and textCounter: {textCounter}");
                // ��������� ��������� ����� ��������� � ���������� �������
                dialogueHistory.Push((currentDialogueId, textCounter - 1));
                Debug.Log($"����������� � ����: previousDialogueId={currentDialogueId}, textCounter={textCounter - 1}");

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

            Debug.LogWarning("�� ������ ��������� ������. ����� �����?");
        }
    }

    private void HandleDialogueSound(Dialogue dialogue)
    {
        if (!string.IsNullOrEmpty(dialogue.soundTrigger))
        {
            soundManager.HandleSoundTrigger(dialogue.soundTrigger);
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
            characterManager.SetCharacter(dialogue.speaker, dialogue.place, dialogue.isNarration, dialogue.character);
            HandleDialogueAnimation(dialogue);
        }
        goBackButtonActivated = true;
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

        // ��������� ������� ��������� ��� "����� ������"
        //dialogueHistory.Push((null, 0)); // ���������, ��� ����� ��� ������
        Debug.Log("���� ������� ������ � ��������� ����� ������.");

        HideChoices();
        AdvanceToNextDialogue();
    }


    public void SaveProgress()
    {
        int currentTextCounter = textCounter - 1;
        GameStateManager.Instance.UpdateSceneState(
            currentScene.sceneId.ToString(),
            currentDialogueId.ToString(),
            currentTextCounter 

        );
        GameStateManager.Instance.UpdateFlags(flagsManager.GetAllFlags());

        var (currentHairIndex, currentClothesIndex) = GameStateManager.Instance.LoadAppearance();
        GameStateManager.Instance.SaveAppearance(currentHairIndex, currentClothesIndex);

        string leftCharacter = characterManager.GetCurrentLeftCharacter();
        string rightCharacter = characterManager.GetCurrentRightCharacter();
        GameStateManager.Instance.SaveCharacterNames(leftCharacter, rightCharacter);

        var animationName = backgroundController.GetCurrentAnimationName();
        var frameDelay = backgroundController.GetCurrentFrameDelay();
        var repeatCount = backgroundController.GetCurrentRepeatCount();
        var keepLastFrame = backgroundController.GetKeepLastFrame();

        GameStateManager.Instance.SaveBackgroundAnimation(animationName, frameDelay, repeatCount, keepLastFrame);


        GameStateManager.Instance.SaveGame();
        Debug.Log("�������� ���� �������� ����� GameStateManager.");
    }



    public void LoadProgress()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager.Instance �� ���������������! ���������, ��� ������ GameStateManager ������������ �� �����.");
            return;
        }

        if (GameStateManager.Instance.LoadGame())
        {
            var loadedState = GameStateManager.Instance.GetGameState();

            if (loadedState == null)
            {
                Debug.LogError("����������� GameState ����� null.");
                return;
            }

            // �������� � �������������� ������
            if (loadedState.flags == null)
            {
                Debug.LogWarning("����� �����������. ������� ������ �������.");
                loadedState.flags = new Dictionary<string, bool>();
            }

            flagsManager.SetAllFlags(loadedState.flags);

            // �������������� ����� � �������
            LoadScene(int.Parse(loadedState.currentScene));
            InitializeDialogue(int.Parse(loadedState.currentDialogue), loadedState.textCounter);

            var (animationName, frameDelay, repeatCount, keepLastFrame) = GameStateManager.Instance.LoadBackgroundAnimation();
            if (!string.IsNullOrEmpty(animationName))
            {
                backgroundController.StartBackgroundAnimation(animationName, frameDelay, repeatCount, keepLastFrame);
            }

            characterManager.LoadCharacters();
            characterManager.LoadAppearance();
        }
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
