using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject loadingScreen;         
    public Image progressFill;               
    public RectTransform thumb;              
    public float loadTime = 1.5f;
    private float progress = 0f;             // Прогресс загрузки (0-1)
    private float maxWidth;                  // Ширина прогресс-бара

    public CanvasGroup menuCanvasGroup;      // CanvasGroup для плавного появления
    public CanvasGroup slotsCanvasGroup;
    public float fadeInDuration = 1.5f;

    [SerializeField] private GameObject saveSlotsCanvas; // Ссылка на Canvas со слотами
    [SerializeField] private Transform slotsParent;      // Родительский объект для кнопок слотов
    [SerializeField] private GameObject slotButtonPrefab; // Префаб кнопки

    [SerializeField] private GameObject settingsCanvas; // Ссылка на Canvas со слотами

    private FeedbackManager feedbackManager;

    //public bool isNewGame = false; // Флаг, указывающий, была ли начата новая игра
    

    private void Start()
    {
        feedbackManager = FindFirstObjectByType<FeedbackManager>();
        loadingScreen.SetActive(false);
        maxWidth = ((RectTransform)progressFill.transform.parent).rect.width;
        loadingScreen.SetActive(false);      // Скрываем экран загрузки в начале

        // Начинаем с полностью прозрачного меню
        menuCanvasGroup.alpha = 0f;
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;

        // Запускаем эффект плавного появления
        StartCoroutine(FadeInMenu());

        //gameStateManager = FindFirstObjectByType<GameStateManager>();
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager не найден в сцене. Убедитесь, что объект присутствует.");
            return;
        }
        GameStateManager.Instance.LoadSaveSlots();
        PopulateSaveSlots();
        //soundManager = FindFirstObjectByType<SoundManager>();
        if (SoundManager.Instance == null)
        {
            Debug.LogError("SoundManager не найден. Убедитесь, что объект присутствует на сцене.");
            return;
        }

        SoundManager.Instance.PlaySoundByName("MainMenuSound");
        SoundManager.Instance.PlaySoundByName("owl");
        SoundManager.Instance.UnmuteAllSounds();

    }

    private IEnumerator FadeInMenu()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            menuCanvasGroup.alpha = alpha;
            yield return null;
        }

        // После появления делаем меню интерактивным
        menuCanvasGroup.alpha = 1f;
        menuCanvasGroup.interactable = true;
        menuCanvasGroup.blocksRaycasts = true;
    }


    public void NewGame()
    {
 
        Debug.Log("New Game is started.");

        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager не инициализирован.");
            return;
        }

        // Проверяем, есть ли пустой слот
        var emptySlotIndex = GameStateManager.Instance.GetSaveSlots()
            .FindIndex(slot => slot.gameState == null);

        if (emptySlotIndex == -1)
        {
            Debug.LogWarning("Все слоты заняты. Перезаписываем Слот 1.");
            emptySlotIndex = 0; // Перезаписываем первый слот
            GameStateManager.Instance.rewritingGame = true;
        }
        else
        {
            GameStateManager.Instance.isNewGame = true;
            
        }

        // Инициализируем новый прогресс игры
        var saveSlots = GameStateManager.Instance.GetSaveSlots();
        var emptySlot = saveSlots[emptySlotIndex];
        emptySlot.gameState = new GameState
        {
            currentEpisode = "1",
            currentScene = "1",
            currentDialogue = "1",
            textCounter = 0,
            flags = new Dictionary<string, bool>(),
            hairIndex = 0,
            clothesIndex = 0,
            episodeNameShowed = false,
            keys = 5,
             // **Очищаем unlockedHairstyles и unlockedClothes**
            unlockedHairstyles = new List<int> { 0, 1 },
            unlockedClothes = new List<int> { 0, 1 }

        };
        emptySlot.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


        // Устанавливаем выбранный слот
        GameStateManager.Instance.SelectSlot(emptySlotIndex);

        // Сохраняем слоты в файл
        GameStateManager.Instance.SaveSlotsToFile();

        progress = 0f; // Сброс прогресса
        loadingScreen.SetActive(true); // Показать экран загрузки
        StartCoroutine(LoadSceneAsync("Scene1"));
        menuCanvasGroup.alpha = 0f;
    }


    private IEnumerator LoadSceneAsync(string sceneName)
    {
       
        //Debug.Log($"Попытка загрузить сцену: {sceneName}");
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        SoundManager.Instance.StopAllSounds();
        GameStateManager.Instance.RemoveAllPlayingTracks();


        if (operation == null)
        {
            Debug.LogError($"Сцена '{sceneName}' не существует или не добавлена в Build Settings!");
            yield break;
        }

        operation.allowSceneActivation = false;
        Debug.Log("Loading has started...");

        while (!operation.isDone)
        {
            //Debug.Log($"operation.progress: {operation.progress}");
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            progress = Mathf.MoveTowards(progress, targetProgress, Time.deltaTime / loadTime);
            UpdateProgress(progress);

            if (progress >= 0.99f) // Условие с небольшим допуском
            {
               // Debug.Log("Сцена готова к активации");
                operation.allowSceneActivation = true;
            }


            yield return null;
        }
        
        //Debug.Log("Сцена успешно загружена!");
    }



    private void UpdateProgress(float progress)
    {
        // Обновляем ширину заполнения прогресс-бара
        float newWidth = maxWidth * progress;
        progressFill.rectTransform.sizeDelta = new Vector2(newWidth, progressFill.rectTransform.sizeDelta.y);

        // Учитываем ширину бегунка и корректируем его позицию
        float thumbWidth = thumb.rect.width;
        thumb.anchoredPosition = new Vector2(newWidth - (thumbWidth / 2), thumb.anchoredPosition.y);
    }
    public void ContinueGame()
    {
        saveSlotsCanvas.SetActive(true);
        menuCanvasGroup.alpha = 0f;
    }

    public void CloseSaveSlots()
    {
        saveSlotsCanvas.SetActive(false);
        menuCanvasGroup.alpha = 1f;
    }

    public void OpenSettings()
    {
        settingsCanvas.SetActive(true);
        menuCanvasGroup.alpha = 0f;
    }

    public void CloseSettings()
    {
        settingsCanvas.SetActive(false);
        menuCanvasGroup.alpha = 1f;
    }

    public void QuitGame()
    {
        Debug.Log("Quit нажата");
        Application.Quit(); // Работает только в собранной версии игры, не в редакторе Unity
    }

    

    public void PopulateSaveSlots()
    {
        // Очищаем старые кнопки, если они есть
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }

        // Получаем список слотов из GameStateManager
        var slots = GameStateManager.Instance.GetSaveSlots();

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            var slotButtonInstance = Instantiate(slotButtonPrefab, slotsParent); // Создаём кнопку из префаба

            // Устанавливаем текст кнопки
            var slotText = slotButtonInstance.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            slotText.text = $"Slot {i + 1}";

            // Добавляем обработчик нажатия
            int slotIndex = i; // Локальная переменная для замыкания
            slotButtonInstance.GetComponent<Button>().onClick.AddListener(() =>
            {
                HandleSlotSelection(slotIndex); // Вызываем метод обработки выбора слота
            });

            // Отображаем информацию о сохранении
            if (slot.gameState != null)
            {
                slotText.text += $"\nDate: {slot.saveDate}";
            }
            else
            {
                slotText.text += "\nEmpty slot";
            }
        }
    }

    private void HandleSlotSelection(int slotIndex)
    {
        Debug.Log($"Выбран слот {slotIndex + 1}");

        // Проверяем наличие GameStateManager
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager.Instance не инициализирован.");
            return;
        }

        // Получаем список слотов
        var slots = GameStateManager.Instance.GetSaveSlots();

        // Проверяем корректность индекса
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogError($"Индекс слота {slotIndex} вне диапазона. Всего слотов: {slots.Count}");
            return;
        }

        var selectedSlot = slots[slotIndex];
        if (selectedSlot == null)
        {
            Debug.LogError($"Слот с индексом {slotIndex} не найден.");
            return;
        }

        if (selectedSlot.gameState == null)
        {
            Debug.Log($"Slot {slotIndex + 1} is empty.");
            feedbackManager.ShowMessage($"Slot {slotIndex + 1} is empty.");
            return;
        }

        Debug.Log($"slotIndex: {slotIndex}");


        // Загружаем прогресс из слота
        GameStateManager.Instance.SelectSlot(slotIndex);

        loadingScreen.SetActive(true); // Показать экран загрузки
        StartCoroutine(LoadSceneAsync("Scene" + selectedSlot.gameState.currentScene));

        menuCanvasGroup.alpha = 0f;
        slotsCanvasGroup.alpha = 0f;    
       
    }



}
