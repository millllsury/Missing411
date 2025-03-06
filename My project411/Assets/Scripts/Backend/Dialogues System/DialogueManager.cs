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
    private EpisodeEndManager episodeEndManager;
    private int currentDialogueId = 1;
    private int textCounter = 0;

    private bool firstTimeSceneSound;

    public bool isChoosing = false;
    public bool isEpisodeScreenActive = false;
    public bool inputUnavailable = false;

    private bool EpisodeNameShowed = false; 


    private bool canGoBack = true;
    private Stack<(int? currentDialogueId, int textCounter)> dialogueHistory = new Stack<(int?, int)>();   


    [SerializeField] private GameObject choiceButtonNormalPrefab; 
    [SerializeField] private GameObject choiceButtonCostPrefab;  
    [SerializeField] private Transform choicesPanel; 

    

    private Dictionary<string, int> characterPositions = new Dictionary<string, int>(); 
    private int selectedSlotIndex;

    public bool blockMovingForward = false;



    [SerializeField] public GameObject speakerPanelLeft;
    [SerializeField] public GameObject speakerPanelCenter;
    [SerializeField] public GameObject speakerPanelRight;


    void Start()
    {

        InitializeComponents();
        LoadInitialData();


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
        int currentEpisodeId = int.Parse(GameStateManager.Instance.GetGameState().currentEpisode);

        visualNovelData = dataLoader.LoadData(currentEpisodeId);

        if (visualNovelData == null || visualNovelData.episodes == null)
        {
            Debug.LogError("Ошибка загрузки JSON! Проверь файл.");
            return;
        }

        Debug.Log($"[LoadInitialData] Загружен эпизод {currentEpisodeId} с {visualNovelData.episodes.Count} сценами.");

        foreach (var episode in visualNovelData.episodes)
        {
            Debug.Log($"[LoadInitialData] Episode {episode.episodeId} содержит {episode.scenes.Count} сцен.");
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

        
        if (isEpisodeScreenActive || isChoosing || backgroundController.IsTransitioning || inputUnavailable || blockMovingForward)
            return;


      
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextDialogueText();
        }


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

        if (visualNovelData.episodes.Count > 0)
        {
            currentEpisode = visualNovelData.episodes[0]; 
        }
        else
        {
            Debug.LogError($"Эпизод с ID {episodeId} не найден! Возможно, JSON загружен некорректно.");
            return;
        }

        ShowEpisodeName();
    }


    public void LoadScene(int sceneId)
    {
        firstTimeSceneSound = true;
        Debug.Log($"[LoadScene] Ищем сцену с ID {sceneId}. Доступные сцены: {string.Join(", ", currentEpisode.scenes.Select(s => s.sceneId))}");


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
        if (currentScene == null)
        {
            Debug.LogError("Ошибка: currentScene == null! Сцена не загружена.");
            return;
        }

        if (currentScene.dialogues == null || !currentScene.dialogues.Any())
        {
            Debug.LogError("❌ Ошибка: В сцене нет диалогов!");
            return;
        }

       
        currentDialogueId = startingDialogueId;
        textCounter = startingTextCounter;
        isChoosing = false;

        var dialogue = currentScene.dialogues.FirstOrDefault(d => d.id == currentDialogueId);

        if (dialogue == null)
        {
            Debug.LogError($"Ошибка: Диалог с ID {startingDialogueId} не найден!");
            return;
        }

        if (dialogue.texts == null)
        {
            Debug.LogWarning($"Внимание: Диалог {startingDialogueId} не содержит текстов! Возможно, это выбор (isChoosing).");

            if (dialogue.choices != null && dialogue.choices.Count > 0)
            {
                isChoosing = true;
                ShowChoices(dialogue.choices);
                return;
            }
            else
            {
                Debug.LogError($"Ошибка: Диалог {startingDialogueId} не содержит ни текстов, ни выборов!");
                return;
            }
        }

        if (textCounter >= dialogue.texts.Count)
        {
            Debug.Log($"TextCounter ({textCounter}) выходит за пределы текста диалога. Сбрасываем на 0.");
            textCounter--;
        }

        HideChoices();
        firstTimeSceneSound = true;
        HandleDialogueSound(dialogue);
        HandleDialogueAnimation(dialogue);
        DisplayDialogueText(dialogue);
    }


    public void ShowNextDialogueText()
    {

        var dialogue = GetCurrentDialogue();
        if (dialogue == null) return;
        if (returnedBack)
        {
            textCounter++;
            returnedBack = false;
        }
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

     
        if (!string.IsNullOrEmpty(dialogue.animation) && !dialogue.isAnimationPlayed)
        {
            animations.PlayAnimation(GetCharacterPosition(dialogue.character, dialogue.place), dialogue.animation, dialogue.character);

            Debug.Log($"HandleDialogueAnimation вызван с animationName: {dialogue.animation}");

           
            dialogue.isAnimationPlayed = true;
        }

    }

   
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
        string animationType = preset?.type ?? "background"; 

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
                
                dialogue.place = GameStateManager.GetCharacterPosition(dialogue.character);

                Debug.Log($"[GetCurrentDialogue] Character: {dialogue.character}, Assigned Place: {dialogue.place}");
            }
            else
            {
                Debug.LogWarning($"Dialogue {dialogue.id} has no character and is not narration.");
            }

            
            if (!string.IsNullOrEmpty(dialogue.speaker))
            {
                Debug.Log($"[GetCurrentDialogue] Speaker assigned: {dialogue.speaker}");
            }
            else if (!string.IsNullOrEmpty(dialogue.character))
            {
                dialogue.speaker = dialogue.character; 
            }
        }

        return dialogue;
    }




    private string GetCharacterPosition(string character, int place)
    {
        
        if (!string.IsNullOrEmpty(character) && characterPositions.TryGetValue(character, out int lastPlace))
        {
            return lastPlace == 1 ? "left" : "right";
        }

        
        return place == 1 ? "left" : "right";
    }


    bool goBackButtonActivated = false;

    private void DisplayDialogueText(Dialogue dialogue)
    {

       
    
        characterManager.SetCharacter(dialogue.speaker, dialogue.place, dialogue.isNarration, dialogue.character);
        UpdateSpeakerPanel(dialogue);


        if (dialogue.texts != null && dialogue.texts.Count > 0)
        {
            if (textCounter == 0)
            {
                dialogueText.text = dialogue.texts[textCounter];
                textCounter++;
            }
            else if (textCounter < dialogue.texts.Count)
            {
                
                if (goBackButtonActivated)
                {
                    dialogueText.text = dialogue.texts[textCounter++];
                }
                else
                {
                    
                    dialogueHistory.Push((currentDialogueId, textCounter - 1));
                    dialogueText.text = dialogue.texts[textCounter];
                    textCounter++;
                    UpdateBackButtonState(true);
                }
            }
            else
            {
                
                dialogueHistory.Push((currentDialogueId, textCounter - 1));

               
                if (!string.IsNullOrEmpty(dialogue.feedback))
                {
                    FeedbackManager.Instance.ShowMessage(dialogue.feedback);
                }

                if (dialogue.unlockNewItem)
                {
                    GameStateManager.Instance.UnlockNextItem();
                    FeedbackManager.Instance.ShowMessage("New outfit available in wardrobe.");
                }

                if (dialogue.reward > 0)
                {
                    CurrencyManager.Instance.AddKeys(dialogue.reward);
                    FeedbackManager.Instance.ShowMessage($"You've received {dialogue.reward} keys!");
                }

                if (dialogue.smoothDisappear)
                {
                    blinkingManager.StopBlinking(dialogue.character);
                    
                    characterManager.SmoothDisappearCharacter(dialogue.smoothDisappear, dialogue.place);
                }

                
                HandleDialogueEnd(dialogue);
            }
        }
        else if (dialogue.choices != null && dialogue.choices.Count > 0)
        {
            ShowChoices(dialogue.choices);
        }

        canGoBack = true; 
        goBackButtonActivated = false;
    }




    public void UpdateSpeakerPanel(Dialogue dialogue)
    {
        
        speakerPanelLeft.SetActive(false);
        speakerPanelCenter.SetActive(false);
        speakerPanelRight.SetActive(false);

        GameObject activePanel = null;
        string speakerText = "...";

        if (!string.IsNullOrEmpty(dialogue.character))
        {
           
            int characterPlace = GameStateManager.GetCharacterPosition(dialogue.character);

            if (characterPlace == 1) 
            {
                activePanel = speakerPanelLeft;
            }
            else if (characterPlace == 2) 
            {
                activePanel = speakerPanelRight;
            }

            if (!string.IsNullOrEmpty(dialogue.speaker))
            {
                speakerText = dialogue.speaker;
            }
            else
            {
                speakerText = dialogue.character; 
            }
            
        }
        else if (dialogue.isNarration) 
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

     

        if (TryLoadNextScene()) return;

        EndEpisode();
    }




    private void EndEpisode()
    {
        Debug.Log("эпизод завершён. Проверяем следующий эпизод.");
        

        int currentEpisodeId = int.Parse(GameStateManager.Instance.GetGameState().currentEpisode);
        int nextEpisodeId = currentEpisodeId + 1;

        Debug.Log($"попытка загрузить эпизод {nextEpisodeId}...");

        visualNovelData = dataLoader.LoadData(nextEpisodeId);

        if (visualNovelData == null || visualNovelData.episodes == null || visualNovelData.episodes.Count == 0)
        {
            Debug.Log($"oшибка: эпизод {nextEpisodeId} не найден или пуст!");
            FindFirstObjectByType<EpisodeEndManager>().EndEpisode();
            return;
        }

        currentEpisode = visualNovelData.episodes.Find(e => e.episodeId == nextEpisodeId);
        if (currentEpisode == null)
        {
            Debug.Log($"эпизод {nextEpisodeId} найден в JSON, но не загружен в память!");
            FindFirstObjectByType<EpisodeEndManager>().EndEpisode();
            return;
        }

        Debug.Log($"эпизод {nextEpisodeId} загружен, сцен: {currentEpisode.scenes.Count}");

        if (currentEpisode.scenes.Count == 0)
        {
            Debug.Log($"ошибка: В эпизоде {nextEpisodeId} нет сцен!");
            FindFirstObjectByType<EpisodeEndManager>().EndEpisode();
            return;
        }


       /* string firstSceneId = currentEpisode.scenes[0].sceneId.ToString();
        GameStateManager.Instance.UpdateSceneState(nextEpisodeId.ToString(), firstSceneId, "1", 0, false);

        Debug.Log($"загружаем эпизод {nextEpisodeId}, первая сцена: {firstSceneId}");

        LoadEpisode(nextEpisodeId);
        LoadProgress();*/
    }



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

    private bool TryLoadNextScene()
    {
        Debug.Log($"[TryLoadNextScene] Текущая сцена: {currentScene.sceneId}, сцен в эпизоде: {currentEpisode.scenes.Count}");

        int currentSceneIndex = currentEpisode.scenes.IndexOf(currentScene);

        Debug.Log($"[TryLoadNextScene] Индекс текущей сцены: {currentSceneIndex}");

        if (currentSceneIndex == -1)
        {
            Debug.LogError($"[TryLoadNextScene] Ошибка! Текущая сцена {currentScene.sceneId} не найдена в списке сцен эпизода!");
            return false;
        }

        if (currentSceneIndex >= currentEpisode.scenes.Count - 1)
        {
            Debug.Log("[TryLoadNextScene] Все сцены эпизода пройдены. Завершаем эпизод...");
            return false;
        }

        var nextScene = currentEpisode.scenes[currentSceneIndex + 1];

        Debug.Log($"[TryLoadNextScene] Загружаем следующую сцену: {nextScene.sceneId}");

        SaveGameStateForSceneChange(nextScene);

        characterManager.HideAvatars();

        SceneManager.LoadScene("Scene" + nextScene.sceneId);

        return true;
    }


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


        GameStateManager.Instance.SaveBackground(GameStateManager.Instance.LoadBackground()); 
        GameStateManager.Instance.SavePlayingTracks();//////
        GameStateManager.Instance.ClearBackgroundAnimation();
        GameStateManager.Instance.ClearForegroundAnimation();
        GameStateManager.Instance.SaveGameToSlot(selectedSlotIndex);

        Debug.Log($"Сохранено состояние для перехода. Scene={nextScene.sceneId}, Dialogue=1, TextCounter=0");
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

    public bool returnedBack;

    public void GoBackOneStep(GameObject clickedObject)
    {

        if (!canGoBack) return;

        if (clickedObject == backButton.gameObject)
        {
            if (backgroundController.IsTransitioning || inputUnavailable) return;

            if (isChoosing)
            {
                HideChoices();

            }

            if (dialogueHistory.Count == 0)
            {
                Debug.Log("История пуста. Возврат невозможен.");
                UpdateBackButtonState(false);
                return;
            }

        
            var previousState = dialogueHistory.Pop();

         
            if (previousState.currentDialogueId == null && previousState.textCounter == 0)
            {
                Debug.Log("Достигнут первый диалог после выбора. Отключаем кнопку возврата.");
                UpdateBackButtonState(false);
                return;
            }

            returnedBack = true;
            goBackButtonActivated = true;

           
            currentDialogueId = previousState.currentDialogueId.Value;
            textCounter = previousState.textCounter;

            Debug.Log($"Восстановлено состояние: previousDialogueId={currentDialogueId}, textCounter={textCounter}");


            UpdateDialogueText();
            UpdateBackButtonState(dialogueHistory.Count > 0 || textCounter > 0);
        }
    }

    private void UpdateDialogueText()
    {
        var dialogue = GetCurrentDialogue();
        if (dialogue != null)
        {
            dialogueText.text = dialogue.texts[textCounter];
            Debug.Log($"Восстановлено: Dialogue: {currentDialogueId} and textCounter: {textCounter}");
            UpdateSpeakerPanel(dialogue);
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

        // сleaning previous buttons
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

                    button.gameObject.SetActive(true); // mnaking the button active
                    sceneButtons.Add(button);
                }
                else
                {
                    Debug.LogError($"Button with ID {choice.buttonID} was not found in the scene.");
                    continue;
                }
            }

            // ff button was not found or the selection does not have a buttonID, create new one
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
        // looking for ALL buttons in the scene, even if inactive
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
            Destroy(child.gameObject); 
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

        if (choice.unlockNewItem)
        {
            GameStateManager.Instance.UnlockNextItem();
            FeedbackManager.Instance.ShowMessage("New outfit available in wardrobe.");
        }

        dialogueHistory.Clear();

        Debug.Log("Stack contents after selection: " + string.Join(" -> ", dialogueHistory.Select(d => $"[ID={d.currentDialogueId}, TextCounter={d.textCounter}]")));



        HideChoices();
        AdvanceToNextDialogue();
        uiManager.OnGameStateChanged();
    }

    public void ClearDialogueHistory()
    {
        dialogueHistory.Clear();
        Debug.Log("Диалоговая история очищена.");
    }

    public void SaveProgress()
    {
        if (selectedSlotIndex == -1)
        {
            Debug.LogError("No save slot selected. Saving is not possible.");
            return;
        }

        int safeTextCounter = textCounter < 0 ? 0 : textCounter - 1;

        int episodeId = currentEpisode.episodeId;
        int sceneId = currentScene.sceneId;
        int dialogueId = currentDialogueId;
        int textCounterToSave = safeTextCounter;
        string backgroundToSave = GameStateManager.Instance.LoadBackground();


        if (blockMovingForward)
        {
            episodeId = 1;
            sceneId = 2;
            dialogueId = 90;
            textCounterToSave = 0;
            backgroundToSave = "ChoosingDoorsAll";
            GameStateManager.Instance.SaveBackground(backgroundToSave);
            Debug.Log("Сохранение принудительного прогресса: 1-й эпизод, 2-я сцена, 88-й диалог, 0-й текст.");
        }

        GameStateManager.Instance.SaveCurrentState(
            episodeId,
            sceneId,
            dialogueId,
            textCounterToSave,
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


        var dialogueHistoryList = dialogueHistory.Reverse()
            .Select(item => new DialogueState(item.currentDialogueId ?? -1, item.textCounter))
            .ToList();

        Debug.Log("Saved dialog path: " +
            string.Join(" -> ", dialogueHistoryList.Select(d => $"(ID={d.dialogueId}, TextCounter={d.textCounter})")));

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

        loadedState.textCounter = loadedState.textCounter < 0 ? 0 : loadedState.textCounter; ////////////////////////////////////////////
        InitializeDialogue(int.Parse(loadedState.currentDialogue), loadedState.textCounter);
        Debug.Log($"Диалог: {loadedState.currentDialogue}");

        HideCollectedKeys();


        RestoreGameState(loadedState);

    }

    private void HideCollectedKeys()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager.Instance is null! Cannot check collected keys.");
            return;
        }

        int selectedSlot = GameStateManager.Instance.GetSelectedSlotIndex();
        if (selectedSlot == -1)
        {
            Debug.LogError("No slot selected! Keys cannot be checked.");
            return;
        }

        HashSet<string> collectedKeys = GameStateManager.Instance.LoadCollectedKeys();

        Debug.Log($"[HideCollectedKeys] Загруженные ключи для слота {selectedSlot}: {(collectedKeys.Count > 0 ? string.Join(", ", collectedKeys) : "ПУСТО")}");


        foreach (GameObject keyObject in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {

            if (keyObject.name.StartsWith("GoldenKey")) 
            {
                Debug.Log($"[HideCollectedKeys] Проверяем {keyObject.name}...");

                if (collectedKeys.Contains(keyObject.name)) 
                {
                    keyObject.SetActive(false);
                    Debug.Log($"[HideCollectedKeys] {keyObject.name} уже собран, скрываем объект.");
                }
            }
        }
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

        characterManager.LoadCharacters();


        GameStateManager.Instance.LoadPlayingTracks();

    }




    public void GotKeyReward(Button button)
    {

        string keyID = button.gameObject.name;

        // Checking if the key has already been got
        if (GameStateManager.Instance.IsKeyCollected(keyID))
        {
            Debug.Log($"[GotKeyReward] Ключ {keyID} уже собран, пропускаем.");
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
            return;
        }

        // Add the key to the saved ones
        GameStateManager.Instance.SaveKeyCollected(keyID);

        // We are giving out a reward
        CurrencyManager.Instance.AddKeys(1);
        FeedbackManager.Instance.ShowMessage("You've found a key!");

        // Torn off the btn
        if (button != null)
        {
            SoundManager.Instance.PlaySoundByName("key");
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