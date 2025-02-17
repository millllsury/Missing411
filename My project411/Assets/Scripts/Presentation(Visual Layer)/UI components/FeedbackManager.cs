using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI feedbackText; // Поле для отображения текста
    [SerializeField] private CanvasGroup feedbackPanel; // Панель, которая будет анимироваться
    [SerializeField] private float displayTime = 3f; // Время показа
    [SerializeField] private float fadeSpeed = 1f; // Скорость исчезновения
    public static FeedbackManager Instance { get; private set; }
    private Queue<string> messageQueue = new Queue<string>(); // Очередь уведомлений
    private bool isDisplaying = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void ShowMessage(string message)
    {
        messageQueue.Enqueue(message);

        if (!isDisplaying)
        {
            StartCoroutine(DisplayMessages());
        }
    }

    private IEnumerator DisplayMessages()
    {
        isDisplaying = true;

        while (messageQueue.Count > 0)
        {
            feedbackText.text = messageQueue.Dequeue();
            feedbackPanel.alpha = 1f;
            yield return new WaitForSeconds(displayTime);

            // Анимация исчезновения
            while (feedbackPanel.alpha > 0)
            {
                feedbackPanel.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
        }

        isDisplaying = false;
    }
}
