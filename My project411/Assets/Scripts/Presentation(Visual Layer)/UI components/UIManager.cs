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

    private void Start()
    {
        
    }


    public void OpenSettings()
    {
        QuitConfirmationPanel.SetActive(false);
        settingsCanvas.SetActive(true);
        Time.timeScale = 0; // Останавливаем время при открытии настроек
    }


    public void CloseSettings()
    {
        settingsCanvas.SetActive(false);
        Time.timeScale = 1; // Возвращаем нормальное время после закрытия настроек
        dialogueManager.inputUnavailable = false;
    }


    public void ShowEpisodeScreen(string episodeName, Sprite backgroundImage)
    {
        if (isDisplaying) return; // Если экран уже отображается, не делаем ничего

        isDisplaying = true;
        episodeNamePanel.SetActive(true);  // Показываем панель

        if (episodeImage != null && backgroundImage != null)
        {
            episodeImage.sprite = backgroundImage; // Устанавливаем изображение фона
        }
       

        StartCoroutine(ShowTextWithTypingEffect(episodeName, 0.1f));
    }

    private IEnumerator ShowTextWithTypingEffect(string text, float typingSpeed)
    {
        episodeText.text = "";  // Очищаем текст перед началом
        foreach (char letter in text.ToCharArray())
        {
            episodeText.text += letter;  // Добавляем по одной букве
            yield return new WaitForSeconds(typingSpeed);  // Задержка между буквами
        }

        StartCoroutine(HideEpisodeScreen());
    }

    private IEnumerator HideEpisodeScreen()
    {
        yield return new WaitForSeconds(3f);
        episodeNamePanel.SetActive(false);
        isDisplaying = false;

        if (dialogueManager != null)
        {
            dialogueManager.SetEpisodeScreenActive(false); // Устанавливаем флаг
        }
        else
        {
            Debug.LogError("DialogueManager не найден!");
        }

    }

    public void OnMainMenuClick(GameObject clickedObject)
    {
        // Проверяем, является ли клик на объекте кнопкой "Главное меню"
        if (clickedObject == toMainMenuButton.gameObject)
        {
            // Открываем подтверждение выхода в главное меню
            QuitConfirmationPanel.SetActive(true);
            Time.timeScale = 0;
            dialogueManager.inputUnavailable = true;
            return;
        }

        // Проверяем, можно ли обработать клик на игровом экране
        if (dialogueManager.isChoosing || backgroundController.IsTransitioning || dialogueManager.inputUnavailable) return;

        // Показываем следующий текст
        dialogueManager.ShowNextDialogueText();
    }

    public void MainMenuButtonClick()
    { 

        QuitConfirmationPanel.SetActive(false);
        SaveConfirmationPanel.SetActive(true);
    }

    public void CloseWindow()
    {
        QuitConfirmationPanel.SetActive(false);
        SaveConfirmationPanel.SetActive(false);
        Time.timeScale = 1;
        dialogueManager.inputUnavailable = false;
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

        // Сохраняем изменения в слотах
        GameStateManager.Instance.SaveSlotsToFile();

        // Закрываем панели и возвращаемся в главное меню
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

    public void OpenWardrobe(GameObject clickedObject)
    { 
        int selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();
        dialogueManager.SaveProgress();
        SoundManager.Instance.StopAllSounds();
        GameStateManager.Instance.ClearTracksOnSceneChange();
        SceneManager.LoadScene(wardrobeSceneName);

    }
}
