using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SceneController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundImage; // SpriteRenderer ��� ����
    [SerializeField] private BackgroundAnimationController backgroundAnimationController;

    public bool IsTransitioning { get; private set; }

    //private CanvasGroup canvasGroup;

    void Start()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<SpriteRenderer>();
            if (backgroundImage == null)
            {
                Debug.LogError("SpriteRenderer ��� ���� �� ������!");
            }
        }

        if (backgroundAnimationController == null)
        {
            // ���� ��������� � �������� �������
            backgroundAnimationController = GetComponentInChildren<BackgroundAnimationController>();
            if (backgroundAnimationController == null)
            {
                Debug.LogError("BackgroundAnimationController �� ������! ��������� ������� ������� BackgroundAnimationFrame.");
            }
        }
    }
    private Coroutine currentTransitionCoroutine;
    private string currentBackgroundName;

    public void SetBackgroundSmooth(string backgroundName, bool smoothTransition)
    {
        if (currentBackgroundName == backgroundName) return; // ���������, ��� ��� ��������
        currentBackgroundName = backgroundName;

        if (smoothTransition)
        {
            if (currentTransitionCoroutine != null)
            {
                StopCoroutine(currentTransitionCoroutine);
            }
            currentTransitionCoroutine = StartCoroutine(SmoothBackgroundTransition(backgroundName));
        }
        else
        {
            SetBackground(backgroundName);
        }
    }


    public Image darkOverlay;
    private IEnumerator SmoothBackgroundTransition(string backgroundName)
    {
        IsTransitioning = true;

        // ���������� �� 50%
        float fadeDuration = 2f;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            Color overlayColor = darkOverlay.color;
            overlayColor.a = Mathf.Lerp(0f, 0.5f, elapsedTime / fadeDuration);
            darkOverlay.color = overlayColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        darkOverlay.color = new Color(0, 0, 0, 0.5f);

        // ������ ���
        SetBackground(backgroundName);

        // ���������� �� ������ ���������
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            Color overlayColor = darkOverlay.color;
            overlayColor.a = Mathf.Lerp(0.5f, 0f, elapsedTime / fadeDuration);
            darkOverlay.color = overlayColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        darkOverlay.color = new Color(0, 0, 0, 0f);

        IsTransitioning = false;
        currentTransitionCoroutine = null;
    }



    public void SetBackground(string backgroundName)
    {
        Sprite bgSprite = Resources.Load<Sprite>("Backgrounds/" + backgroundName);
        if (bgSprite != null)
        {
            backgroundImage.sprite = bgSprite; // ������������� ������
        }
        else
        {
            Debug.LogError("��� " + backgroundName + " �� ������ � ����� Resources/Backgrounds.");
        }
    }

    public void StartBackgroundAnimation(string animationFolder, float delay, int repeatCount = -1, bool keepLastFrame = false)
    {
        if (backgroundAnimationController != null && backgroundAnimationController.IsAnimating)
        {
            Debug.Log($"������������� ������� �������� {backgroundAnimationController.CurrentAnimation} ����� �������� ����� {animationFolder}.");
            backgroundAnimationController.StopAnimation();
        }

        Sprite[] sprites = Resources.LoadAll<Sprite>("Backgrounds/" + animationFolder);

        if (sprites.Length > 0)
        {
            if (backgroundAnimationController != null && backgroundAnimationController.gameObject != null)
            {
                backgroundAnimationController.gameObject.SetActive(true);
            }

            // �������� ��� ����� � StartAnimation
            backgroundAnimationController.StartAnimation(new List<Sprite>(sprites), delay, animationFolder, repeatCount, keepLastFrame);
        }
        else
        {
            Debug.LogError("�������� �� ������� � ����� Resources/Backgrounds/" + animationFolder);
        }
    }

    public void StopBackgroundAnimation()
    {
        if (backgroundAnimationController != null)
        {
            Debug.Log("Stopping background animation...");
            backgroundAnimationController.StopAnimation();
            // ��������� ������ ����� ��������� ��������
            backgroundAnimationController.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("������� ���������� ��������, �� BackgroundAnimationController �� ������.");
        }
    }

    public bool IsAnimatingBackground(string animationFolder)
    {
        if (backgroundAnimationController == null) return false;

        return backgroundAnimationController.IsAnimating &&
               backgroundAnimationController.CurrentAnimation == animationFolder;
    }
}
