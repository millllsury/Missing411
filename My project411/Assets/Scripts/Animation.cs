using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Animations : MonoBehaviour
{
    [SerializeField] private SpriteRenderer emotionImageLeft;
    [SerializeField] private SpriteRenderer emotionImageRight;
    [SerializeField] private SpriteRenderer eyesImageLeft;
    [SerializeField] private SpriteRenderer eyesImageRight;

    private Dictionary<string, SpriteRenderer> emotionRenderers;
    private Dictionary<string, SpriteRenderer> eyesRenderers;

    private bool isLeftAvatarAnimation = false;
    private bool isRightAvatarAnimation = false;

    private BlinkingManager blinkingManager;
    public bool IsLeftAvatarAnimating => isLeftAvatarAnimation;
    public bool IsRightAvatarAnimating => isRightAvatarAnimation;


    private void Start()
    {
        emotionRenderers = new Dictionary<string, SpriteRenderer>
        {
            { "left", emotionImageLeft },
            { "right", emotionImageRight }
        };

        eyesRenderers = new Dictionary<string, SpriteRenderer>
        {
            { "left", eyesImageLeft },
            { "right", eyesImageRight }
        };

        blinkingManager = FindAnyObjectByType<BlinkingManager>();
        if (blinkingManager == null)
        {
            Debug.LogError("BlinkingManager не найден в сцене!");
        }

    }

    public void PlayAnimation(string characterPosition, string animationName, string character)
    {


        if (string.IsNullOrEmpty(characterPosition) || string.IsNullOrEmpty(animationName) || string.IsNullOrEmpty(character))
        {
            Debug.LogWarning("Недостаточно данных для воспроизведения анимации.");
            return;
        }

        if (!emotionRenderers.TryGetValue(characterPosition, out var emotionRenderer) ||
            !eyesRenderers.TryGetValue(characterPosition, out var eyesRenderer))
        {
            return;
        }

        SpriteNull(emotionRenderer);
        SpriteNull(eyesRenderer);

        bool isAvatarAnimating = (characterPosition == "left") ? isLeftAvatarAnimation : isRightAvatarAnimation;
        if (isAvatarAnimating) return;

        string emotion = null;
        string eyes = null;
        Debug.Log($"Получено animationName: {animationName}");
        switch (animationName.ToLower())
        {
            case "laugh":
                emotion = "happy";
                //eyes = "eyesToTheSideBase";
                break;
            case "sad":
                emotion = "sad";
                break;
            case "happy":
                emotion = "happy";
                eyes = "eyesToTheSideBase";
                break;
            case "tothesidebase":
                eyes = "tothesidebase";
                break;
            case "emotionClosedEyes":
                emotion = "emotionClosedEyes";
                break;
            default:
                Debug.LogWarning($"Анимация {animationName} не найдена.");
                return;
        }

        SetEmotionImage(emotionRenderer, eyesRenderer, character, emotion, eyes);

        if (characterPosition == "left") isLeftAvatarAnimation = true;
        else if (characterPosition == "right") isRightAvatarAnimation = true;

        StartCoroutine(ShowEmotionForDuration(emotionRenderer, eyesRenderer, characterPosition, 3f));
    }

    private void SpriteNull(SpriteRenderer renderer)
    {
        renderer.sprite = null;
    }

    private void SetEmotionImage(SpriteRenderer emotionRenderer, SpriteRenderer eyesRenderer, string character, string emotion, string eyes)
    {
        SetSprite(emotionRenderer, !string.IsNullOrEmpty(emotion) ? $"Characters/{character}/{character}_{emotion}" : null);
        SetSprite(eyesRenderer, !string.IsNullOrEmpty(eyes) ? $"Characters/{character}/{character}_{eyes}" : null);
    }

    private void SetSprite(SpriteRenderer renderer, string path)
    {
        if (renderer == null) return;

        if (string.IsNullOrEmpty(path))
        {
            renderer.gameObject.SetActive(false);
            return;
        }

        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            renderer.sprite = sprite;
            renderer.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log($"Спрайт не найден по пути: {path}");
            renderer.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowEmotionForDuration(SpriteRenderer emotionRenderer, SpriteRenderer eyesRenderer, string characterPosition, float duration)
    {
        blinkingManager.StopBlinking(characterPosition);


        // Ожидание завершения анимации
        yield return FadeIn(emotionRenderer);
        yield return FadeIn(eyesRenderer);

        yield return new WaitForSeconds(duration);

        // Ожидание завершения анимации перед морганием
        yield return FadeOut(emotionRenderer);
        yield return FadeOut(eyesRenderer);

        // Завершаем анимацию
        if (characterPosition == "left") isLeftAvatarAnimation = false;
        else if (characterPosition == "right") isRightAvatarAnimation = false;

        Debug.Log("Анимация завершена, эмоция скрыта.");

        // Проверка перед запуском моргания
        if (blinkingManager != null && eyesRenderers.ContainsKey(characterPosition) && eyesRenderers[characterPosition] != null)
        {
            blinkingManager.StartBlinking(characterPosition, eyesRenderers[characterPosition]);
        }
        else
        {
            Debug.LogWarning($"Не удалось запустить моргание для позиции {characterPosition}. Проверьте настройки.");
        }

        if (blinkingManager != null)
        {
            blinkingManager.StopBlinking(characterPosition);
            blinkingManager.StartBlinking(characterPosition, eyesRenderer); ///////////////
        }
    }


    private IEnumerator FadeIn(SpriteRenderer renderer)
    {
        if (renderer == null) yield break;

        for (float alpha = 0f; alpha <= 1f; alpha += Time.deltaTime * 5) // Регулируем скорость
        {
            SetAlpha(renderer, alpha);
            yield return null;
        }
        SetAlpha(renderer, 1f); // Убедимся, что значение точно 1
    }

    private IEnumerator FadeOut(SpriteRenderer renderer)
    {
        if (renderer == null) yield break;

        for (float alpha = 1f; alpha >= 0f; alpha -= Time.deltaTime * 5)
        {
            SetAlpha(renderer, alpha);
            yield return null;
        }
        SetAlpha(renderer, 0f); // Убедимся, что значение точно 0
    }

    private void SetAlpha(SpriteRenderer renderer, float alpha)
    {
        if (renderer == null) return;

        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }
}


