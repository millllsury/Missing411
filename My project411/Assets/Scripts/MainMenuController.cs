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
    private float progress = 0f;             // �������� �������� (0-1)
    private float maxWidth;                  // ������ ��������-����

    public CanvasGroup menuCanvasGroup;      // CanvasGroup ��� �������� ���������
    public float fadeInDuration = 1.5f;


    [SerializeField] private GameObject saveSlotsCanvas; // ������ �� Canvas �� �������
    [SerializeField] private Transform slotsParent;      // ������������ ������ ��� ������ ������
    [SerializeField] private GameObject slotButton; // ������ ������ �����

    [SerializeField] private GameObject settingsCanvas; // ������ �� Canvas �� �������

    private GameStateManager gameStateManager;
    private void Start()
    {
        loadingScreen.SetActive(false);
        maxWidth = ((RectTransform)progressFill.transform.parent).rect.width;
        loadingScreen.SetActive(false);      // �������� ����� �������� � ������

        // �������� � ��������� ����������� ����
        menuCanvasGroup.alpha = 0f;
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;

        // ��������� ������ �������� ���������
        StartCoroutine(FadeInMenu());

        gameStateManager = FindFirstObjectByType<GameStateManager>();
        if (gameStateManager == null)
        {
            Debug.LogError("GameStateManager �� ������ � �����. ���������, ��� ������ ������������.");
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

        // ����� ��������� ������ ���� �������������
        menuCanvasGroup.alpha = 1f;
        menuCanvasGroup.interactable = true;
        menuCanvasGroup.blocksRaycasts = true;
    }

    public void NewGame()
    {
        Debug.Log("����� ���� ������.");

        if (gameStateManager == null)
        {
            Debug.LogError("GameStateManager �� ���������������.");
            return;
        }

        // ���������, ���� �� ������ ����
        var emptySlot = gameStateManager.GetSaveSlots().FirstOrDefault(slot => slot.gameState == null);

        if (emptySlot == null)
        {
            Debug.LogWarning("��� ����� ������. �������������� ���� 1.");
            emptySlot = gameStateManager.GetSaveSlots()[0]; // �������������� ������ ����
        }

        // �������������� ����� �������� ����
        emptySlot.gameState = new GameState
        {
            currentScene = "1",
            currentDialogue = "0",
            textCounter = 0,
            flags = new Dictionary<string, bool>()
        };
        emptySlot.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        gameStateManager.SaveSlotsToFile();

        progress = 0f; // ����� ���������
        loadingScreen.SetActive(true); // �������� ����� ��������
        StartCoroutine(LoadSceneAsync("Scene1"));
        menuCanvasGroup.alpha = 0f;
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
        Debug.Log("Quit ������");
        Application.Quit(); // �������� ������ � ��������� ������ ����, �� � ��������� Unity
    }

  
}
