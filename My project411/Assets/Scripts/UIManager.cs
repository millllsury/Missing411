using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;


public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject QuitConfirmationPanel;
    [SerializeField] private GameObject SaveConfirmationPanel;
    [SerializeField] private Button toMainMenuButton;

    [SerializeField] private DialogueManager dialogueManager;
    private BackgroundController backgroundController;
    //private GameSaveManager gameSaveManager;
    public GameObject episodeNamePanel;  // Панель с названием эпизода и фоном
    public TextMeshProUGUI episodeText;  // Текст для названия эпизода

    private bool isDisplaying = false;
    private Image episodeImage;  // Поле для компонента Image на панели

    public string wardrobeSceneName = "WardrobeScene";

    private void Start()
    {
        // Инициализация episodeImage
        if (episodeNamePanel != null)
        {
            episodeImage = episodeNamePanel.GetComponent<Image>();
            if (episodeImage == null)
            {
                Debug.LogError("Компонент Image не найден на episodeNamePanel.");
            }
        }
        else
        {
            Debug.LogError("episodeNamePanel не назначена в инспекторе.");
        }
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
        else
        {
            if (episodeImage == null)
                Debug.LogError("Компонент Image отсутствует на episodeNamePanel.");
            if (backgroundImage == null)
                Debug.LogError("Передан пустой фон для эпизода.");
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

    public void GoToMainMenuConfirmation()
    {
        dialogueManager.SaveProgress(); // Сохраняем прогресс перед выходом в меню
        QuitConfirmationPanel.SetActive(false);
        SaveConfirmationPanel.SetActive(true);
    }

    public void GoToMainMenuRejection()
    {
        QuitConfirmationPanel.SetActive(false);
        SaveConfirmationPanel.SetActive(false);
        Time.timeScale = 1;
        dialogueManager.inputUnavailable = false;
    }

    public void SaveConfirmation()
    {
        dialogueManager.SaveProgress();
        QuitConfirmationPanel.SetActive(false);
        SaveConfirmationPanel.SetActive(false);
        Time.timeScale = 1;
        dialogueManager.inputUnavailable = false;
        SceneManager.LoadScene("MainMenu");

    }

    public void SaveRejection()
    {
        QuitConfirmationPanel.SetActive(false);
        SaveConfirmationPanel.SetActive(false);
        Time.timeScale = 1;
        dialogueManager.inputUnavailable = false;
        SceneManager.LoadScene("MainMenu");

    }

    public void OpenWardrobe(GameObject clickedObject)
    { 
         int selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();
        dialogueManager.SaveProgress();
        SceneManager.LoadScene(wardrobeSceneName);
    }
}
