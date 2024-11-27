using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Animations : MonoBehaviour
{
    [SerializeField] private Animator leftAnimator;
    [SerializeField] private Animator rightAnimator;

    [SerializeField] private SpriteRenderer emotionImageLeft;
    [SerializeField] private SpriteRenderer emotionImageRight;

    [SerializeField] private SpriteRenderer eyesImageLeft;
    [SerializeField] private SpriteRenderer eyesImageRight;

    private CharacterManager characterManager;

    private Dictionary<string, Animator> animators;
    private Dictionary<string, SpriteRenderer> emotionRenderers;
    private Dictionary<string, SpriteRenderer> eyesRenderers;

    private void Start()
    {
        characterManager = FindFirstObjectByType<CharacterManager>();

        // Инициализация словарей
        animators = new Dictionary<string, Animator> {
            { "left", leftAnimator },
            { "right", rightAnimator }
        };

        emotionRenderers = new Dictionary<string, SpriteRenderer> {
            { "left", emotionImageLeft },
            { "right", emotionImageRight }
        };

        eyesRenderers = new Dictionary<string, SpriteRenderer> {
            { "left", eyesImageLeft },
            { "right", eyesImageRight }
        };
    }

    public void PlayAnimation(string characterPosition, string animationName, string character)
    {
        if (string.IsNullOrEmpty(characterPosition) || string.IsNullOrEmpty(animationName) || string.IsNullOrEmpty(character))
        {
            Debug.LogWarning("Недостаточно данных для воспроизведения анимации.");
            return;
        }

        // Используем словари для получения соответствующих объектов
        Animator animator = GetAnimator(characterPosition);
        SpriteRenderer emotionRenderer = GetEmotionRenderer(characterPosition);
        SpriteRenderer eyesRenderer = GetEyesRenderer(characterPosition);

        if (animator != null)
        {
            Debug.Log($"Активируем анимацию: {animationName}");

            emotionRenderer.gameObject.SetActive(false); // Отключаем изображение эмоции перед воспроизведением

            if ((characterPosition == "left" && !characterManager.IsLeftAvatarAnimating) ||
                (characterPosition == "right" && !characterManager.IsRightAvatarAnimating))
            {
                TriggerAnimation(animationName, animator, emotionRenderer, eyesRenderer, character);
            }
        }
    }

    private Animator GetAnimator(string characterPosition) => animators.ContainsKey(characterPosition) ? animators[characterPosition] : null;

    private SpriteRenderer GetEmotionRenderer(string characterPosition) => emotionRenderers.ContainsKey(characterPosition) ? emotionRenderers[characterPosition] : null;

    private SpriteRenderer GetEyesRenderer(string characterPosition) => eyesRenderers.ContainsKey(characterPosition) ? eyesRenderers[characterPosition] : null;

    private void TriggerAnimation(string animationName, Animator animator, SpriteRenderer emotionRenderer, SpriteRenderer eyesRenderer, string character)
    {
        string emotion = ""; // There will be the values for SetEmotionImage()
        string eyes = "";

        switch (animationName.ToLower())
        {
            case "laugh":
                animator.SetTrigger("LaughTrigger");
                emotion = "happy";
                break;

            case "sad":
                animator.SetTrigger("SadTrigger");
                break;

            default:
                Debug.LogWarning("Анимация не найдена: " + animationName);
                return;
        }

        SetEmotionImage(emotionRenderer, eyesRenderer, character, emotion, eyes);
        StartCoroutine(ShowEmotionForAnimationDuration(animator, emotionRenderer, eyesRenderer));
    }

    private void SetEmotionImage(SpriteRenderer emotionRenderer, SpriteRenderer eyesRenderer, string character, string emotion, string eyes = null)
    {
        // Устанавливаем изображение эмоции
        if (emotionRenderer != null)
        {
            Sprite emotionSprite = Resources.Load<Sprite>($"Characters/{character}_{emotion}");
            if (emotionSprite != null)
            {
                emotionRenderer.sprite = emotionSprite;
                emotionRenderer.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Эмоция не найдена: {character}_{emotion}");
                emotionRenderer.gameObject.SetActive(false);
            }
        }

        // Устанавливаем изображение глаз
        if (!string.IsNullOrEmpty(eyes) && eyesRenderer != null)
        {
            Sprite eyesSprite = Resources.Load<Sprite>($"Characters/{character}_{eyes}");
            if (eyesSprite != null)
            {
                eyesRenderer.sprite = eyesSprite;
                eyesRenderer.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Глаза не найдены: {character}_{eyes}");
                eyesRenderer.gameObject.SetActive(false);
            }
        }
        else
        {
            eyesRenderer?.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowEmotionForAnimationDuration(Animator animator, SpriteRenderer emotionRenderer, SpriteRenderer eyesRenderer)
    {
        // Ожидаем окончания анимации
        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"Текущее состояние анимации: {stateInfo.shortNameHash}");

        if (stateInfo.length > 0)
        {
            float animationDuration = stateInfo.length;
            yield return new WaitForSeconds(animationDuration + 1f);

            emotionRenderer.gameObject.SetActive(false);
            eyesRenderer?.gameObject.SetActive(false);

            Debug.Log("Анимация завершена, эмоция скрыта.");
        }
        else
        {
            Debug.LogWarning("Не удалось получить информацию о текущем анимационном клипе.");
        }
    }
}
