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
    private int textCounter;

    public bool isChoosing = false;
    public bool isEpisodeScreenActive = false;
    public bool inputUnavailable = false;

    private bool episodeNameShowed = false; // По умолчанию не показана



    private Stack<(int? currentDialogueId, int textCounter)> dialogueHistory = new Stack<(int?, int)>();    // Стек для отслеживания диалогов


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
            Debug.LogError("Не удалось найти UIManager.");
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
        if (isEpisodeScreenActive || isChoosing || backgroundController.IsTransitioning || inputUnavailable)
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

        // Если текущий диалог уже установлен, используем его
        if (currentDialogueId > 0)
        {
            InitializeDialogue(currentDialogueId, textCounter);
        }
        else
        {
            // Если текущий диалог не задан, начинаем с первого
            currentDialogueId = currentScene.dialogues[0].id;
            textCounter = 0;
            //characterManager.HideAvatars();
            InitializeDialogue(currentDialogueId, textCounter);
            ShowEpisodeName();
            HandleSceneBackground();
        }
    }


    private void ShowEpisodeName()
    {
        if (episodeNameShowed) return; // Если уже показывали, не показываем снова

        if (isEpisodeScreenActive) return; // Проверяем, активен ли уже экран

        Sprite backgroundImage = Resources.Load<Sprite>(currentEpisode.backgroundImage);
        isEpisodeScreenActive = true;
        UIManager.ShowEpisodeScreen(currentEpisode.episodeName, backgroundImage);

        episodeNameShowed = true; // Устанавливаем флаг, что экран был показан
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
            Debug.LogError("В сцене нет диалогов.");
            return;
        }

        // Устанавливаем текущий диалог и счетчик текста
        currentDialogueId = startingDialogueId;
        textCounter = startingTextCounter;
        isChoosing = false;

        var dialogue = GetCurrentDialogue();
        if (dialogue != null)
        {
            if (textCounter >= dialogue.texts.Count)
            {
                Debug.LogWarning($"TextCounter ({textCounter}) выходит за пределы текста диалога. Сбрасываем на 0.");
                textCounter = 0;
            }

            // Обрабатываем звуки и анимации, если они указаны
            HandleDialogueSound(dialogue);
            HandleDialogueAnimation(dialogue);
            DisplayDialogueText(dialogue); // Отображаем текущий текст
        }
        else
        {
            Debug.LogWarning($"Диалог с ID {startingDialogueId} не найден.");
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

        // Проверка, была ли уже проиграна анимация
        if (!string.IsNullOrEmpty(dialogue.animation) && !dialogue.isAnimationPlayed)
        {
            animations.PlayAnimation(GetCharacterPosition(dialogue.place), dialogue.animation, dialogue.character);

            Debug.Log($"HandleDialogueAnimation вызван с animationName: {dialogue.animation}");

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

            // Сбрасываем текстовый счётчик для нового диалога
            textCounter = 0;

            // Устанавливаем новый текущий диалог
            currentDialogueId = nextDialogue.id;

            // Сбрасываем флаг анимации для следующего диалога
            nextDialogue.isAnimationPlayed = false;

            // Обрабатываем анимацию перед показом текста
            HandleDialogueAnimation(nextDialogue);

            HandleDialogueSound(nextDialogue);
            // Показываем текст следующего диалога
            ShowNextDialogueText();
        }
        else
        {

            Debug.LogWarning("Не найден следующий диалог. Конец сцены?");
        }
    }

    private void HandleDialogueSound(Dialogue dialogue)
    {
        if (!string.IsNullOrEmpty(dialogue.soundTrigger))
        {
            soundManager.HandleSoundTrigger(dialogue.soundTrigger);
        }
    }

      private bool canGoBack = true; // Разрешение на возврат

    public void GoBackOneStep(GameObject clickedObject)
    {
        if (!canGoBack) return;

        if (clickedObject == backButton.gameObject)
        {
            if (backgroundController.IsTransitioning || inputUnavailable) return;

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


    public void SaveProgress()
    {
        int currentTextCounter = textCounter - 1;
        GameStateManager.Instance.UpdateSceneState(
            currentEpisode.episodeId.ToString(),
            currentScene.sceneId.ToString(),
            currentDialogueId.ToString(),
            currentTextCounter,
            episodeNameShowed
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
        Debug.Log("Прогресс игры сохранен через GameStateManager.");
    }



    public void LoadProgress()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager.Instance не инициализирован! Убедитесь, что объект GameStateManager присутствует на сцене.");
            return;
        }

        if (GameStateManager.Instance.LoadGame())
        {
            var loadedState = GameStateManager.Instance.GetGameState();

            if (loadedState == null)
            {
                Debug.LogError("Загруженный GameState равен null.");
                return;
            }

            // Проверка и восстановление флагов
            if (loadedState.flags == null)
            {
                Debug.LogWarning("Флаги отсутствуют. Создаем пустой словарь.");
                loadedState.flags = new Dictionary<string, bool>();
            }

            flagsManager.SetAllFlags(loadedState.flags);

            // Восстановление сцены и диалога
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
