using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class DialogueManager : MonoBehaviour
{


    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject dialogueTextPanel;

    [SerializeField] private Button backButton;

    public UIManager UIManager;
    [SerializeField] private Animations animations;

    private DataLoader dataLoader;
    private BackgroundController backgroundController;
    private CharacterManager characterManager;
    private GameFlagsManager flagsManager;
    [SerializeField] private DoorBehaviour doorBehaviour;
    [SerializeField] private BlinkingManager blinkingManager;
    [SerializeField] private UIManager uiManager;
    private VisualNovelData visualNovelData;
    private Episode currentEpisode;
    private SceneData currentScene;

    private int currentDialogueId = 1;
    private int textCounter = 0;

    private bool firstTimeSceneSound;

    public bool isChoosing = false;
    public bool isEpisodeScreenActive = false;
    public bool inputUnavailable = false;

    private bool EpisodeNameShowed = false; // По умолчанию не показана


    private bool canGoBack = true; // Разрешение на возврат
    private Stack<(int? currentDialogueId, int textCounter)> dialogueHistory = new Stack<(int?, int)>();    // Стек для отслеживания диалогов


    [SerializeField] private GameObject choiceButtonNormalPrefab; // Префаб обычной кнопки
    [SerializeField] private GameObject choiceButtonCostPrefab;   // Префаб кнопки с ценой
    [SerializeField] private Transform choicesPanel; // Панель, куда добавляются кнопки


    private Dictionary<string, int> characterPositions = new Dictionary<string, int>(); // для позиций персонажей
    private int selectedSlotIndex;

    public bool blockMovingForward = false;



    [SerializeField] public GameObject speakerPanelLeft;
    [SerializeField] public GameObject speakerPanelCenter;
    [SerializeField] public GameObject speakerPanelRight;


    void Start()
    {

        InitializeComponents();
        LoadInitialData();

        HashSet<string> collectedKeys = GameStateManager.Instance.LoadCollectedKeys(selectedSlotIndex);

        foreach (Button keyButton in FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            string keyID = keyButton.gameObject.name; // Используем имя объекта как уникальный ID ключа

            if (collectedKeys.Contains(keyID))
            {
                keyButton.gameObject.SetActive(false);
                Debug.Log($"[Start] Ключ {keyID} уже собран в слоте {selectedSlotIndex}, скрываем объект.");
            }
        }

        if (GameStateManager.Instance != null)
        {
            selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();
        }
        else
        {
            Debug.LogError("GameStateManager.Instance is null!");
        }
        
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

    private void LoadInitialData()
    {
        visualNovelData = dataLoader.LoadData("Episode 1");
        if (visualNovelData == null || visualNovelData.episodes == null)
        {
            Debug.LogError("Ошибка загрузки JSON! Убедитесь, что файл правильный.");
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

        LoadProgress();
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

        // Блокируем любые действия при активном экране эпизода, выборе или недоступности ввода
        if (isEpisodeScreenActive || isChoosing || backgroundController.IsTransitioning || inputUnavailable || blockMovingForward)
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
        ShowEpisodeName();
    }

    public void LoadScene(int sceneId)
    {
        firstTimeSceneSound = true;
        currentScene = currentEpisode.scenes.Find(scene => scene.sceneId == sceneId);
        if (currentScene == null)
        {
            Debug.LogError($"Сцена с ID {sceneId} не найдена!");
            return;
        }
        characterManager.AdjustCharacterAppearance($"{SceneManager.GetActiveScene().name}");

    }


    private void ShowEpisodeName()
    {
        EpisodeNameShowed = GameStateManager.Instance.GetGameState().episodeNameShowed;

        if (EpisodeNameShowed) return;

        if (isEpisodeScreenActive) return;

        Sprite backgroundImage = Resources.Load<Sprite>(currentEpisode.backgroundImage);
        isEpisodeScreenActive = true;
        UIManager.ShowEpisodeScreen(currentEpisode.episodeName, backgroundImage);

        EpisodeNameShowed = true;
    }


    public void SetEpisodeScreenActive(bool isActive)
    {
        isEpisodeScreenActive = isActive;
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

        var dialogue = currentScene.dialogues.FirstOrDefault(d => d.id == currentDialogueId);
        if (dialogue != null)
        {
            if (textCounter >= dialogue.texts.Count)
            {
                Debug.LogWarning($"TextCounter ({textCounter}) выходит за пределы текста диалога. Сбрасываем на 0.");
                textCounter = 0;
            }
            HideChoices();
            firstTimeSceneSound = true;
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
        uiManager.OnGameStateChanged();
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

        if (!string.IsNullOrEmpty(dialogue.background))
        {

            backgroundController.SetBackgroundSmooth(dialogue.background, dialogue.smoothBgReplacement);
        }

        // Проверка, была ли уже проиграна анимация
        if (!string.IsNullOrEmpty(dialogue.animation) && !dialogue.isAnimationPlayed)
        {
            animations.PlayAnimation(GetCharacterPosition(dialogue.character, dialogue.place), dialogue.animation, dialogue.character);

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
        string soundName = preset?.soundName ?? null;
        string animationType = preset?.type ?? "background"; //  Проблема может быть здесь!

        Debug.Log($" animationType из пресета: {animationType}");

        backgroundController.StartBackgroundAnimation(dialogue.backgroundAnimation, frameDelay, repeatCount, keepLastFrame, soundName, animationType);
    }


    private Dialogue GetCurrentDialogue()
    {
        var dialogue = currentScene.dialogues.FirstOrDefault(d => d.id == currentDialogueId);

        if (dialogue != null)
        {
            if (dialogue.isNarration)
            {
                dialogue.isNarration = true;
            }
            else if (!string.IsNullOrEmpty(dialogue.character))
            {
                // Получаем позицию персонажа из GameStateManager
                dialogue.place = GameStateManager.GetCharacterPosition(dialogue.character);

                Debug.Log($"[GetCurrentDialogue] Character: {dialogue.character}, Assigned Place: {dialogue.place}");
            }
            else
            {
                Debug.LogWarning($"Dialogue {dialogue.id} has no character and is not narration.");
            }

            // Гарантируем, что speaker устанавливается всегда
            if (string.IsNullOrEmpty(dialogue.speaker) && !string.IsNullOrEmpty(dialogue.character))
            {
                dialogue.speaker = dialogue.character;
            }
        }

        return dialogue;
    }



    private string GetCharacterPosition(string character, int place)
    {
        // Если персонаж уже был, берем его последнюю позицию из characterPositions
        if (!string.IsNullOrEmpty(character) && characterPositions.TryGetValue(character, out int lastPlace))
        {
            return lastPlace == 1 ? "left" : "right";
        }

        // Если персонажа нет в словаре, используем переданное значение place
        return place == 1 ? "left" : "right";
    }


    bool goBackButtonActivated = false;

    private void DisplayDialogueText(Dialogue dialogue)
    {
        characterManager.SetCharacter(dialogue.speaker, dialogue.place, dialogue.isNarration, dialogue.character);
        UpdateSpeakerPanel(dialogue);

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
                    // Debug.Log($"ПУТЬ 1.2 textCounter: {textCounter}");
                    dialogueHistory.Push((currentDialogueId, textCounter - 1));
                    //Debug.Log($"Сохранилось в стек: previousDialogueId={currentDialogueId}, textCounter={textCounter - 1}");
                    dialogueText.text = dialogue.texts[textCounter];
                    textCounter++;
                    UpdateBackButtonState(true);
                    // Debug.Log($"ПУТЬ 1.2 textCounter: {textCounter}");
                }
            }
            else
            {
                //Debug.Log($"ПУТЬ2 - dialogue.texts.Count: {dialogue.texts.Count} and textCounter: {textCounter}");
                // Сохраняем состояние перед переходом к следующему диалогу
                dialogueHistory.Push((currentDialogueId, textCounter - 1));
                //Debug.Log($"Сохранилось в стек: previousDialogueId={currentDialogueId}, textCounter={textCounter - 1}");
                // Показываем Feedback перед завершением диалога
                if (!string.IsNullOrEmpty(dialogue.feedback))
                {
                    FeedbackManager.Instance.ShowMessage(dialogue.feedback);
                }

                if (dialogue.unlockNewItem)
                {
                    GameStateManager.Instance.UnlockNextItem();

                    FeedbackManager.Instance.ShowMessage("Ты открыл новый наряд и прическу!");

                }

                if (dialogue.reward > 0)
                {
                    CurrencyManager.Instance.AddKeys(dialogue.reward);
                    FeedbackManager.Instance.ShowMessage($"You've received {dialogue.reward} keys!");
                }


                if (dialogue.smoothDisappear)
                {

                    if (blinkingManager != null && !string.IsNullOrEmpty(dialogue.character))
                    {
                        blinkingManager.StopBlinking(dialogue.character);
                    }
                    characterManager.SmoothDisappearCharacter(dialogue.smoothDisappear, dialogue.place);
                }


                if (dialogue.miniGame)
                {

                }

                HandleDialogueEnd(dialogue);
            }
        }
        else if (dialogue.choices != null && dialogue.choices.Count > 0)
        {
            ShowChoices(dialogue.choices);
        }

        canGoBack = true; // Разрешаем возврат после обработки текста
        goBackButtonActivated = false;
    }

    public void UpdateSpeakerPanel(Dialogue dialogue)
    {
        // Деактивируем все панели перед установкой
        speakerPanelLeft.SetActive(false);
        speakerPanelCenter.SetActive(false);
        speakerPanelRight.SetActive(false);

        GameObject activePanel = null;
        string speakerText = "...";

        if (!string.IsNullOrEmpty(dialogue.character)) // Если есть персонаж
        {
            // Получаем позицию персонажа через GameStateManager
            int characterPlace = GameStateManager.GetCharacterPosition(dialogue.character);

            if (characterPlace == 1) // Если персонаж слева
            {
                activePanel = speakerPanelLeft;
            }
            else if (characterPlace == 2) // Если персонаж справа
            {
                activePanel = speakerPanelRight;
            }
            speakerText = dialogue.character; // Имя персонажа
        }
        else if (dialogue.isNarration) // Если это нарративный текст
        {
            activePanel = speakerPanelCenter;
        }

        if (activePanel != null)
        {
            activePanel.SetActive(true);
            TextMeshProUGUI speakerTextComponent = activePanel.transform.Find("SpeakerText").GetComponent<TextMeshProUGUI>();
            speakerTextComponent.text = speakerText;
        }
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
        GameStateManager.Instance.SetHasTransited(false);
        var nextDialogue = FindNextDialogue();
        if (nextDialogue != null)
        {
            StartNextDialogue(nextDialogue);
            return;
        }

        Debug.LogWarning("Не найден следующий диалог. Переход к следующей сцене.");
        if (TryLoadNextScene()) return;

        Debug.LogWarning("Это последняя сцена эпизода. Завершение эпизода.");
        EndEpisode();
    }

    // Старт нового диалога
    private void StartNextDialogue(Dialogue nextDialogue)
    {
        textCounter = 0;
        currentDialogueId = nextDialogue.id;
        nextDialogue.isAnimationPlayed = false;

        HandleDialogueAnimation(nextDialogue);

        if (!firstTimeSceneSound)
        {
            HandleDialogueSound(nextDialogue);
        }

        UpdateBackButtonState(true);
        ShowNextDialogueText();
        HadleDialogueBlockForward(nextDialogue);

    }

    // Переход к следующей сцене, если есть
    private bool TryLoadNextScene()
    {
        int currentSceneIndex = currentEpisode.scenes.IndexOf(currentScene);
        if (currentSceneIndex < 0 || currentSceneIndex >= currentEpisode.scenes.Count - 1)
            return false;

        var nextScene = currentEpisode.scenes[currentSceneIndex + 1];

        SaveGameStateForSceneChange(nextScene);

        characterManager.HideAvatars();
        SceneManager.LoadScene("Scene" + nextScene.sceneId);
        LoadScene(nextScene.sceneId);

        return true;
    }



    // Сохранение прогресса перед переходом к следующей сцене
    private void SaveGameStateForSceneChange(SceneData nextScene)
    {
        GameStateManager.Instance.UpdateFlags(flagsManager.GetAllFlags());
        var (currentHairIndex, currentClothesIndex) = GameStateManager.Instance.LoadAppearance();

        GameStateManager.Instance.SaveAppearance(currentHairIndex, currentClothesIndex);
        GameStateManager.Instance.SaveCharacterNames(null, null);
        GameStateManager.Instance.UpdateSceneState(
            currentEpisode.episodeId.ToString(),
            nextScene.sceneId.ToString(),
            "1", 0, true
        );

        dialogueHistory.Clear();
        GameStateManager.Instance.GetGameState().dialogueHistory = new List<DialogueState>();
        GameStateManager.Instance.GetGameState().currentBackgroundAnimation = null;
        GameStateManager.Instance.GetGameState().currentForegroundAnimation = null;


        GameStateManager.Instance.SaveBackground(GameStateManager.Instance.LoadBackground()); //  Вместо обнуления сохраняем последний фон
        GameStateManager.Instance.SavePlayingTracks();//////
        GameStateManager.Instance.ClearBackgroundAnimation();
        GameStateManager.Instance.ClearForegroundAnimation();
        GameStateManager.Instance.SaveGameToSlot(selectedSlotIndex);

        Debug.Log($"Сохранено состояние для перехода. Scene={nextScene.sceneId}, Dialogue=1, TextCounter=0");
    }

    private void EndEpisode()
    {
        Debug.Log("Эпизод завершён. Переход к экрану завершения эпизода или следующему эпизоду.");

        // Здесь можно показать экран завершения эпизода или выполнить переход
        // Например, перейти в главное меню:
        SceneManager.LoadScene("MainMenu"); // Убедитесь, что сцена "MainMenu" добавлена в Build Settings
    }


    private void HandleDialogueSound(Dialogue dialogue)
    {
        if (firstTimeSceneSound == true)
        {
            firstTimeSceneSound = false;
        }

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

    private void HadleDialogueBlockForward(Dialogue dialogue)
    {
        if (dialogue.blockMovingForward == true)
        {
            blockMovingForward = true;
            doorBehaviour.HideObjects();
        }


    }


    public void GoBackOneStep(GameObject clickedObject)
    {
        if (!canGoBack) return; // Если возврат невозможен, ничего не делаем.

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
                Debug.Log("История пуста. Возврат невозможен.");
                UpdateBackButtonState(false);
                return;
            }

            // Извлекаем предыдущее состояние из стека
            var previousState = dialogueHistory.Pop();

            // Проверяем, был ли предыдущий диалог первым после выбора
            if (previousState.currentDialogueId == null && previousState.textCounter == 0)
            {
                Debug.Log("Достигнут первый диалог после выбора. Отключаем кнопку возврата.");
                UpdateBackButtonState(false);
                return;
            }

            // Восстанавливаем состояние диалога
            currentDialogueId = previousState.currentDialogueId.Value;
            textCounter = previousState.textCounter;

            Debug.Log($"Восстановлено состояние: previousDialogueId={currentDialogueId}, textCounter={textCounter}");

            // Синхронизируем текст и персонажей
            UpdateDialogueText();

            goBackButtonActivated = true;
            // Обновляем состояние кнопки
            UpdateBackButtonState(dialogueHistory.Count > 0);
        }
    }

    private void UpdateDialogueText()
    {
        var dialogue = GetCurrentDialogue();
        if (dialogue != null)
        {
            dialogueText.text = dialogue.texts[textCounter];
            Debug.Log($"Восстановлено: Dialogue: {currentDialogueId} and textCounter: {textCounter}");
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

    public void ShowChoices(List<Choice> choices)
    {
        dialogueTextPanel.SetActive(false);
        isChoosing = true;

        // Cleaning previous buttons
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject);
        }
        sceneButtons.Clear();

        foreach (Choice choice in choices)
        {
            Button button = null;

            if (!string.IsNullOrEmpty(choice.buttonID))
            {
                button = FindExistingButton(choice.buttonID);
                if (button != null)
                {

                    button.gameObject.SetActive(true); // Making the button active
                    sceneButtons.Add(button);
                }
                else
                {
                    Debug.LogError($"Button with ID {choice.buttonID} was not found in the scene.");
                    continue;
                }
            }

            // If the button was not found or the selection does not have a buttonID, create a new one
            if (button == null)
            {
                GameObject buttonPrefab = choice.cost > 0 ? choiceButtonCostPrefab : choiceButtonNormalPrefab;
                GameObject choiceButton = Instantiate(buttonPrefab, choicesPanel);
                button = choiceButton.GetComponent<Button>();
            }

            button.Configure(choice, OnChoiceSelected);
        }
    }



    private Button FindExistingButton(string buttonID)
    {
        //We are looking for ALL buttons in the scene, even if they are inactive
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();

        foreach (var button in allButtons)
        {
            if (button.gameObject.name == buttonID)
            {
                Debug.Log($"Found button {buttonID}");
                return button;
            }
        }

        Debug.LogError($"Button {buttonID} NOT found!");
        return null;
    }

    private List<Button> sceneButtons = new List<Button>();

    private void HideChoices()
    {
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject); // Removing all buttons from the selection panel
        }

        foreach (var button in sceneButtons)
        {
            button.gameObject.SetActive(false);
        }


        dialogueTextPanel.SetActive(true);
        isChoosing = false;
    }



    public void OnChoiceSelected(Choice choice)
    {


        if (choice.cost > 0)
        {
            if (!CurrencyManager.Instance.SpendKeys(choice.cost))
            {
                //Debug.Log("Not enough keys to select!");
                FeedbackManager.Instance.ShowMessage("There are not enough keys to select!");
                return;

            }

        }

        if (choice.reward > 0)
        {
            CurrencyManager.Instance.AddKeys(choice.reward);
            FeedbackManager.Instance.ShowMessage($"You've received {choice.reward} keys!");
        }

        if (choice.actions != null)
        {
            foreach (var action in choice.actions)
            {
                flagsManager.SetFlag(action.key, action.value);
            }
        }

        if (choice.feedback != null)
        {
            FeedbackManager.Instance.ShowMessage(choice.feedback);
        }


        // Clearing the dialogue history stack
        dialogueHistory.Clear();

        Debug.Log("Stack contents after selection: " + string.Join(" -> ", dialogueHistory.Select(d => $"[ID={d.currentDialogueId}, TextCounter={d.textCounter}]")));



        HideChoices();
        AdvanceToNextDialogue();
        uiManager.OnGameStateChanged();
    }


    public void SaveProgress()
    {
        if (selectedSlotIndex == -1)
        {
            Debug.LogError("No save slot selected. Saving is not possible.");
            return;
        }

        int safeTextCounter = textCounter < 0 ? 0 : textCounter - 1;

        GameStateManager.Instance.SaveCurrentState(
            currentEpisode.episodeId,
            currentScene.sceneId,
            currentDialogueId,
            safeTextCounter, // Используем исправленный textCounter
            EpisodeNameShowed,
            flagsManager.GetAllFlags(),
            characterManager.GetCurrentLeftCharacter(),
            characterManager.GetCurrentRightCharacter()
        );

        var (currentHairIndex, currentClothesIndex) = GameStateManager.Instance.LoadAppearance();
        GameStateManager.Instance.SaveAppearance(currentHairIndex, currentClothesIndex);

        if (SceneManager.GetActiveScene().name != "WardrobeScene" && SceneManager.GetActiveScene().name != "MainMenu")
        {
            GameStateManager.Instance.SavePlayingTracks();
        }

        // Сохраняем историю диалогов до выбора
        var dialogueHistoryList = dialogueHistory.Reverse()
       .Select(item => new DialogueState(item.currentDialogueId ?? -1, item.textCounter))
       .ToList();

        Debug.Log("Saved dialog path: " +
            string.Join(" -> ", dialogueHistoryList.Select(d => $"(ID={d.dialogueId}, TextCounter=	{d.textCounter})")));

        GameStateManager.Instance.GetGameState().dialogueHistory = dialogueHistoryList;

        GameStateManager.Instance.SaveGameToSlot(selectedSlotIndex);

        Debug.Log($"Game progress saved in slot {selectedSlotIndex + 1}.");
    }


    public void LoadProgress()
    {

        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager.Instance not initialized.");
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
                    : "Empty"));

        // clearing the stack 
        dialogueHistory.Clear();

        // recovering stack from history (without reverse)
        if (loadedState.dialogueHistory != null)
        {
            foreach (var item in loadedState.dialogueHistory)
            {
                dialogueHistory.Push((item.dialogueId == -1 ? null : (int?)item.dialogueId, item.textCounter));
                Debug.Log($"Stack element was added: ID={item.dialogueId}, TextCounter={item.textCounter}");
            }
        }

        
        // scene and dialogue
        EpisodeNameShowed = loadedState.episodeNameShowed;
        Debug.Log($"Scene {loadedState.currentScene} and dialogue {loadedState.currentDialogue} are loading.");
        LoadEpisode(int.Parse(loadedState.currentEpisode));
        LoadScene(int.Parse(loadedState.currentScene));
        InitializeDialogue(int.Parse(loadedState.currentDialogue), loadedState.textCounter);
        Debug.Log($"Диалог: {loadedState.currentDialogue}");


       
        RestoreGameState(loadedState);

    }

    private void RestoreGameState(GameState loadedState)
    {

        Debug.Log($"starts with GameStateManager.Instance.isNewGame: {GameStateManager.Instance.isNewGame}");
        // Restoring flags
        flagsManager.SetAllFlags(loadedState.flags);

        // BG
        var backgroundName = GameStateManager.Instance.LoadBackground();
        backgroundController.SetBackground(backgroundName);

        var (foregroundAnimation, foregroundFrameDelay, foregroundRepeatCount, foregroundKeepLastFrame) = GameStateManager.Instance.LoadForegroundAnimation();

        if (!string.IsNullOrEmpty(foregroundAnimation))
        {
            backgroundController.StartBackgroundAnimation(foregroundAnimation, foregroundFrameDelay, foregroundRepeatCount, foregroundKeepLastFrame, null, "foreground");
        }

        var (backgroundAnimation, frameDelay, repeatCount, keepLastFrame) = GameStateManager.Instance.LoadBackgroundAnimation();
        if (!string.IsNullOrEmpty(backgroundAnimation))
        {
            backgroundController.StartBackgroundAnimation(backgroundAnimation, frameDelay, repeatCount, keepLastFrame, null, "background");
        }
        //characterPositions = GameStateManager.Instance.LoadCharacterPositions(selectedSlotIndex);
        // characters
        characterManager.LoadCharacters();


        GameStateManager.Instance.LoadPlayingTracks();

    }

    public void GotKeyReward(Button button)
    {

        string keyID = button.gameObject.name;

        // Checking if the key has already been got
        if (GameStateManager.Instance.IsKeyCollected(selectedSlotIndex, keyID))
        {
            Debug.Log($"[GotKeyReward] Ключ {keyID} уже собран, пропускаем.");
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
            return;
        }

        // Add the key to the saved ones
        GameStateManager.Instance.SaveKeyCollected(selectedSlotIndex, keyID);

        // We are giving out a reward
        CurrencyManager.Instance.AddKeys(1);
        FeedbackManager.Instance.ShowMessage("You've found a key!");

        // Torn off the btn
        if (button != null)
        {
            button.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Error in GotKeyReward!");
        }
        uiManager.OnGameStateChanged();
    }



}



public static class ButtonExtensions
{
    public static void Configure(this Button button, Choice choice, UnityAction<Choice> onClick)
    {
        button.gameObject.SetActive(true);

        // Find UI elements inside the button
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        Transform costContainer = button.transform.Find("CostContainer");
        TextMeshProUGUI costText = costContainer?.Find("CostText")?.GetComponent<TextMeshProUGUI>();
        Image keyIcon = costContainer?.Find("KeyIcon")?.GetComponent<Image>();

        // Set the main text of the button
        if (buttonText != null)
            buttonText.text = choice.text;

        // If the choice is worth the keys, we show the cost and the iconу
        if (choice.cost > 0)
        {
            if (costText != null)
            {
                costText.text = choice.cost.ToString();
                costText.gameObject.SetActive(true);
            }
            if (keyIcon != null)
            {
                keyIcon.gameObject.SetActive(true);
            }
        }
        else
        {
            if (costText != null) costText.gameObject.SetActive(false);
            if (keyIcon != null) keyIcon.gameObject.SetActive(false);
        }

        // Remove old events and add a new click handler
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(choice));
    }
}