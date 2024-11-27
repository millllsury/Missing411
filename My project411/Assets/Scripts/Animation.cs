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

        animators = new Dictionary<string, Animator>
        {
            { "left", leftAnimator },
            { "right", rightAnimator }
        };

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
    }

    public void PlayAnimation(string characterPosition, string animationName, string character)
    {
        if (string.IsNullOrEmpty(characterPosition) || string.IsNullOrEmpty(animationName) || string.IsNullOrEmpty(character))
        {
            Debug.LogWarning("Недостаточно данных для воспроизведения анимации.");
            return;
        }

        if (!animators.TryGetValue(characterPosition, out var animator)) return;
        if (!emotionRenderers.TryGetValue(characterPosition, out var emotionRenderer)) return;
        if (!eyesRenderers.TryGetValue(characterPosition, out var eyesRenderer)) return;

        if ((characterPosition == "left" && !characterManager.IsLeftAvatarAnimating) ||
            (characterPosition == "right" && !characterManager.IsRightAvatarAnimating))
        {
            emotionRenderer.gameObject.SetActive(false); // Отключаем изображение эмоции перед анимацией
            TriggerAnimation(animationName, animator, emotionRenderer, eyesRenderer, character);
        }
    }

    private void TriggerAnimation(string animationName, Animator animator, SpriteRenderer emotionRenderer, SpriteRenderer eyesRenderer, string character)
    {
        string emotion = null;
        string eyes = null;

        switch (animationName.ToLower())
        {
            case "laugh":
                animator.SetTrigger("LaughTrigger");
                emotion = "happy";
                break;

            case "sad":
                animator.SetTrigger("SadTrigger");
                emotion = "sad";
                break;

            default:
                Debug.LogWarning($"Анимация {animationName} не найдена.");
                return;
        }

        SetEmotionImage(emotionRenderer, eyesRenderer, character, emotion, eyes);
        StartCoroutine(ShowEmotionForAnimationDuration(animator, emotionRenderer, eyesRenderer));
    }

    private void SetEmotionImage(SpriteRenderer emotionRenderer, SpriteRenderer eyesRenderer, string character, string emotion, string eyes)
    {
        SetSprite(emotionRenderer, $"Characters/{character}_{emotion}");
        SetSprite(eyesRenderer, !string.IsNullOrEmpty(eyes) ? $"Characters/{character}_{eyes}" : null);
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
            Debug.LogWarning($"Спрайт не найден по пути: {path}");
            renderer.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowEmotionForAnimationDuration(Animator animator, SpriteRenderer emotionRenderer, SpriteRenderer eyesRenderer)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.length == 0)
        {
            Debug.LogWarning("Не удалось получить длину анимации.");
            yield break;
        }

        float animationDuration = stateInfo.length;
        yield return new WaitForSeconds(animationDuration + 1f);

        emotionRenderer.gameObject.SetActive(false);
        eyesRenderer?.gameObject.SetActive(false);

        Debug.Log("Анимация завершена, эмоция скрыта.");
    }
}
