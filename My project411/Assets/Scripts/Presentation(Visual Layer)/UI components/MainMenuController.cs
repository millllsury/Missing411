using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{
    
    public GameObject loadingScreen;
    public Image progressFill;
    public RectTransform thumb;
    public float loadTime = 1.5f;
    private float progress = 0f;     
    private float maxWidth;           

    public CanvasGroup menuCanvasGroup;    
    public CanvasGroup slotsCanvasGroup;
    public float fadeInDuration = 1.5f;

    [SerializeField] private GameObject saveSlotsCanvas; 
    [SerializeField] private Transform slotsParent;     
    [SerializeField] private GameObject slotButtonPrefab; 

    [SerializeField] private GameObject settingsCanvas; 

    private FeedbackManager feedbackManager;
    [SerializeField] private GameObject noSavesText; 

    [SerializeField] private GameObject rewriteSlotPanel;

    private void Start()
    {
        feedbackManager = FindFirstObjectByType<FeedbackManager>();
        loadingScreen.SetActive(false);
        maxWidth = ((RectTransform)progressFill.transform.parent).rect.width;
        loadingScreen.SetActive(false); 


        menuCanvasGroup.alpha = 0f;
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;


        StartCoroutine(FadeInMenu());


        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager не найден в сцене. Убедитесь, что объект присутствует.");
            return;
        }
        GameStateManager.Instance.LoadSaveSlots();
        PopulateSaveSlots();
  
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


        menuCanvasGroup.alpha = 1f;
        menuCanvasGroup.interactable = true;
        menuCanvasGroup.blocksRaycasts = true;
    }


    private int emptySlotIndex = -1; 

    public void NewGame()
    {
        Debug.Log("New Game is started.");

        if (GameStateManager.Instance == null)
        {
            return;
        }


        emptySlotIndex = GameStateManager.Instance.GetSaveSlots()
            .FindIndex(slot => slot.gameState == null);

        if (emptySlotIndex == -1)
        {
            rewriteSlotPanel.SetActive(true);
            Debug.LogWarning("All slots are full. Waiting for confirmation to overwrite Slot 1.");
            return; //
        }


        GameStateManager.Instance.isNewGame = true;
        StartNewGame(emptySlotIndex);
    }


    public void ConfirmOverwriteSlot()
    {
        emptySlotIndex = 0;
        GameStateManager.Instance.isNewGame = true; 
        rewriteSlotPanel.SetActive(false);
        StartNewGame(emptySlotIndex);
    }


    public void CancelOverwriteSlot()
    {
        rewriteSlotPanel.SetActive(false);
        Debug.Log("Slot overwrite canceled.");
    }

 
    private void StartNewGame(int slotIndex)
    {
        var saveSlots = GameStateManager.Instance.GetSaveSlots();
        var emptySlot = saveSlots[slotIndex];

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
            unlockedHairstyles = new List<int> { 0, 1 },
            unlockedClothes = new List<int> { 0, 1 },
            collectedKeys = new List<string>()
        };

        emptySlot.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        GameStateManager.Instance.SelectSlot(slotIndex);
        GameStateManager.Instance.SaveSlotsToFile();

        progress = 0f;
        loadingScreen.SetActive(true);
        StartCoroutine(LoadSceneAsync("Scene1"));
        menuCanvasGroup.alpha = 0f;
    }



    private IEnumerator LoadSceneAsync(string sceneName)
    {
       
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
           
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            progress = Mathf.MoveTowards(progress, targetProgress, Time.deltaTime / loadTime);
            UpdateProgress(progress);

            if (progress >= 0.99f) 
            {
               
                operation.allowSceneActivation = true;
            }


            yield return null;
        }
        
       
    }



    private void UpdateProgress(float progress)
    {
       
        float newWidth = maxWidth * progress;
        progressFill.rectTransform.sizeDelta = new Vector2(newWidth, progressFill.rectTransform.sizeDelta.y);

    
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
        Application.Quit(); 
    }


    private void PopulateSaveSlots()
    {
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }

        var slots = GameStateManager.Instance.GetSaveSlots();
        bool hasSaves = false; 

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];

            if (slot.gameState == null)
            {
                continue;
            }

            hasSaves = true; 

            var slotButtonInstance = Instantiate(slotButtonPrefab, slotsParent);
            var slotText = slotButtonInstance.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            slotText.text = $"Slot {i + 1}\nDate: {slot.saveDate}";

            int slotIndex = i;
            slotButtonInstance.GetComponent<Button>().onClick.AddListener(() => HandleSlotSelection(slotIndex));

            Button deleteButton = slotButtonInstance.transform.Find("DeleteButton")?.GetComponent<Button>();
            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener(() => DeleteSlot(slotIndex, slotButtonInstance));
            }
        }

        if (noSavesText != null)
        {
            noSavesText.SetActive(!hasSaves);
        }
    }



    private void DeleteSlot(int slotIndex, GameObject slotUIElement)
    {
        GameStateManager.Instance.ClearSlot(slotIndex);
        Destroy(slotUIElement);
        Debug.Log($"Слот {slotIndex + 1} очищен.");
    }

    private void HandleSlotSelection(int slotIndex)
    {
        Debug.Log($"Slot {slotIndex + 1} selected");

        if (GameStateManager.Instance == null)
        {
            return;
        }
        
        var slots = GameStateManager.Instance.GetSaveSlots();

        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogError($"Slot index {slotIndex} is out of r=the range.");
            return;
        }

        var selectedSlot = slots[slotIndex];
        if (selectedSlot == null)
        {
            Debug.LogError($"Slot {slotIndex} isn't found.");
            return;
        }

        if (selectedSlot.gameState == null)
        {
            Debug.Log($"Slot {slotIndex + 1} is empty.");
            feedbackManager.ShowMessage($"Slot {slotIndex + 1} is empty.");
            return;
        }

        Debug.Log($"slotIndex: {slotIndex}");


        // loading progress
        GameStateManager.Instance.SelectSlot(slotIndex);

        loadingScreen.SetActive(true);
        StartCoroutine(LoadSceneAsync("Scene" + selectedSlot.gameState.currentScene));

        menuCanvasGroup.alpha = 0f;
        slotsCanvasGroup.alpha = 0f;    
       
    }



}
