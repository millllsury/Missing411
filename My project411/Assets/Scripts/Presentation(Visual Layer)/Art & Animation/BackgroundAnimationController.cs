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


    private string currentBackgroundName;

    private Coroutine currentTransitionCoroutine;

    [SerializeField] private float animationFrameDelay = 3f; // Пауза между кадрами
    [SerializeField] private int animationRepeatCount = -1;
    [SerializeField] private Image backgroundLastFrame;
    private List<Sprite> backgroundSprites;
    private Coroutine backgroundAnimationCoroutine;
    private bool isAnimatingBackground = false;
    private bool animationKeepLastFrame = false;
    private string currentBackgroundAnimation;

    [SerializeField] private float foregroundFrameDelay = 3f; // Пауза между кадрами
    [SerializeField] private int foregroundRepeatCount = -1;
    [SerializeField] private Image animationFrame;
    private List<Sprite> foregroundSprites;
    private Coroutine foregroundAnimationCoroutine;
    private string currentForegroundAnimation;
    private bool isAnimatingForeground = false;
    private bool foregroundKeepLastFrame = false;

    private string lastStoredFrame = null; // Храним название последнего кадра
    private bool hasAnimationPlayed = false;

    public bool IsTransitioning { get; private set; }
    public bool IsAnimatingBackground => isAnimatingBackground;
    public bool IsAnimatingForeground => isAnimatingForeground;

    public string GetCurrentBackgroundName() => currentBackgroundName;
    // Для заднего плана
   


    [SerializeField] private CharacterManager characterManager;  


    [SerializeField] private CanvasGroup uiElements; // CanvasGroup для UI элементов
    [SerializeField] private CanvasGroup uiButtons;
    [SerializeField] private GameObject charactersParent; // Родительский объект для персонажей

    [SerializeField] private CanvasGroup keysCanvas;

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
            if (GameStateManager.Instance.HasBeenTransited())
            {
                return;
            }
            else
            {
                currentTransitionCoroutine = StartCoroutine(SmoothBackgroundTransitionWithElements(backgroundName));
                GameStateManager.Instance.SetHasTransited(true);
            }
            
            
        }
        else
        {
            SetBackground(backgroundName);
            backgroundLastFrame.gameObject.SetActive(false);

        }
    }

    private IEnumerator SmoothBackgroundTransitionWithElements(string backgroundName)
    {
        IsTransitioning = true;
       
        // 1. Отключаем UI и персонажей
        ToggleElements(false);
        
        // 2. Плавное затемнение экрана (до 50%)
        float fadeDuration = 1.5f;
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
        animationFrame.gameObject.SetActive(false);
        backgroundLastFrame.gameObject.SetActive(false);
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

        if (isAnimatingForeground)
        {
            animationFrame.gameObject.SetActive(true);
        }
        // 5. Плавное включение элементов
        yield return StartCoroutine(FadeInElements());

       
        IsTransitioning = false;
        currentTransitionCoroutine = null;
    }


    private void ToggleElements(bool isActive)
    {
        if(uiButtons != null) {
            uiButtons.interactable = isActive;
            uiElements.blocksRaycasts = isActive;
        }

        if (uiElements != null)
        {
            uiElements.alpha = isActive ? 1f : 0f;
            uiElements.interactable = isActive;
            uiElements.blocksRaycasts = isActive;

            keysCanvas.alpha = isActive ? 1f : 0f;
            keysCanvas.interactable = isActive;
            keysCanvas.blocksRaycasts = isActive;

        }

         characterManager.StartCoroutine(characterManager.FadeOutCharacters(charactersParent.transform));
        
    }


    private IEnumerator FadeInElements()
    {
        float duration = 1f;
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

            if (uiButtons != null)
            {
                uiButtons.interactable = true;
                uiElements.blocksRaycasts = true;
            }


            keysCanvas.alpha = 1f;
            keysCanvas.interactable = true;
            keysCanvas.blocksRaycasts = true;
        }

        

        if (charactersParent != null)
        {
            Debug.Log($"[FadeInElements] StartBlinking called for .... at {Time.time}");
            yield return characterManager.StartCoroutine(characterManager.FadeInCharacters(charactersParent.transform));
           
        }
    }


    public void SetBackground(string backgroundName)
    {
        Sprite bgSprite = Resources.Load<Sprite>("Backgrounds/" + backgroundName);

        if (backgroundName == "fall9")
        {
            keysCanvas.alpha = 0f;
            keysCanvas.interactable = false;
        }

        if (bgSprite != null)
        {
            GameStateManager.Instance.ClearBackgroundAnimation();
            backgroundImage.sprite = bgSprite; // Устанавливаем спрайт
            GameStateManager.Instance.SaveBackground(backgroundName);
        }
        else
        {
            Debug.LogError($"Фон {backgroundName} не найден в папке Resources/Backgrounds.");
        }
    }


    #endregion

    #region Background Animation
    public void StartBackgroundAnimation(string animationFolder, float delay, int repeatCount = -1, bool keepLastFrame = false, string currentSoundName = null, string animationType = "background")
    {
        Debug.Log($" Получено animationType: {animationType} для {animationFolder}");

        Sprite[] sprites = Resources.LoadAll<Sprite>("Backgrounds/" + animationFolder);
        if (sprites.Length == 0)
        {
            Debug.LogError($"Анимация не найдена в папке Resources/Backgrounds/{animationFolder}");
            return;
        }

        if (animationType.ToLower() == "background")
        {
            backgroundSprites = new List<Sprite>(sprites);
            PlayBackgroundAnimation(animationFolder, delay, repeatCount, currentSoundName, keepLastFrame);
            Debug.Log(" Начинаем анимацию заднего фона.");
        }
        else if (animationType.ToLower() == "foreground")
        {
            foregroundSprites = new List<Sprite>(sprites);
            PlayForegroundAnimation(animationFolder, delay, repeatCount, currentSoundName, keepLastFrame);
            Debug.Log("Начинаем анимацию переднего фона.");
        }
        else
        {
            Debug.LogError($" Неизвестный тип анимации: {animationType}. Допустимые значения: 'foreground', 'background'.");
        }
    }


    public void PlayForegroundAnimation(string animationName, float delay, int repeatCount, string soundName, bool keepLastFrame)
    {
        
        if (foregroundAnimationCoroutine != null)
            StopCoroutine(foregroundAnimationCoroutine);
        foregroundFrameDelay = delay;

        isAnimatingForeground = true;
        currentForegroundAnimation = animationName; // Сохраняем имя анимации
        foregroundAnimationCoroutine = StartCoroutine(PlayAnimation(animationName, animationFrame, foregroundSprites, delay, repeatCount, keepLastFrame));

        if (!string.IsNullOrEmpty(soundName))
        {
            SoundManager.Instance.PlaySoundByName(soundName);
        }
    }


    public void PlayBackgroundAnimation(string animationName, float delay, int repeatCount, string soundName, bool keepLastFrame)
    {
        
        if (backgroundAnimationCoroutine != null)
            StopCoroutine(backgroundAnimationCoroutine);
        animationFrameDelay = delay;

        isAnimatingBackground = true;
        currentBackgroundAnimation = animationName; // Сохраняем имя анимации
        backgroundAnimationCoroutine = StartCoroutine(PlayAnimation(animationName, backgroundLastFrame, backgroundSprites, delay, repeatCount, keepLastFrame));

        if (!string.IsNullOrEmpty(soundName))
        {
            SoundManager.Instance.PlaySoundByName(soundName);
        }
    }


    private IEnumerator PlayAnimation(string animationName, Image animationImage, List<Sprite> sprites, float frameDelay, int repeatCount, bool keepLastFrame)
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

        // Всегда сохраняем и задний, и передний план
        if (animationImage == backgroundLastFrame)
        {
            GameStateManager.Instance.SaveBackgroundAnimation(animationName, frameDelay, repeatCount, keepLastFrame);
        }
        else if (animationImage == animationFrame)
        {
            GameStateManager.Instance.SaveForegroundAnimation(animationName, frameDelay, repeatCount, keepLastFrame);
        }


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
            isAnimatingBackground = false;
            GameStateManager.Instance.ClearForegroundAnimation();///////////
            backgroundAnimationCoroutine = null;
        }
        else
        {
            animationImage.gameObject.SetActive(false);
            isAnimatingForeground = false;
            GameStateManager.Instance.ClearForegroundAnimation();
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
        GameStateManager.Instance.ClearForegroundAnimation();
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
        GameStateManager.Instance.ClearBackgroundAnimation();
        backgroundLastFrame.gameObject.SetActive(false);
    }

    public (string, float, int, bool) GetCurrentBackgroundAnimationData()
    {
        return (currentBackgroundAnimation, animationFrameDelay, animationRepeatCount, animationKeepLastFrame);
    }


    public (string, float, int, bool) GetCurrentForegroundAnimationData()
    {
        return (currentForegroundAnimation, foregroundFrameDelay, foregroundRepeatCount, foregroundKeepLastFrame);
    }


    public bool HasAnimationPlayed() => hasAnimationPlayed;
    #endregion
}
