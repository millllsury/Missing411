using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;

public class BlinkingManager : MonoBehaviour
{


    private Dictionary<string, Coroutine> blinkingCoroutines = new Dictionary<string, Coroutine>();

    public bool IsLeftAvatarAnimating { get; set; } = false; // Флаг анимации левого аватара
    public bool IsRightAvatarAnimating { get; set; } = false; // Флаг анимации правого аватара

    public delegate bool AnimationCheckDelegate();
    public AnimationCheckDelegate IsExternalAnimationPlaying;




    public void StartBlinking(string characterName, SpriteRenderer eyesImage)
    {
        if (string.IsNullOrEmpty(characterName) || eyesImage == null)
        {
            Debug.LogWarning("Character name or eyesImage is null. Blinking will not start.");
            return;
        }

        // Остановить старую корутину, если она есть
        if (blinkingCoroutines.ContainsKey(characterName))
        {
            StopBlinking(characterName);
        }

        // Запуск новой корутины
        Coroutine coroutine = StartCoroutine(BlinkCoroutine(eyesImage, characterName));
        blinkingCoroutines[characterName] = coroutine;
        Debug.LogWarning("корутина");
    }


    public void StopBlinking(string characterPosition)
    {
        if (blinkingCoroutines.TryGetValue(characterPosition, out Coroutine coroutine) && coroutine != null)
        {
            StopCoroutine(coroutine);
            blinkingCoroutines.Remove(characterPosition);
            Debug.Log("Coroutine Stopped");
        }
    }



    public void StopAllBlinking()
    {
        foreach (var coroutine in blinkingCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        blinkingCoroutines.Clear();
    }

    private IEnumerator BlinkCoroutine(SpriteRenderer eyesImage, string characterName)
    {
        if (string.IsNullOrEmpty(characterName))
        {
            Debug.LogWarning("Character name is null or empty. Blink animation will not start.");
            yield break;
        }

        while (true)
        {
            if ((!IsLeftAvatarAnimating && !IsRightAvatarAnimating) &&
                (IsExternalAnimationPlaying == null || !IsExternalAnimationPlaying()))
            {
                Sprite closedEyesSprite = Resources.Load<Sprite>($"Characters/{characterName}/{characterName}_ClosedEyes");
                if (closedEyesSprite == null)
                {
                    Debug.LogWarning($"Closed eyes sprite for {characterName} not found. Retrying...");
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                // Показ закрытых глаз.
                eyesImage.sprite = closedEyesSprite;
                eyesImage.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.2f);

                // Скрытие глаз.
                eyesImage.gameObject.SetActive(false);
                yield return new WaitForSeconds(Random.Range(3f, 5f));
            }
            else
            {
                yield return null;
            }
        }
    }


}