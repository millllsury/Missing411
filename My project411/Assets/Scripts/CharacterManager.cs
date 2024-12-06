using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer leftAvatar;
    [SerializeField] private SpriteRenderer rightAvatar;

    [SerializeField] private SpriteRenderer leftEyesImage;
    [SerializeField] private SpriteRenderer rightEyesImage;

    [SerializeField] private GameObject speakerPanelLeft;
    [SerializeField] private GameObject speakerPanelCenter;
    [SerializeField] private GameObject speakerPanelRight;

    private Coroutine leftBlinkCoroutine;
    private Coroutine rightBlinkCoroutine;
    private string currentLeftCharacter;
    private string currentRightCharacter;

    [SerializeField] private Animations animations;
    private bool isLeftAvatarAnimating = false;
    private bool isRightAvatarAnimating = false;


    // Метод для установки персонажа
    public void SetCharacter(string speaker, int place, bool isNarration, string character)
    {
        Debug.Log("SetCharacter вызван для персонажа: " + speaker + " в позиции: " + place);

        speakerPanelLeft.SetActive(false);
        speakerPanelCenter.SetActive(false);
        speakerPanelRight.SetActive(false);

        GameObject activePanel = null;

        if (place == 1)
        {
            UpdateCharacter(ref currentLeftCharacter, leftAvatar, ref leftBlinkCoroutine, leftEyesImage, character, true);
            activePanel = speakerPanelLeft;
        }
        else if (place == 2)
        {
            UpdateCharacter(ref currentRightCharacter, rightAvatar, ref rightBlinkCoroutine, rightEyesImage, character, false);
            activePanel = speakerPanelRight;
        }
        else if (isNarration)
        {
            activePanel = speakerPanelCenter;
        }
        else
        {
            HideAvatars();
            Debug.LogWarning("Некорректное значение 'place' для SetCharacter: " + place);
            return;
        }

        if (activePanel != null)
        {
            activePanel.SetActive(true);
            TextMeshProUGUI speakerText = activePanel.transform.Find("SpeakerText").GetComponent<TextMeshProUGUI>();
            speakerText.text = isNarration ? "..." : speaker;
        }
    }

    private void UpdateCharacter(ref string currentCharacter, SpriteRenderer avatar, ref Coroutine blinkCoroutine, SpriteRenderer eyesImage, string character, bool isLeft)
    {
        if (currentCharacter != character)
        {
            currentCharacter = character;
            UpdateAvatar(avatar, character, isLeft);

            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);

            blinkCoroutine = StartCoroutine(BlinkCoroutine(eyesImage, character));
        }
    }

    private void UpdateAvatar(SpriteRenderer avatar, string character, bool isLeft)
    {


        if (avatar == null)
        {
            Debug.LogError("Avatar is null!");
            return;
        }

        if (string.IsNullOrEmpty(character))
        {
            Debug.LogWarning("Имя персонажа не указано.");
            StartCoroutine(SmoothDisappear(avatar));
            return;
        }

        Sprite loadedSprite = Resources.Load<Sprite>("Characters/" + character);
        if (loadedSprite == null)
        {
            Debug.LogError($"Спрайт для {character} не найден в Resources/Characters!");
            return;
        }

        avatar.sprite = loadedSprite;

        if (avatar.sprite != null)
        {
            if (avatar.gameObject.activeSelf)
            {
                StartCoroutine(SmoothDisappear(avatar));
                StartCoroutine(WaitAndShowNewAvatar(avatar, character, isLeft));
            }
            else
            {
                float targetX = isLeft ? -3f : 3f;
                StartCoroutine(SmoothAppear(avatar, character, targetX));
            }

            Debug.Log("Установлен спрайт для персонажа: " + character);
        }
        else
        {
            StartCoroutine(SmoothDisappear(avatar));
            Debug.LogWarning("Спрайт не найден для персонажа: " + character);
        }
    }

    private IEnumerator SmoothAppear(SpriteRenderer avatar, string character, float endPositionX)
    {
        while (isLeftAvatarAnimating || isRightAvatarAnimating)
        {
            yield return null;
        }

        isLeftAvatarAnimating = true;

        Vector3 startPosition = avatar.transform.position;
        Vector3 endPosition = new Vector3(endPositionX, avatar.transform.position.y, avatar.transform.position.z);

        avatar.gameObject.SetActive(true);

        // Анимация появления
        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            avatar.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            avatar.color = new Color(avatar.color.r, avatar.color.g, avatar.color.b, Mathf.Lerp(0f, 1f, elapsedTime / duration));

            yield return null;
        }

        avatar.transform.position = endPosition;
        avatar.color = new Color(avatar.color.r, avatar.color.g, avatar.color.b, 1f);

        isLeftAvatarAnimating = false; // или isRightAvatarAnimating в зависимости от того, какой аватар обновляется
    }

    private IEnumerator SmoothDisappear(SpriteRenderer avatar)
    {
        while (isLeftAvatarAnimating || isRightAvatarAnimating)
        {
            yield return null;
        }

        avatar.gameObject.SetActive(false);
        avatar.color = new Color(avatar.color.r, avatar.color.g, avatar.color.b, 0f); // Обнуляем прозрачность на случай повторного использования
    }

    private IEnumerator WaitAndShowNewAvatar(SpriteRenderer avatar, string character, bool isLeft)
    {
        while (isLeftAvatarAnimating || isRightAvatarAnimating)
        {
            yield return null;
        }

        float targetX = isLeft ? -3f : 3f;
        yield return StartCoroutine(SmoothAppear(avatar, character, targetX));
    }

    public void HideAvatars()
    {
        leftAvatar.gameObject.SetActive(false);
        rightAvatar.gameObject.SetActive(false);
        speakerPanelLeft.SetActive(false);
        speakerPanelCenter.SetActive(false);
        speakerPanelRight.SetActive(false);

        if (leftBlinkCoroutine != null)
            StopCoroutine(leftBlinkCoroutine);
        if (rightBlinkCoroutine != null)
            StopCoroutine(rightBlinkCoroutine);

        leftEyesImage.gameObject.SetActive(false);
        rightEyesImage.gameObject.SetActive(false);

        Debug.Log("Скрыты все аватары и эмоции персонажей.");
    }


    private IEnumerator BlinkCoroutine(SpriteRenderer eyesImage, string character)
    {
        if (string.IsNullOrEmpty(character))
        {
           Debug.LogWarning("Character is null or empty. Blink animation will not start.");
            yield break; // Выходим из корутины, если character равен null или пустой строке
        }

        while (true)
        {
            if ((!isLeftAvatarAnimating && !isRightAvatarAnimating) && (!animations.IsLeftAvatarAnimating && !animations.IsRightAvatarAnimating)) // Проверяем, не идет ли анимация
            {
                Sprite closedEyesSprite = Resources.Load<Sprite>("Characters/" + character + "_ClosedEyes");
                if (closedEyesSprite != null)
                {
                    eyesImage.sprite = closedEyesSprite;
                    eyesImage.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"Спрайт закрытых глаз не найден для: {character}");
                }

                // Моргание (короткая пауза)
                yield return new WaitForSeconds(0.2f);

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


