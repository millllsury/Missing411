using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FeedbackManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private CanvasGroup feedbackPanel;
    [SerializeField] private float displayTime = 3f;
    [SerializeField] private float fadeSpeed = 1f;
    public static FeedbackManager Instance { get; private set; }

    private Queue<string> messageQueue = new Queue<string>();
    private bool isDisplaying = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += ClearMessagesOnSceneChange;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= ClearMessagesOnSceneChange;
    }

    private void ClearMessagesOnSceneChange(Scene scene, LoadSceneMode mode)
    {
        messageQueue.Clear();
        feedbackText.text = "";
        feedbackPanel.alpha = 0f;
    }

    public void ShowMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

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

            while (feedbackPanel.alpha > 0)
            {
                feedbackPanel.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
        }

        isDisplaying = false;
    }
}
