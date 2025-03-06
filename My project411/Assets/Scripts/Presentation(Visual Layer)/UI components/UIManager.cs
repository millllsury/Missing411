using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Newtonsoft.Json;


public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject QuitConfirmationPanel;
    [SerializeField] private GameObject SaveConfirmationPanel;
    [SerializeField] private Button toMainMenuButton;

    [SerializeField] private DialogueManager dialogueManager;
    private BackgroundController backgroundController;


    public GameObject episodeNamePanel;  // Панель с названием эпизода и фоном
    public TextMeshProUGUI episodeText;  // Текст для названия эпизода

    private bool isDisplaying = false;
    private Image episodeImage;  // Поле для компонента Image на панели

    public string wardrobeSceneName = "WardrobeScene";

    [SerializeField] private GameObject settingsCanvas;

    [SerializeField] private Button saveGameButton; 

    private void Start()
    {
        if (saveGameButton!=null)
        {
            UpdateSaveButtonState();
        }
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (QuitConfirmationPanel != null)
            {
                HandleEscapeKey();
            }
            
        }
    }

    private void HandleEscapeKey()
    {
        if (QuitConfirmationPanel.activeSelf)
        {
           
            QuitConfirmationPanel.SetActive(false);
            Time.timeScale = 1;
            dialogueManager.inputUnavailable = false;
            return;
        }

        if (SaveConfirmationPanel.activeSelf)
        {
            SaveConfirmationPanel.SetActive(false);
            Time.timeScale = 1;
            dialogueManager.inputUnavailable = false;
            return;
        }

       
        if (settingsCanvas.activeSelf)
        {
            CloseSettings();
            return;
        }

        OnMainMenuClick(toMainMenuButton.gameObject);
    }


    public void OpenSettings()
    {
        QuitConfirmationPanel.SetActive(false);
        settingsCanvas.SetActive(true);
        Time.timeScale = 0; 
    }


    public void CloseSettings()
    {
        settingsCanvas.SetActive(false);
        Time.timeScale = 1; 
        dialogueManager.inputUnavailable = false;
    }


    public void ShowEpisodeScreen(string episodeName, Sprite backgroundImage)
    {
        if (isDisplaying) return; 

        isDisplaying = true;
        episodeNamePanel.SetActive(true); 

        if (episodeImage != null && backgroundImage != null)
        {
            episodeImage.sprite = backgroundImage; 
        }
       

        StartCoroutine(ShowTextWithTypingEffect(episodeName, 0.1f));
    }

    private IEnumerator ShowTextWithTypingEffect(string text, float typingSpeed)
    {
        episodeText.text = ""; 
        foreach (char letter in text.ToCharArray())
        {
            episodeText.text += letter; 
            yield return new WaitForSeconds(typingSpeed);  
        }

        StartCoroutine(HideEpisodeScreen());
    }

    private IEnumerator HideEpisodeScreen()
    {
        yield return new WaitForSeconds(2f);
        episodeNamePanel.SetActive(false);
        isDisplaying = false;

        if (dialogueManager != null)
        {
            dialogueManager.SetEpisodeScreenActive(false);
        }
        else
        {
            Debug.LogError("DialogueManager не найден!");
        }

    }

    public void OnMainMenuClick(GameObject clickedObject)
    {
        
        if (clickedObject == toMainMenuButton.gameObject)
        {
            
            QuitConfirmationPanel.SetActive(true);
            Time.timeScale = 0;
            dialogueManager.inputUnavailable = true;
            return;
        }

        
        if (dialogueManager.isChoosing || backgroundController.IsTransitioning || dialogueManager.inputUnavailable) return;

       
        dialogueManager.ShowNextDialogueText();
    }

    public void MainMenuButtonClick()
    {
        if (!GameStateManager.Instance.HasSaved())
        {
            SaveConfirmationPanel.SetActive(true);
            QuitConfirmationPanel.SetActive(false);
        }
        else
        {
            QuitConfirmationPanel.SetActive(false);
            Time.timeScale = 1;
            dialogueManager.inputUnavailable = false;
            SceneManager.LoadScene("MainMenu");
            SoundManager.Instance.StopAllSounds();
        }
        
    }

    public void CloseWindow()
    {
        QuitConfirmationPanel.SetActive(false);
        SaveConfirmationPanel.SetActive(false);
        Time.timeScale = 1;
        dialogueManager.inputUnavailable = false;
    }
    public void SaveGame()
    {
        dialogueManager.SaveProgress();
        GameStateManager.Instance.isNewGame = false;
        GameStateManager.Instance.SaveOriginalState(GameStateManager.Instance.GetSelectedSlotIndex()); 
        GameStateManager.Instance.SetHasSaved(true); 
        saveGameButton.interactable = false; 
    }


    public void SaveConfirmation()
    {
        GameStateManager.Instance.isNewGame = false;
        dialogueManager.SaveProgress();
        QuitConfirmationPanel.SetActive(false);
        SaveConfirmationPanel.SetActive(false);
        Time.timeScale = 1;
        dialogueManager.inputUnavailable = false;
        SceneManager.LoadScene("MainMenu");

    }

    public void OnGameStateChanged()
    {
        GameStateManager.Instance.SetHasSaved(false); 
        saveGameButton.interactable = true; 
    }

    private void UpdateSaveButtonState()
    {
        saveGameButton.interactable = !GameStateManager.Instance.HasSaved();
    }

    public void ExitWithoutSaving()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager не инициализирован.");
            return;
        }

        int selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();

        if (selectedSlotIndex == -1)
        {
            Debug.LogWarning("Слот не выбран. Нечего восстанавливать.");
            return;
        }

        var saveSlots = GameStateManager.Instance.GetSaveSlots();
        if (saveSlots == null || selectedSlotIndex >= saveSlots.Count || saveSlots[selectedSlotIndex] == null)
        {
            Debug.LogError($"Ошибка: Слот {selectedSlotIndex} не существует или пуст.");
            return;
        }
        

        if (GameStateManager.Instance.isNewGame)
        {
            GameStateManager.Instance.ClearSlot(selectedSlotIndex);
            Debug.Log($"Новая игра была начата, но не сохранена. Слот {selectedSlotIndex + 1} удален.");
        }
        else if (GameStateManager.Instance.originalState != null)
        {
            GameStateManager.Instance.GetSaveSlots()[selectedSlotIndex].gameState = JsonConvert.DeserializeObject<GameState>(
                JsonConvert.SerializeObject(GameStateManager.Instance.originalState));
            Debug.Log($"Исходное состояние слота {selectedSlotIndex + 1} восстановлено.");
        }
        else
        {
            GameStateManager.Instance.ClearSlot(selectedSlotIndex);
            Debug.Log($"Слот {selectedSlotIndex + 1} был пуст и удален.");
        }

        
        GameStateManager.Instance.SaveSlotsToFile();

        if (QuitConfirmationPanel != null)
        {
            QuitConfirmationPanel.SetActive(false);
        }

        if (SaveConfirmationPanel != null)
        {
            SaveConfirmationPanel.SetActive(false);
        }

        Time.timeScale = 1;
        if (dialogueManager != null)
        {
            dialogueManager.inputUnavailable = false;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopAllSounds();
        }

        SceneManager.LoadScene("MainMenu");
    }


    public void UiClick()
    {
        SoundManager.Instance.UIClickSound();
    }

    public void UiClickSound(string name)
    {
        SoundManager.Instance.PlaySoundByName(name);
    }

    public void OpenWardrobe(GameObject clickedObject)
    {
       
        int selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();
        dialogueManager.SaveProgress();
        SoundManager.Instance.StopAllSounds();
        GameStateManager.Instance.ClearTracksOnSceneChange();
        SceneManager.LoadScene(wardrobeSceneName);
       
    }
}
