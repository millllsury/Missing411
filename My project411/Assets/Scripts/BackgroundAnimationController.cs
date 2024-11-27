using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundAnimationController : MonoBehaviour
{
    [SerializeField] private Image animationFrame;
    [SerializeField] private float frameDelay = 3f; // Пауза между кадрами
    [SerializeField] private int repeatCount = -1;
    [SerializeField] private List<Sprite> animationSprites;

    private Coroutine animationCoroutine;

    public string CurrentAnimation { get; private set; }

    private bool isAnimating = false;
    public bool IsAnimating => isAnimating;

    [SerializeField] private bool keepLastFrame = false;
    void Start()
    {
        if (animationFrame == null)
        {
            animationFrame = GetComponent<Image>();
        }
    }



    public void StartAnimation(List<Sprite> sprites, float delay, string animationName, int repeatCount = -1, bool keepLastFrame = false)
    {
        if (isAnimating)
        {
            Debug.LogWarning("Попытка запустить новую анимацию, пока предыдущая ещё выполняется.");
            StopAnimation();
        }

        CurrentAnimation = animationName; // Устанавливаем имя текущей анимации
        animationSprites = sprites;
        frameDelay = delay;
        this.repeatCount = repeatCount;
        this.keepLastFrame = keepLastFrame;

        if (animationSprites != null && animationSprites.Count > 0)
        {
            isAnimating = true;
            animationCoroutine = StartCoroutine(PlayAnimation());
        }
        else
        {
            Debug.LogError("Список спрайтов для анимации пуст.");
        }
    }


    public void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        isAnimating = false;
    }

    private IEnumerator PlayAnimation()
    {
        if (animationSprites == null || animationSprites.Count == 0)
        {
            Debug.LogError("Список спрайтов для анимации пуст.");
            yield break; // Прерываем выполнение анимации, если список пуст
        }

        if (animationFrame == null)
        {
            Debug.LogError("Компонент Image не найден. Убедитесь, что он прикреплён к объекту.");
            yield break; // Прерываем выполнение анимации, если animationFrame не инициализирован
        }

        int currentFrame = 0;
        int playedCount = 0; // Счётчик завершённых циклов
        int lastFrameIndex = 0; // Переменная для хранения последнего кадра, который нужно показать

        while (repeatCount == -1 || playedCount < repeatCount)
        {
            // Показываем текущий кадр анимации
            animationFrame.sprite = animationSprites[currentFrame];
            //Debug.Log($"Frame {currentFrame}, repeatCount: {repeatCount}, playedCount: {playedCount}");

            // Обновляем lastFrameIndex каждый раз, когда показываем новый кадр
            lastFrameIndex = currentFrame;

            currentFrame = (currentFrame + 1) % animationSprites.Count;

            if (currentFrame == 0)
            {
                playedCount++; // Увеличиваем счётчик повторений при завершении цикла
            }

            yield return new WaitForSeconds(frameDelay);
        }

        // После завершения цикла, проверяем флаг keepLastFrame
        if (keepLastFrame)
        {
            Debug.Log($"Animation finished. Showing last frame: {lastFrameIndex} and playedCount: {playedCount}");
            animationFrame.sprite = animationSprites[lastFrameIndex];
        }
        else
        {
            // Выключаем объект анимации после завершения, если keepLastFrame == false
            Debug.Log("Disabling background animation object.");
            gameObject.SetActive(false);
        }

        isAnimating = false;
        animationCoroutine = null;
    }

}