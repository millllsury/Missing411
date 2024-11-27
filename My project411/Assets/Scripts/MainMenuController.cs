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

    private float progress = 0f;             // �������� �������� (0-1)
    private float maxWidth;                  // ������ ��������-����

    private void Start()
    {
        loadingScreen.SetActive(false);
        maxWidth = ((RectTransform)progressFill.transform.parent).rect.width;
        loadingScreen.SetActive(false);      // �������� ����� �������� � ������
    }

    public void NewGame()
    {
        progress = 0f; // ����� ���������
        loadingScreen.SetActive(true);       // �������� ����� ��������
        StartCoroutine(LoadSceneAsync("Scene1"));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
       
        Debug.Log($"������� ��������� �����: {sceneName}");
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError($"����� '{sceneName}' �� ���������� ��� �� ��������� � Build Settings!");
            yield break;
        }

        operation.allowSceneActivation = false;
        Debug.Log("�������� ��������...");

        while (!operation.isDone)
        {
            Debug.Log($"operation.progress: {operation.progress}");
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            progress = Mathf.MoveTowards(progress, targetProgress, Time.deltaTime / loadTime);
            UpdateProgress(progress);

            if (progress >= 0.99f) // ������� � ��������� ��������
            {
                Debug.Log("����� ������ � ���������");
                operation.allowSceneActivation = true;
            }


            yield return null;
        }

        Debug.Log("����� ������� ���������!");
    }



    private void UpdateProgress(float progress)
    {
        // ��������� ������ ���������� ��������-����
        float newWidth = maxWidth * progress;
        progressFill.rectTransform.sizeDelta = new Vector2(newWidth, progressFill.rectTransform.sizeDelta.y);

        // ��������� ������ ������� � ������������ ��� �������
        float thumbWidth = thumb.rect.width;
        thumb.anchoredPosition = new Vector2(newWidth - (thumbWidth / 2), thumb.anchoredPosition.y);
    }

    public void ContinueGame()
    {
        Debug.Log("Continue Game ������");
    }

    public void OpenSettings()
    {
        Debug.Log("Settings ������");
    }

    public void QuitGame()
    {
        Debug.Log("Quit ������");
        Application.Quit(); // �������� ������ � ��������� ������ ����, �� � ��������� Unity
    }

    
}
