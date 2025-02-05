using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    [Header("Background Settings")]
    [SerializeField] private SpriteRenderer backgroundImage; // SpriteRenderer ��� ����
    [SerializeField] private Image darkOverlay;

    [Header("Background Animation Settings")]
    [SerializeField] private Image animationFrame;
    [SerializeField] private Image backgroundLastFrame;
    [SerializeField] private float frameDelay = 3f; // ����� ����� �������
    [SerializeField] private int repeatCount = -1;
    private string soundName = string.Empty;

    private Coroutine currentTransitionCoroutine;
    private Coroutine animationCoroutine;
    private List<Sprite> animationSprites;
    private bool isAnimatingBackground = false;
    private bool keepLastFrame = false;
    private string currentBackgroundName;
    private string currentAnimationName;
    private string currentSoundName;
    private bool hasAnimationPlayed = false;

    public bool IsTransitioning { get; private set; }
    public bool IsAnimatingBackground => isAnimatingBackground;

    public string GetCurrentAnimationName() => currentAnimationName;

    public string GetSoundName() => currentSoundName;

    public float GetCurrentFrameDelay() => frameDelay;

    public int GetCurrentRepeatCount() => repeatCount;

    public bool GetKeepLastFrame() => keepLastFrame;

    public string GetCurrentBackgroundName() => currentBackgroundName;

    [SerializeField] private CharacterManager characterManager;  


    [SerializeField] private CanvasGroup uiElements; // CanvasGroup ��� UI ���������
    [SerializeField] private GameObject charactersParent; // ������������ ������ ��� ����������

   
    
    #region Background Management
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
            currentTransitionCoroutine = StartCoroutine(SmoothBackgroundTransitionWithElements(backgroundName));
        }
        else
        {
            SetBackground(backgroundName);
        }
    }

    private IEnumerator SmoothBackgroundTransitionWithElements(string backgroundName)
    {
        IsTransitioning = true;

        // 1. ��������� UI � ����������
        ToggleElements(false);

        // 2. ������� ���������� ������ (�� 50%)
        float fadeDuration = 3f;
        float elapsedTime = 0f;
        

        while (elapsedTime < fadeDuration)
        {
            Color overlayColor = darkOverlay.color;
            overlayColor.a = Mathf.Lerp(0f, 0.8f, elapsedTime / fadeDuration);
            darkOverlay.color = overlayColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        darkOverlay.color = new Color(0, 0, 0, 0.8f);
        animationFrame.gameObject.SetActive(false); ////
        // 3. ������ ���
        SetBackground(backgroundName);

        // 4. ������� ���������� ������ (�� 0%)
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            Color overlayColor = darkOverlay.color;
            overlayColor.a = Mathf.Lerp(0.8f, 0f, elapsedTime / fadeDuration);
            darkOverlay.color = overlayColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        darkOverlay.color = new Color(0, 0, 0, 0f);
        
        // 5. ������� ��������� ���������
        yield return StartCoroutine(FadeInElements());

        if (isAnimatingBackground)
        {
            animationFrame.gameObject.SetActive(true);
        }
        IsTransitioning = false;
        currentTransitionCoroutine = null;
    }


    private void ToggleElements(bool isActive)
    {
        if (uiElements != null)
        {
            uiElements.alpha = isActive ? 1f : 0f;
            uiElements.interactable = isActive;
            uiElements.blocksRaycasts = isActive;
        }

         characterManager.StartCoroutine(characterManager.FadeOutCharacters(charactersParent.transform));
        
    }


    private IEnumerator FadeInElements()
    {
        float duration = 1.5f;
        float elapsedTime = 0f;

        if (uiElements != null)
        {
            while (elapsedTime < duration)
            {
                uiElements.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // ����������� ������ ���������
            uiElements.alpha = 1f;
            uiElements.interactable = true;
            uiElements.blocksRaycasts = true;

        }

        if (charactersParent != null)
        {
            yield return characterManager.StartCoroutine(characterManager.FadeInCharacters(charactersParent.transform));
        }
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
            Debug.LogError($"��� {backgroundName} �� ������ � ����� Resources/Backgrounds.");
        }
    }


    #endregion

    #region Background Animation
    public void StartBackgroundAnimation(string animationFolder, float delay, int repeatCount = -1, bool keepLastFrame = false, string currentSoundName = null)
    {
        if (isAnimatingBackground)
        {
            //Debug.Log($"������� ��������� ����� �������� {animationFolder}, ���� ���������� ��� ����������� ({currentAnimationName}).");
            StopBackgroundAnimation();
        }

        Sprite[] sprites = Resources.LoadAll<Sprite>("Backgrounds/" + animationFolder);
        if (sprites.Length > 0)
        {
            animationSprites = new List<Sprite>(sprites);
            frameDelay = delay;
            this.repeatCount = repeatCount;
            this.keepLastFrame = keepLastFrame;
            currentAnimationName = animationFolder;
            this.currentSoundName = soundName;
            isAnimatingBackground = true;
            animationCoroutine = StartCoroutine(PlayBackgroundAnimation());

            if (currentSoundName != null)
            {
                SoundManager.Instance.PlaySoundByName(currentSoundName);
            }

        }
        else
        {
            Debug.LogError($"�������� �� ������� � ����� Resources/Backgrounds/{animationFolder}");
        }
    }

    public void StopBackgroundAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        isAnimatingBackground = false;
        animationFrame.gameObject.SetActive(false);
    }

    private IEnumerator PlayBackgroundAnimation()
    {
        if (animationSprites == null || animationSprites.Count == 0)
        {
            Debug.LogError("������ �������� ��� �������� ����.");
            yield break;
        }



        int currentFrame = 0;
        int playedCount = 0;
        int lastFrameIndex = 0;

        animationFrame.gameObject.SetActive(true);

        while (repeatCount == -1 || playedCount < repeatCount)
        {
            animationFrame.sprite = animationSprites[currentFrame];
            lastFrameIndex = currentFrame;

            currentFrame = (currentFrame + 1) % animationSprites.Count;
            if (currentFrame == 0)
            {
                playedCount++;
            }

            yield return new WaitForSeconds(frameDelay);
        }

        if (keepLastFrame)
        {
            animationFrame.sprite = animationSprites[lastFrameIndex];
        }
        else
        {
            animationFrame.gameObject.SetActive(false);
        }

        isAnimatingBackground = false;
        hasAnimationPlayed = true;
        animationCoroutine = null;
        if (!keepLastFrame)
        {
            GameStateManager.Instance.ClearBackgroundAnimation();
        }
    }

    public bool HasAnimationPlayed() => hasAnimationPlayed;
    #endregion
}
