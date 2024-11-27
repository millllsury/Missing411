using System.Collections;
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

    private void Start()
    {
        loadingScreen.SetActive(false);
        maxWidth = ((RectTransform)progressFill.transform.parent).rect.width;
        loadingScreen.SetActive(false);      // Скрываем экран загрузки в начале
    }

    public void NewGame()
    {
        progress = 0f; // Сброс прогресса
        loadingScreen.SetActive(true);       // Показать экран загрузки
        StartCoroutine(LoadSceneAsync("Scene1"));
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
        Debug.Log("Continue Game нажата");
    }

    public void OpenSettings()
    {
        Debug.Log("Settings нажата");
    }

    public void QuitGame()
    {
        Debug.Log("Quit нажата");
        Application.Quit(); // Работает только в собранной версии игры, не в редакторе Unity
    }

    
}
