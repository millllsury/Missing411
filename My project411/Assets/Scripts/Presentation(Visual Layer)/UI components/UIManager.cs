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


    public GameObject episodeNamePanel;  // ������ � ��������� ������� � �����
    public TextMeshProUGUI episodeText;  // ����� ��� �������� �������

    private bool isDisplaying = false;
    private Image episodeImage;  // ���� ��� ���������� Image �� ������

    public string wardrobeSceneName = "WardrobeScene";

    [SerializeField] private GameObject settingsCanvas;

    private void Start()
    {
        
    }


    public void OpenSettings()
    {
        QuitConfirmationPanel.SetActive(false);
        settingsCanvas.SetActive(true);
        Time.timeScale = 0; // ������������� ����� ��� �������� ��������
    }


    public void CloseSettings()
    {
        settingsCanvas.SetActive(false);
        Time.timeScale = 1; // ���������� ���������� ����� ����� �������� ��������
        dialogueManager.inputUnavailable = false;
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
            Debug.LogError("GameStateManager �� ���������������.");
            return;
        }

        int selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();

        if (selectedSlotIndex == -1)
        {
            Debug.LogWarning("���� �� ������. ������ ���������������.");
            return;
        }

        var saveSlots = GameStateManager.Instance.GetSaveSlots();
        if (saveSlots == null || selectedSlotIndex >= saveSlots.Count || saveSlots[selectedSlotIndex] == null)
        {
            Debug.LogError($"������: ���� {selectedSlotIndex} �� ���������� ��� ����.");
            return;
        }

        if (GameStateManager.Instance.isNewGame)
        {
            GameStateManager.Instance.ClearSlot(selectedSlotIndex);
            Debug.Log($"����� ���� ���� ������, �� �� ���������. ���� {selectedSlotIndex + 1} ������.");
        }
        else if (GameStateManager.Instance.originalState != null)
        {
            GameStateManager.Instance.GetSaveSlots()[selectedSlotIndex].gameState = JsonConvert.DeserializeObject<GameState>(
                JsonConvert.SerializeObject(GameStateManager.Instance.originalState));
            Debug.Log($"�������� ��������� ����� {selectedSlotIndex + 1} �������������.");
        }
        else
        {
            GameStateManager.Instance.ClearSlot(selectedSlotIndex);
            Debug.Log($"���� {selectedSlotIndex + 1} ��� ���� � ������.");
        }

        // ��������� ��������� � ������
        GameStateManager.Instance.SaveSlotsToFile();

        // ��������� ������ � ������������ � ������� ����
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
