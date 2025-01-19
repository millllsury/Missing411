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
    public float fadeInDuration = 1.5f;


    [SerializeField] private GameObject saveSlotsCanvas; // Ссылка на Canvas со слотами
    [SerializeField] private Transform slotsParent;      // Родительский объект для кнопок слотов
    [SerializeField] private GameObject slotButton; // Префаб кнопки слота

    [SerializeField] private GameObject settingsCanvas; // Ссылка на Canvas со слотами

    private GameStateManager gameStateManager;
    private void Start()
    {
        loadingScreen.SetActive(false);
        maxWidth = ((RectTransform)progressFill.transform.parent).rect.width;
        loadingScreen.SetActive(false);      // Скрываем экран загрузки в начале

        // Начинаем с полностью прозрачного меню
        menuCanvasGroup.alpha = 0f;
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;

        // Запускаем эффект плавного появления
        StartCoroutine(FadeInMenu());

        gameStateManager = FindFirstObjectByType<GameStateManager>();
        if (gameStateManager == null)
        {
            Debug.LogError("GameStateManager не найден в сцене. Убедитесь, что объект присутствует.");
            return;
        }
        gameStateManager.LoadSaveSlots();
        
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
        Debug.Log("Новая игра начата.");

        if (gameStateManager == null)
        {
            Debug.LogError("GameStateManager не инициализирован.");
            return;
        }

        // Проверяем, есть ли пустой слот
        var emptySlot = gameStateManager.GetSaveSlots().FirstOrDefault(slot => slot.gameState == null);

        if (emptySlot == null)
        {
            Debug.LogWarning("Все слоты заняты. Перезаписываем Слот 1.");
            emptySlot = gameStateManager.GetSaveSlots()[0]; // Перезаписываем первый слот
        }

        // Инициализируем новый прогресс игры
        emptySlot.gameState = new GameState
        {
            currentScene = "1",
            currentDialogue = "0",
            textCounter = 0,
            flags = new Dictionary<string, bool>()
        };
        emptySlot.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        gameStateManager.SaveSlotsToFile();

        progress = 0f; // Сброс прогресса
        loadingScreen.SetActive(true); // Показать экран загрузки
        StartCoroutine(LoadSceneAsync("Scene1"));
        menuCanvasGroup.alpha = 0f;
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
       
        Debug.Log($"Попытка загрузить сцену: {sceneName}");
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError($"Сцена '{sceneName}' не существует или не добавлена в Build Settings!");
            yield break;
        }

        operation.allowSceneActivation = false;
        Debug.Log("Загрузка началась...");

        while (!operation.isDone)
        {
            Debug.Log($"operation.progress: {operation.progress}");
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            progress = Mathf.MoveTowards(progress, targetProgress, Time.deltaTime / loadTime);
            UpdateProgress(progress);

            if (progress >= 0.99f) // Условие с небольшим допуском
            {
                Debug.Log("Сцена готова к активации");
                operation.allowSceneActivation = true;
            }


            yield return null;
        }

        Debug.Log("Сцена успешно загружена!");
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

  
}
