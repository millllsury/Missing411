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
    public GameObject episodeNamePanel;  // ������ � ��������� ������� � �����
    public TextMeshProUGUI episodeText;  // ����� ��� �������� �������

    private bool isDisplaying = false;
    private Image episodeImage;  // ���� ��� ���������� Image �� ������

    public string wardrobeSceneName = "WardrobeScene";

    private void Start()
    {
        // ������������� episodeImage
        if (episodeNamePanel != null)
        {
            episodeImage = episodeNamePanel.GetComponent<Image>();
            if (episodeImage == null)
            {
                Debug.LogError("��������� Image �� ������ �� episodeNamePanel.");
            }
        }
        else
        {
            Debug.LogError("episodeNamePanel �� ��������� � ����������.");
        }
    }


    public void ShowEpisodeScreen(string episodeName, Sprite backgroundImage)
    {
        if (isDisplaying) return; // ���� ����� ��� ������������, �� ������ ������

        isDisplaying = true;
        episodeNamePanel.SetActive(true);  // ���������� ������

        if (episodeImage != null && backgroundImage != null)
        {
            episodeImage.sprite = backgroundImage; // ������������� ����������� ����
        }
        else
        {
            if (episodeImage == null)
                Debug.LogError("��������� Image ����������� �� episodeNamePanel.");
            if (backgroundImage == null)
                Debug.LogError("������� ������ ��� ��� �������.");
        }

        StartCoroutine(ShowTextWithTypingEffect(episodeName, 0.1f));
    }

    private IEnumerator ShowTextWithTypingEffect(string text, float typingSpeed)
    {
        episodeText.text = "";  // ������� ����� ����� �������
        foreach (char letter in text.ToCharArray())
        {
            episodeText.text += letter;  // ��������� �� ����� �����
            yield return new WaitForSeconds(typingSpeed);  // �������� ����� �������
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
            dialogueManager.SetEpisodeScreenActive(false); // ������������� ����
        }
        else
        {
            Debug.LogError("DialogueManager �� ������!");
        }

    }

    public void OnMainMenuClick(GameObject clickedObject)
    {
        // ���������, �������� �� ���� �� ������� ������� "������� ����"
        if (clickedObject == toMainMenuButton.gameObject)
        {
            // ��������� ������������� ������ � ������� ����
            QuitConfirmationPanel.SetActive(true);
            Time.timeScale = 0;
            dialogueManager.inputUnavailable = true;
            return;
        }

        // ���������, ����� �� ���������� ���� �� ������� ������
        if (dialogueManager.isChoosing || backgroundController.IsTransitioning || dialogueManager.inputUnavailable) return;

        // ���������� ��������� �����
        dialogueManager.ShowNextDialogueText();
    }

    public void GoToMainMenuConfirmation()
    {
        dialogueManager.SaveProgress(); // ��������� �������� ����� ������� � ����
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
