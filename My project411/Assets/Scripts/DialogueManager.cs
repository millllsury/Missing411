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
            Debug.LogError("Не удалось найти EpisodeNameScreen.");
        }
    }

    private T FindComponent<T>(string componentName) where T : Component
    {
        T component = FindFirstObjectByType<T>();
        if (component == null)
        {
            Debug.LogError($"{componentName} не найден!");
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
        // Блокируем любые действия при активном экране эпизода, выборе или недоступности ввода
        if (isEpisodeScreenActive || isChoosing || sceneController.IsTransitioning || inputUnavailable)
            return;

        // Обработка нажатия клавиш
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextDialogueText();
        }

        // Обработка клика мышью на игровом экране
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
            Debug.LogError($"Эпизод с ID {episodeId} не найден!");
            return;
        }

        LoadScene(currentEpisode.scenes[0].sceneId);
    }

    public void LoadScene(int sceneId)
    {
        currentScene = currentEpisode.scenes.Find(scene => scene.sceneId == sceneId);
        if (currentScene == null)
        {
            Debug.LogError($"Сцена с ID {sceneId} не найдена!");
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
            Debug.LogError("В сцене нет диалогов.");
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

        HandleDialogueAnimation(dialogue);
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

        if (!string.IsNullOrEmpty(dialogue.animation))
        {
            animations.PlayAnimation(GetCharacterPosition(dialogue.place), dialogue.animation, dialogue.character);
        }
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

        if (textCounter < dialogue.texts.Count)
        {
            dialogueText.text = dialogue.texts[textCounter++];
        }
        else
        {
            HandleDialogueEnd(dialogue);
        }
    }

    private void HandleDialogueEnd(Dialogue dialogue)
    {
        textCounter = 0;

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
            currentDialogueId = nextDialogue.id;
            ShowNextDialogueText();
        }
        else
        {
            Debug.LogWarning("Не найден следующий диалог. Конец сцены?");
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

        HideChoices();
        AdvanceToNextDialogue();
    }


    public void OnMainMenuClick(GameObject clickedObject)
    {
        // Проверяем, является ли клик на объекте кнопкой "Главное меню"
        if (clickedObject == toMainMenuButton.gameObject)
        {
            // Открываем подтверждение выхода в главное меню
            QuitConfirmationPanel.SetActive(true);
            Time.timeScale = 0;
            inputUnavailable = true;
            return; // Завершаем метод, чтобы не обрабатывать ShowNextDialogueText()
        }

        // Проверяем, можно ли обработать клик на игровом экране
        if (isChoosing || sceneController.IsTransitioning || inputUnavailable) return;

        // Показываем следующий текст
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
