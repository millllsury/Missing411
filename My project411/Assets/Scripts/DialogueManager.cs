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


    private Stack<(int? currentDialogueId, int textCounter)> dialogueHistory = new Stack<(int?, int)>();    // Стек для отслеживания диалогов


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

        // Проверка, была ли уже проиграна анимация
        if (!string.IsNullOrEmpty(dialogue.animation) && !dialogue.isAnimationPlayed)
        {
            animations.PlayAnimation(GetCharacterPosition(dialogue.place), dialogue.animation, dialogue.character);

            // Устанавливаем флаг, что анимация проиграна
            dialogue.isAnimationPlayed = true;
        }

    }

    // Если нужно сбросить флаг в другом месте, например, после завершения диалога
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

                textCounter++; // увеличиваем textCounter после показа текста
            }
            else
            {
                // Если все тексты выведены, переходим к выбору или следующему диалогу
                dialogueHistory.Push((currentDialogueId, textCounter - 1)); // сохраняем предыдущий текст
                Debug.Log($"Сохранилось в стек: currentDialogueId: {currentDialogueId} и textCounter: {textCounter}");
                HandleDialogueEnd(dialogue);
            }
        }
        else
        {
            // Если в диалоге нет текстов, значит это диалог с выбором
            if (dialogue.choices != null && dialogue.choices.Count > 0)
            {
                ShowChoices(dialogue.choices); // Показываем выбор
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
            // Сохраняем текущее состояние в стек перед сбросом textCounter
           

            // Сбрасываем текстовый счётчик для нового диалога
            textCounter = 0;

            // Устанавливаем новый текущий диалог
            currentDialogueId = nextDialogue.id;

            // Сбрасываем флаг анимации для следующего диалога
            nextDialogue.isAnimationPlayed = false;

            // Обрабатываем анимацию перед показом текста
            HandleDialogueAnimation(nextDialogue);

            // Показываем текст следующего диалога
            ShowNextDialogueText();
        }
        else
        {
            Debug.LogWarning("Не найден следующий диалог. Конец сцены?");
        }
    }



    private bool canGoBack = true; // Разрешение на возврат

    public void GoBackOneStep(GameObject clickedObject)
    {
        if (!canGoBack) return;

        if (clickedObject == backButton.gameObject)
        {
            if (sceneController.IsTransitioning || inputUnavailable) return;

            if (isChoosing)
            {
                HideChoices(); // если мы выбираем

            }

            // Если мы находимся в середине текущего диалога, уменьшаем textCounter
            if (textCounter > 0)
            {
                textCounter--;
                var dialogue = GetCurrentDialogue();
                if (dialogue != null)
                {
                    dialogueText.text = dialogue.texts[textCounter];

                }

            }

            // Иначе возвращаемся к предыдущему диалогу
            if (dialogueHistory.Count > 0)
            {
                var previousState = dialogueHistory.Pop();


                // Если достигли метки
                if (previousState.currentDialogueId == null)
                {
                    Debug.Log("Достигнут первый диалог после выбора. Возврат невозможен.");
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
                Debug.Log("Нет предыдущих диалогов для возврата.");
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
