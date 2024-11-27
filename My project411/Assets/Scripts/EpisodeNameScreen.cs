using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EpisodeNameScreen : MonoBehaviour
{
    public GameObject episodeNamePanel;  // ������ � ��������� ������� � �����
    public TextMeshProUGUI episodeText;  // ����� ��� �������� �������

    private bool isDisplaying = false;
    private Image episodeImage;  // ���� ��� ���������� Image �� ������

    void Awake()
    {
        // ������� ��������� Image �� ������ � ��������� ������ �� ����
        episodeImage = episodeNamePanel.GetComponent<Image>();

        if (episodeImage == null)
        {
            Debug.LogError("��������� Image �� ������ �� episodeNamePanel. �������� Image � ������ � Unity.");
        }
    }

    public void ShowEpisodeScreen(string episodeName, Sprite backgroundImage)
    {
        if (isDisplaying) return; // ���� ����� ��� ������������, �� ������ ������

        isDisplaying = true;
        episodeNamePanel.SetActive(true);  // ���������� ������

        if (episodeImage != null && backgroundImage != null) // ��������, ��� ��������� � ����������� ����������
        {
            episodeImage.sprite = backgroundImage; // ������������� ����������� ����
        }
        else
        {
            Debug.LogError("��� ��� ������� �� �������� ��� ��������� Image �����������.");
        }

        // ��������� �������� ��� ����������� ������ � ���������
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
        // ��������� �������� ��� ������� ������ ����� 5 ������
        StartCoroutine(HideEpisodeScreen());
    }

    private IEnumerator HideEpisodeScreen()
    {
        yield return new WaitForSeconds(3f);
        episodeNamePanel.SetActive(false);
        isDisplaying = false;

        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.SetEpisodeScreenActive(false); // ������������� ����
        }
        else
        {
            Debug.LogError("DialogueManager �� ������!");
        }
    }



}


