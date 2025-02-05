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
        if (isAnimatingBackground)
        {
            //Debug.Log($"Попытка запустить новую анимацию {animationFolder}, пока предыдущая ещё выполняется ({currentAnimationName}).");
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
            Debug.LogError($"Анимация не найдена в папке Resources/Backgrounds/{animationFolder}");
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
            Debug.LogError("Список спрайтов для анимации пуст.");
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
