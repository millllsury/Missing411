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
    [SerializeField] private GameObject QuitConfirmationPanel;
    [SerializeField] private GameObject SaveConfirmationPanel;
    [SerializeField] private Button toMainMenuButton;
    [SerializeField]private Button backButton;

    public EpisodeNameScreen episodeScreen;
    [SerializeField] private Animations animations;

    private DataLoader dataLoader;
    private SceneController sceneController;
    private CharacterManager characterManager;
    private GameFlagsManager flagsManager;

    private VisualNovelData visualNovelData;
    private Episode currentEpisode;
    private SceneData currentScene;

    private int currentDialogueId;
    private int textCounter;

    private bool isChoosing = false;
    private bool isEpisodeScreenActive = false;
    private bool inputUnavailable = false;


    private Stack<(int? currentDialogueId, int textCounter)> dialogueHistory = new Stack<(int?, int)>();    // ���� ��� ������������ ��������


    void Start()
    {
        InitializeComponents();
        LoadInitialData();
    }

    private void InitializeComponents()
    {
        dataLoader = FindComponent<DataLoader>("DataLoader");
        sceneController = FindComponent<SceneController>("SceneController");
        characterManager = FindComponent<CharacterManager>("CharacterManager");
        flagsManager = FindComponent<GameFlagsManager>("FlagsManager");

        if (episodeScreen == null)
        {
            Debug.LogError("�� ������� ����� EpisodeNameScreen.");
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
        if (isEpisodeScreenActive || isChoosing || sceneController.IsTransitioning || inputUnavailable)
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

        HideChoices();
        ShowEpisodeName();
        HandleSceneBackground();
        InitializeDialogue();
    }

    

    private void ShowEpisodeName()
    {
        
        
        if (isEpisodeScreenActive) return;

        Sprite backgroundImage = Resources.Load<Sprite>(currentEpisode.backgroundImage);
        isEpisodeScreenActive = true;
        episodeScreen.ShowEpisodeScreen(currentEpisode.episodeName, backgroundImage);
        
    }

    public void SetEpisodeScreenActive(bool isActive)
    {
        isEpisodeScreenActive = isActive;
    }

    private void HandleSceneBackground()
    {
        sceneController.SetBackground(currentScene.background);
    }

    private void InitializeDialogue()
    {
        if (currentScene.dialogues == null || !currentScene.dialogues.Any())
        {
            Debug.LogError("� ����� ��� ��������.");
            return;
        }

        currentDialogueId = currentScene.dialogues[0].id;
        textCounter = 0;
        isChoosing = false;
        ShowNextDialogueText();
    }

    private void ShowNextDialogueText()
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
            sceneController.StopBackgroundAnimation();
        }

        if (!string.IsNullOrEmpty(dialogue.backgroundAnimation))
        {
            StartBackgroundAnimation(dialogue);
        }
        else if (!string.IsNullOrEmpty(dialogue.background))
        {
            sceneController.SetBackgroundSmooth(dialogue.background, dialogue.smoothBgReplacement);
        }

        // ��������, ���� �� ��� ��������� ��������
        if (!string.IsNullOrEmpty(dialogue.animation) && !dialogue.isAnimationPlayed)
        {
            animations.PlayAnimation(GetCharacterPosition(dialogue.place), dialogue.animation, dialogue.character);

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

        sceneController.StartBackgroundAnimation(dialogue.backgroundAnimation, frameDelay, repeatCount, keepLastFrame);
    }

    private Dialogue GetCurrentDialogue()
    {
        return currentScene.dialogues.FirstOrDefault(d => d.id == currentDialogueId);
    }

    private string GetCharacterPosition(int place)
    {
        return place == 1 ? "left" : "right";
    }

    private void DisplayDialogueText(Dialogue dialogue)
    {

        characterManager.SetCharacter(dialogue.speaker, dialogue.place, dialogue.isNarration, dialogue.character);

        if (dialogue.texts != null && dialogue.texts.Count > 0)
        {
            if (textCounter < dialogue.texts.Count)
            {
                dialogueText.text = dialogue.texts[textCounter];

                textCounter++; // ����������� textCounter ����� ������ ������
            }
            else
            {
                // ���� ��� ������ ��������, ��������� � ������ ��� ���������� �������
                dialogueHistory.Push((currentDialogueId, textCounter - 1)); // ��������� ���������� �����
                Debug.Log($"����������� � ����: currentDialogueId: {currentDialogueId} � textCounter: {textCounter}");
                HandleDialogueEnd(dialogue);
            }
        }
        else
        {
            // ���� � ������� ��� �������, ������ ��� ������ � �������
            if (dialogue.choices != null && dialogue.choices.Count > 0)
            {
                ShowChoices(dialogue.choices); // ���������� �����
            }
        }
        canGoBack = true;
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
    }

    private void AdvanceToNextDialogue()
    {
        var nextDialogue = FindNextDialogue();
        if (nextDialogue != null)
        {
            // ��������� ������� ��������� � ���� ����� ������� textCounter
           

            // ���������� ��������� ������� ��� ������ �������
            textCounter = 0;

            // ������������� ����� ������� ������
            currentDialogueId = nextDialogue.id;

            // ���������� ���� �������� ��� ���������� �������
            nextDialogue.isAnimationPlayed = false;

            // ������������ �������� ����� ������� ������
            HandleDialogueAnimation(nextDialogue);

            // ���������� ����� ���������� �������
            ShowNextDialogueText();
        }
        else
        {
            Debug.LogWarning("�� ������ ��������� ������. ����� �����?");
        }
    }



    private bool canGoBack = true; // ���������� �� �������

    public void GoBackOneStep(GameObject clickedObject)
    {
        if (!canGoBack) return;

        if (clickedObject == backButton.gameObject)
        {
            if (sceneController.IsTransitioning || inputUnavailable) return;

            if (isChoosing)
            {
                HideChoices(); // ���� �� ��������

            }

            // ���� �� ��������� � �������� �������� �������, ��������� textCounter
            if (textCounter > 0)
            {
                textCounter--;
                var dialogue = GetCurrentDialogue();
                if (dialogue != null)
                {
                    dialogueText.text = dialogue.texts[textCounter];

                }

            }

            // ����� ������������ � ����������� �������
            if (dialogueHistory.Count > 0)
            {
                var previousState = dialogueHistory.Pop();


                // ���� �������� �����
                if (previousState.currentDialogueId == null)
                {
                    Debug.Log("��������� ������ ������ ����� ������. ������� ����������.");
                    canGoBack = false;

                    return;
                }

                currentDialogueId = previousState.currentDialogueId.Value;
                textCounter = previousState.textCounter;

                var dialogue = GetCurrentDialogue();
                if (dialogue != null)
                {
                    dialogueText.text = dialogue.texts[textCounter];
                    characterManager.SetCharacter(dialogue.speaker, dialogue.place, dialogue.isNarration, dialogue.character);
                    HandleDialogueAnimation(dialogue);
                }
            }
            else
            {
                Debug.Log("��� ���������� �������� ��� ��������.");
            }
        }
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
        dialogueHistory.Push((null, 0));
        HideChoices();
        AdvanceToNextDialogue();
    }


    public void OnMainMenuClick(GameObject clickedObject)
    {
        // ���������, �������� �� ���� �� ������� ������� "������� ����"
        if (clickedObject == toMainMenuButton.gameObject)
        {
            // ��������� ������������� ������ � ������� ����
            QuitConfirmationPanel.SetActive(true);
            Time.timeScale = 0;
            inputUnavailable = true;
            return; // ��������� �����, ����� �� ������������ ShowNextDialogueText()
        }

        // ���������, ����� �� ���������� ���� �� ������� ������
        if (isChoosing || sceneController.IsTransitioning || inputUnavailable) return;

        // ���������� ��������� �����
        ShowNextDialogueText();
    }

    public void GoToMainMenuConfirmation()
    {
        QuitConfirmationPanel.SetActive(false);
        SaveConfirmationPanel.SetActive(true);      
    }

    public void SaveConfirmation()
    {
        QuitConfirmationPanel.SetActive(false);
        SaveConfirmationPanel.SetActive(false);
        Time.timeScale = 1;
        inputUnavailable = false;
        SceneManager.LoadScene("MainMenu");

    }

    public void SaveRejection()
    {
        QuitConfirmationPanel.SetActive(false);
        SaveConfirmationPanel.SetActive(false);
        Time.timeScale = 1;
        inputUnavailable = false;
        SceneManager.LoadScene("MainMenu");

    }

    public void GoToMainMenuRejection()
    {
        QuitConfirmationPanel.SetActive(false);
        SaveConfirmationPanel.SetActive(false);
        Time.timeScale = 1;
        inputUnavailable = false;
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
