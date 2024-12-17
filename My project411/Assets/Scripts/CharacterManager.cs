using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private BlinkingManager blinkingManager;
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

    [SerializeField]  private SpriteRenderer hairRenderer;
    [SerializeField]  private SpriteRenderer clothesRenderer;

    private void Start()
    {
       LoadAppearance();
        //LoadCharacters();
    }

    public void LoadCharacters()
    {
        var (leftCharacter, rightCharacter) = GameStateManager.Instance.LoadCharacterNames();

        Debug.Log($"Загружаем персонажей: Left = {leftCharacter}, Right = {rightCharacter}");

        // Восстанавливаем только если имя персонажа задано
        if (!string.IsNullOrEmpty(leftCharacter))
            SetCharacter(leftCharacter, 1, false, leftCharacter);

        if (!string.IsNullOrEmpty(rightCharacter))
            SetCharacter(rightCharacter, 2, false, rightCharacter);
    }



    public void LoadAppearance()
    {
        var (hairIndex, clothesIndex) = GameStateManager.Instance.LoadAppearance();

        Debug.Log($"Загрузка внешнего вида: HairIndex = {hairIndex}, ClothesIndex = {clothesIndex}");

        // Загружаем спрайты по пути
        string hairPath = $"Characters/Alice/Hair/hair{hairIndex}";
        string clothesPath = $"Characters/Alice/Clothes/clothes{clothesIndex}";

        Sprite hairSprite = Resources.Load<Sprite>(hairPath);
        Sprite clothesSprite = Resources.Load<Sprite>(clothesPath);

        Debug.Log($"Пути к ресурсам: Hair = {hairPath}, Clothes = {clothesPath}");

        if (hairSprite != null)
        {
            hairRenderer.sprite = hairSprite;
            Debug.Log("Спрайт для волос успешно загружен.");
        }
        else
        {
            Debug.LogError($"Спрайт для волос не найден: {hairPath}");
        }

        if (clothesSprite != null)
        {
            clothesRenderer.sprite = clothesSprite;
            Debug.Log("Спрайт для одежды успешно загружен.");
        }
        else
        {
            Debug.LogError($"Спрайт для одежды не найден: {clothesPath}");
        }
    }




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
            // Передаем leftEyesImage и указываем isLeft = true
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
            // Остановить моргание для предыдущего персонажа
            if (!string.IsNullOrEmpty(currentCharacter))
            {
                blinkingManager.StopBlinking(currentCharacter); // Исправленный вызов
            }

            currentCharacter = character;
            UpdateAvatar(avatar, character, isLeft);

            // Запустить моргание для нового персонажа
            blinkingManager.StartBlinking(character, eyesImage);
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

        Sprite loadedSprite = Resources.Load<Sprite>("Characters/" + character + "/" + character);
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

    private void OnDisable()
    {
        // Остановить все моргания при отключении
        blinkingManager.StopAllBlinking();
    }

    public string GetCurrentLeftCharacter()
    {
        return string.IsNullOrEmpty(currentLeftCharacter) ? null : currentLeftCharacter;
    }

    public string GetCurrentRightCharacter()
    {
        return string.IsNullOrEmpty(currentRightCharacter) ? null : currentRightCharacter;
    }


   


}




