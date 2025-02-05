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

    private bool EpisodeNameShowed = false; // По умолчанию не показана


    private bool canGoBack = true; // Разрешение на возврат
    private Stack<(int? currentDialogueId, int textCounter)> dialogueHistory = new Stack<(int?, int)>();    // Стек для отслеживания диалогов

    void Start()
    {
        InitializeComponents();
        LoadInitialData();
        LoadProgress(); // Автоматически загружаем состояние, если выбран слот
        
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
            InitializeDialogue(currentDialogueId, textCounter);
            //ShowEpisodeName();
            HandleSceneBackground();
        }
        HideChoices();
    }


    private void ShowEpisodeName()
    {
        if (EpisodeNameShowed) return; // Если уже показывали, не показываем снова

        if (isEpisodeScreenActive) return; // Проверяем, активен ли уже экран

        Sprite backgroundImage = Resources.Load<Sprite>(currentEpisode.backgroundImage);
        isEpisodeScreenActive = true;
        UIManager.ShowEpisodeScreen(currentEpisode.episodeName, backgroundImage);

        EpisodeNameShowed = true; // Устанавливаем флаг, что экран был показан
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

        var dialogue = currentScene.dialogues.FirstOrDefault(d => d.id == currentDialogueId);
        if (dialogue != null)
        {
            if (textCounter >= dialogue.texts.Count)
            {
                Debug.LogWarning($"TextCounter ({textCounter}) выходит за пределы текста диалога. Сбрасываем на 0.");
                textCounter = 0;
            }

            // Обрабатываем звуки и анимации, если они указаны
            //HandleDialogueSound(dialogue);
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

            // Сбрасываем текстовый счётчик для нового диалога
            textCounter = 0;

            // Устанавливаем новый текущий диалог
            currentDialogueId = nextDialogue.id;

            // Сбрасываем флаг анимации для следующего диалога
            nextDialogue.isAnimationPlayed = false;

            // Обрабатываем анимацию перед показом текста
            HandleDialogueAnimation(nextDialogue);

            HandleDialogueSound(nextDialogue);
            UpdateBackButtonState(true);

            // Показываем текст следующего диалога
            ShowNextDialogueText();
        }
        else
        {
            Debug.LogWarning("Не найден следующий диалог. Переход к следующей сцене.");
            var currentSceneIndex = currentEpisode.scenes.IndexOf(currentScene);

            if (currentSceneIndex >= 0 && currentSceneIndex < currentEpisode.scenes.Count - 1)
            {
                // Берем следующую сцену
                var nextScene = currentEpisode.scenes[currentSceneIndex + 1];
                // Сохраняем прогресс для перехода
                GameStateManager.Instance.UpdateFlags(flagsManager.GetAllFlags());
                var (currentHairIndex, currentClothesIndex) = GameStateManager.Instance.LoadAppearance();
                GameStateManager.Instance.SaveAppearance(currentHairIndex, currentClothesIndex);
                GameStateManager.Instance.SaveCharacterNames(
                    null,
                    null
                    );
                GameStateManager.Instance.UpdateSceneState(
                    nextScene.sceneId.ToString(),  // Следующая сцена
                    "1",                          // Диалог начинается с 1
                    0                             // Сброс текстового счётчика
                );

                dialogueHistory.Clear(); // Очищаем стек
                var dialogueHistoryList = new List<DialogueState>(); // Создаём новый пустой список
                GameStateManager.Instance.GetGameState().dialogueHistory = dialogueHistoryList; // Присваиваем новый пустой список

                GameStateManager.Instance.GetGameState().currentBackgroundAnimation = null;
                GameStateManager.Instance.SaveBackground(null);
                int selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();
                GameStateManager.Instance.SaveGameToSlot(selectedSlotIndex);
                // Лог для отладки
                Debug.Log($"Сохранено состояние для перехода. Scene={nextScene.sceneId}, Dialogue=0, TextCounter=0");

                characterManager.HideAvatars();
                // Переход к следующей сцене
                SceneManager.LoadScene("Scene" + nextScene.sceneId);
                LoadScene(nextScene.sceneId);
            }
            else
            {
                Debug.LogWarning("Это последняя сцена эпизода. Завершение эпизода.");
                EndEpisode();
            }
        }
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

        // Очищаем стек истории
        dialogueHistory.Clear();

        Debug.Log("Содержимое стека после выбора: " + string.Join(" -> ", dialogueHistory.Select(d => $"[ID={d.currentDialogueId}, TextCounter={d.textCounter}]")));


        HideChoices();
        AdvanceToNextDialogue();
    }


    public void SaveProgress()
    {
        // Получаем текущий индекс выбранного слота
        int selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();

        if (selectedSlotIndex == -1)
        {
            Debug.LogError("Слот для сохранения не выбран. Сохранение невозможно.");
            return;
        }

        // Сохраняем состояние текущей сцены
        int currentTextCounter = textCounter - 1;
        GameStateManager.Instance.UpdateSceneState(
            currentScene.sceneId.ToString(),
            currentDialogueId.ToString(),
            currentTextCounter 

        );

        // Обновляем флаги
        GameStateManager.Instance.UpdateFlags(flagsManager.GetAllFlags());

        // Сохраняем внешний вид персонажей
        var (currentHairIndex, currentClothesIndex) = GameStateManager.Instance.LoadAppearance();
        GameStateManager.Instance.SaveAppearance(currentHairIndex, currentClothesIndex);

        // Сохраняем имена персонажей
        string leftCharacter = characterManager.GetCurrentLeftCharacter();
        string rightCharacter = characterManager.GetCurrentRightCharacter();
        GameStateManager.Instance.SaveCharacterNames(leftCharacter, rightCharacter);

        // Сохраняем фон и анимацию
        var animationName = backgroundController.GetCurrentAnimationName();
        var frameDelay = backgroundController.GetCurrentFrameDelay();
        var repeatCount = backgroundController.GetCurrentRepeatCount();
        var keepLastFrame = backgroundController.GetKeepLastFrame();
        var backgroundName = backgroundController.GetCurrentBackgroundName(); // Добавьте метод в BackgroundController
        GameStateManager.Instance.SaveBackground(backgroundName); // Сохраняем имя фона
        GameStateManager.Instance.SaveBackgroundAnimation(animationName, frameDelay, repeatCount, keepLastFrame);


        if (SceneManager.GetActiveScene().name != "WardrobeScene" && SceneManager.GetActiveScene().name != "MainMenu")
        {
            GameStateManager.Instance.SavePlayingTracks();
        }

        // Сохраняем историю диалогов до выбора
        var dialogueHistoryList = dialogueHistory.Reverse()
       .Select(item => new DialogueState(item.currentDialogueId ?? -1, item.textCounter))
       .ToList();

        Debug.Log("Сохраняемый путь диалога (лист): " +
            string.Join(" -> ", dialogueHistoryList.Select(d => $"(ID={d.dialogueId}, TextCounter={d.textCounter})")));

        GameStateManager.Instance.GetGameState().dialogueHistory = dialogueHistoryList;
        
        GameStateManager.Instance.SaveGameToSlot(selectedSlotIndex);
        Debug.Log($"Прогресс игры сохранен в слот {selectedSlotIndex + 1}.");


    }



    public void LoadProgress()
    {

        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager.Instance не инициализирован.");
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
                    : "Пусто"));

        // Очищаем стек перед восстановлением
        dialogueHistory.Clear();

        // Восстанавливаем стек из истории (без Reverse)
        if (loadedState.dialogueHistory != null)
        {
            foreach (var item in loadedState.dialogueHistory)
            {
                dialogueHistory.Push((item.dialogueId == -1 ? null : (int?)item.dialogueId, item.textCounter));
                Debug.Log($"Stack element was added: ID={item.dialogueId}, TextCounter={item.textCounter}");
            }
        }

        // Логируем итоговое содержимое стека
        Debug.Log("Итоговое содержимое стека после восстановления: " +
            string.Join(" -> ", dialogueHistory.Select(d => $"[ID={d.currentDialogueId}, TextCounter={d.textCounter}]")));

        // Восстанавливаем сцену и диалог
        Debug.Log($"Scene {loadedState.currentScene} and dialogue {loadedState.currentDialogue} are loading.");
        LoadScene(int.Parse(loadedState.currentScene));
        InitializeDialogue(int.Parse(loadedState.currentDialogue), loadedState.textCounter);

        // Восстановление других данных, таких как фон, персонажи и флаги
        RestoreGameState(loadedState);

        

    }

    private void RestoreGameState(GameState loadedState)
    {
        // Восстановление флагов
        flagsManager.SetAllFlags(loadedState.flags);

        // Восстановление фона
        var backgroundName = GameStateManager.Instance.LoadBackground();
        if (!string.IsNullOrEmpty(backgroundName))
        {
            backgroundController.SetBackground(backgroundName);
        }

        // Восстановление анимации
        var (animationName, frameDelay, repeatCount, keepLastFrame) = GameStateManager.Instance.LoadBackgroundAnimation();
        if (!string.IsNullOrEmpty(animationName))
        {
            backgroundController.StartBackgroundAnimation(animationName, frameDelay, repeatCount, keepLastFrame, null);
        }

        // Восстановление персонажей
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
