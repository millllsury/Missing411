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
    public CanvasGroup slotsCanvasGroup;
    public float fadeInDuration = 1.5f;

    private SoundManager soundManager;

    [SerializeField] private GameObject saveSlotsCanvas; // ������ �� Canvas �� �������
    [SerializeField] private Transform slotsParent;      // ������������ ������ ��� ������ ������
    [SerializeField] private GameObject slotButtonPrefab; // ������ ������

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
        PopulateSaveSlots();
        soundManager = FindFirstObjectByType<SoundManager>();
        if (soundManager == null)
        {
            Debug.LogError("SoundManager �� ������. ���������, ��� ������ ������������ �� �����.");
            return;
        }

        soundManager.PlaySoundByName("MainMenuSound");
        soundManager.PlaySoundByName("owl");
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
        var emptySlotIndex = gameStateManager.GetSaveSlots()
            .FindIndex(slot => slot.gameState == null);

        if (emptySlotIndex == -1)
        {
            Debug.LogWarning("��� ����� ������. �������������� ���� 1.");
            emptySlotIndex = 0; // �������������� ������ ����
        }

        // �������������� ����� �������� ����
        var saveSlots = gameStateManager.GetSaveSlots();
        var emptySlot = saveSlots[emptySlotIndex];
        emptySlot.gameState = new GameState
        {
            currentScene = "1",
            currentDialogue = "1",
            textCounter = 0,
            flags = new Dictionary<string, bool>()
        };
        emptySlot.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // ������������� ��������� ����
        gameStateManager.SelectSlot(emptySlotIndex);

        // ��������� ����� � ����
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
        SoundManager.Instance.StopAllSounds();

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

    

    public void PopulateSaveSlots()
    {
        // ������� ������ ������, ���� ��� ����
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }

        // �������� ������ ������ �� GameStateManager
        var slots = GameStateManager.Instance.GetSaveSlots();

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            var slotButtonInstance = Instantiate(slotButtonPrefab, slotsParent); // ������ ������ �� �������

            // ������������� ����� ������
            var slotText = slotButtonInstance.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            slotText.text = $"Slot {i + 1}";

            // ��������� ���������� �������
            int slotIndex = i; // ��������� ���������� ��� ���������
            slotButtonInstance.GetComponent<Button>().onClick.AddListener(() =>
            {
                HandleSlotSelection(slotIndex); // �������� ����� ��������� ������ �����
            });

            // ���������� ���������� � ����������
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
        Debug.Log($"������ ���� {slotIndex + 1}");

        // ��������� ������� GameStateManager
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager.Instance �� ���������������.");
            return;
        }

        // �������� ������ ������
        var slots = GameStateManager.Instance.GetSaveSlots();

        // ��������� ������������ �������
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogError($"������ ����� {slotIndex} ��� ���������. ����� ������: {slots.Count}");
            return;
        }

        var selectedSlot = slots[slotIndex];
        if (selectedSlot == null)
        {
            Debug.LogError($"���� � �������� {slotIndex} �� ������.");
            return;
        }

        if (selectedSlot.gameState == null)
        {
            Debug.LogWarning($"���� {slotIndex + 1} ����.");
            return;
        }

        Debug.Log($"slotIndex: {slotIndex}");


        // ��������� �������� �� �����
        GameStateManager.Instance.SelectSlot(slotIndex);

        loadingScreen.SetActive(true); // �������� ����� ��������
        StartCoroutine(LoadSceneAsync("Scene" + selectedSlot.gameState.currentScene));

        menuCanvasGroup.alpha = 0f;
        slotsCanvasGroup.alpha = 0f;    
       
    }



}
