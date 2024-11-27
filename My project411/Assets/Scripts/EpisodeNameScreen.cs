using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EpisodeNameScreen : MonoBehaviour
{
    public GameObject episodeNamePanel;  // Панель с названием эпизода и фоном
    public TextMeshProUGUI episodeText;  // Текст для названия эпизода

    private bool isDisplaying = false;
    private Image episodeImage;  // Поле для компонента Image на панели

    void Awake()
    {
        // Находим компонент Image на панели и сохраняем ссылку на него
        episodeImage = episodeNamePanel.GetComponent<Image>();

        if (episodeImage == null)
        {
            Debug.LogError("Компонент Image не найден на episodeNamePanel. Добавьте Image к панели в Unity.");
        }
    }

    public void ShowEpisodeScreen(string episodeName, Sprite backgroundImage)
    {
        if (isDisplaying) return; // Если экран уже отображается, не делаем ничего

        isDisplaying = true;
        episodeNamePanel.SetActive(true);  // Показываем панель

        if (episodeImage != null && backgroundImage != null) // Проверка, что компонент и изображение существуют
        {
            episodeImage.sprite = backgroundImage; // Устанавливаем изображение фона
        }
        else
        {
            Debug.LogError("Фон для эпизода не загружен или компонент Image отсутствует.");
        }

        // Запускаем корутину для отображения текста с анимацией
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
        // Запускаем корутину для скрытия экрана после 5 секунд
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
            dialogueManager.SetEpisodeScreenActive(false); // Устанавливаем флаг
        }
        else
        {
            Debug.LogError("DialogueManager не найден!");
        }
    }



}


