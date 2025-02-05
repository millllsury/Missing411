using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    [Header("Background Settings")]
    [SerializeField] private SpriteRenderer backgroundImage; // SpriteRenderer для фона
    [SerializeField] private Image darkOverlay;

    [Header("Background Animation Settings")]
    [SerializeField] private Image animationFrame;
    [SerializeField] private Image backgroundLastFrame;
    [SerializeField] private float frameDelay = 3f; // Пауза между кадрами
    [SerializeField] private int repeatCount = -1;

    private Coroutine currentTransitionCoroutine;
    

    private Coroutine foregroundAnimationCoroutine;
    private Coroutine backgroundAnimationCoroutine;
    private List<Sprite> foregroundSprites;
    private List<Sprite> backgroundSprites;

    private bool isAnimatingBackground = false;
    private bool isAnimatingForeground = false;
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


    [SerializeField] private CanvasGroup uiElements; // CanvasGroup для UI элементов
    [SerializeField] private GameObject charactersParent; // Родительский объект для персонажей

   
    
    #region Background Management
    public void SetBackgroundSmooth(string backgroundName, bool smoothTransition)
    {
        if (currentBackgroundName == backgroundName) return; // Проверяем, что фон меняется
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

        // 1. Отключаем UI и персонажей
        ToggleElements(false);

        // 2. Плавное затемнение экрана (до 50%)
        float fadeDuration = 3f;
        float elapsedTime = 0f;

        animationFrame.gameObject.SetActive(false);
        backgroundLastFrame.gameObject.SetActive(false);
        while (elapsedTime < fadeDuration)
        {
            Color overlayColor = darkOverlay.color;
            overlayColor.a = Mathf.Lerp(0f, 0.8f, elapsedTime / fadeDuration);
            darkOverlay.color = overlayColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        darkOverlay.color = new Color(0, 0, 0, 0.8f);

        // 3. Меняем фон
        SetBackground(backgroundName);

        // 4. Плавное осветление экрана (до 0%)
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
        
        // 5. Плавное включение элементов
        yield return StartCoroutine(FadeInElements());

        if (isAnimatingForeground)
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

            // Гарантируем полную видимость
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
            backgroundImage.sprite = bgSprite; // Устанавливаем спрайт
        }
        else
        {
            Debug.LogError($"Фон {backgroundName} не найден в папке Resources/Backgrounds.");
        }
    }


    #endregion

    #region Background Animation
    public void StartBackgroundAnimation(string animationFolder, float delay, int repeatCount = -1, bool keepLastFrame = false, string currentSoundName = null)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Backgrounds/" + animationFolder);
        if (sprites.Length == 0)
        {
            Debug.LogError($"Анимация не найдена в папке Resources/Backgrounds/{animationFolder}");
            return;
        }

        if (keepLastFrame)  // Анимация для заднего плана
        {
            backgroundSprites = new List<Sprite>(sprites);
            PlayBackgroundAnimation(delay, repeatCount, currentSoundName);
        }
        else  // Анимация для переднего плана
        {
            foregroundSprites = new List<Sprite>(sprites);
            PlayForegroundAnimation(delay, repeatCount, currentSoundName);
        }
    }

    private void PlayForegroundAnimation(float delay, int repeatCount, string soundName)
    {
        if (foregroundAnimationCoroutine != null)
            StopCoroutine(foregroundAnimationCoroutine);

        isAnimatingForeground = true;
        foregroundAnimationCoroutine = StartCoroutine(PlayAnimation(animationFrame, foregroundSprites, delay, repeatCount, false));

        if (!string.IsNullOrEmpty(soundName))
        {
            SoundManager.Instance.PlaySoundByName(soundName);
        }
    }

    private void PlayBackgroundAnimation(float delay, int repeatCount, string soundName)
    {
        if (backgroundAnimationCoroutine != null)
            StopCoroutine(backgroundAnimationCoroutine);

        isAnimatingBackground = true;
        backgroundAnimationCoroutine = StartCoroutine(PlayAnimation(backgroundLastFrame, backgroundSprites, delay, repeatCount, true));

        if (!string.IsNullOrEmpty(soundName))
        {
            SoundManager.Instance.PlaySoundByName(soundName);
        }
    }

    private IEnumerator PlayAnimation(Image animationImage, List<Sprite> sprites, float frameDelay, int repeatCount, bool keepLastFrame)
    {
        if (sprites == null || sprites.Count == 0)
        {
            Debug.LogError("Список спрайтов для анимации пуст.");
            yield break;
        }

        int currentFrame = 0;
        int playedCount = 0;
        int lastFrameIndex = 0;

        animationImage.gameObject.SetActive(true);

        while (repeatCount == -1 || playedCount < repeatCount)
        {
            animationImage.sprite = sprites[currentFrame];
            lastFrameIndex = currentFrame;

            currentFrame = (currentFrame + 1) % sprites.Count;
            if (currentFrame == 0)
            {
                playedCount++;
            }

            yield return new WaitForSeconds(frameDelay);
        }

        if (keepLastFrame)
        {
            animationImage.sprite = sprites[lastFrameIndex];
        }
        else
        {
            animationImage.gameObject.SetActive(false);
        }

        if (keepLastFrame)
        {
            isAnimatingBackground = false;
            backgroundAnimationCoroutine = null;
        }
        else
        {
            isAnimatingForeground = false;
            foregroundAnimationCoroutine = null;
        }
    }

    public void StopForegroundAnimation()
    {
        if (foregroundAnimationCoroutine != null)
        {
            StopCoroutine(foregroundAnimationCoroutine);
            foregroundAnimationCoroutine = null;
        }
        isAnimatingForeground = false;
        animationFrame.gameObject.SetActive(false);
    }

    public void StopBackgroundAnimation()
    {
        if (backgroundAnimationCoroutine != null)
        {
            StopCoroutine(backgroundAnimationCoroutine);
            backgroundAnimationCoroutine = null;
        }
        isAnimatingBackground = false;
        backgroundLastFrame.gameObject.SetActive(false);
    }

    public bool HasAnimationPlayed() => hasAnimationPlayed;
    #endregion
}
