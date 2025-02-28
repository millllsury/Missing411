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

    [SerializeField] private GameObject characterLeft;
    [SerializeField] private GameObject characterRight;

    private Coroutine leftBlinkCoroutine;
    private Coroutine rightBlinkCoroutine;

    private string currentLeftCharacter;
    private string currentRightCharacter;

    [SerializeField] private Animations animations;
    private bool isLeftAvatarAnimating = false;
    private bool isRightAvatarAnimating = false;

    [SerializeField] private SpriteRenderer hairRenderer;
    [SerializeField] private SpriteRenderer clothesRenderer;
    private SpriteRenderer avatarToBeHidden;


    private void Start()
    {
        
    }

    public void LoadCharacters()
    {
        var (leftCharacter, rightCharacter) = GameStateManager.Instance.LoadCharacterNames();

        Debug.Log($"Загрузка персонажей: Левый = {leftCharacter}, Правый = {rightCharacter}");

        if (!string.IsNullOrEmpty(leftCharacter))
        {
            SetCharacter(leftCharacter, 1, false, leftCharacter);
            LoadAppearance();
        }
        else
        {
            leftAvatar.sprite = null; // Удаляем спрайт, если персонаж удалён
        }

        if (!string.IsNullOrEmpty(rightCharacter))
        {
            SetCharacter(rightCharacter, 2, false, rightCharacter);
        }
        else
        {
            rightAvatar.sprite = null; // Удаляем спрайт, если персонаж удалён
        }
        

    }




    public void LoadAppearance()
    {
        var (hairIndex, clothesIndex) = GameStateManager.Instance.LoadAppearance();



        // Загружаем спрайты по пути
        string hairPath = $"Characters/Alice/Hair/hair{hairIndex}";
        string clothesPath = $"Characters/Alice/Clothes/clothes{clothesIndex}";

        Sprite hairSprite = Resources.Load<Sprite>(hairPath);
        Sprite clothesSprite = Resources.Load<Sprite>(clothesPath);

        if (hairSprite != null)
        {
            hairRenderer.sprite = hairSprite;

        }
        else
        {
            Debug.LogError($"Спрайт для волос не найден: {hairPath}");
        }

        if (clothesSprite != null)
        {
            clothesRenderer.sprite = clothesSprite;

        }
        else
        {
            Debug.LogError($"Спрайт для одежды не найден: {clothesPath}");
        }
    }




    // Метод для установки персонажа
    public void SetCharacter(string speaker, int place, bool isNarration, string character)
    {
        //Debug.Log("SetCharacter вызван для персонажа: " + speaker + " в позиции: " + place);

        speakerPanelLeft.SetActive(false);
        speakerPanelCenter.SetActive(false);
        speakerPanelRight.SetActive(false);

        GameObject activePanel = null;

        if (place == 1)
        {
            // Передаем leftEyesImage и указываем isLeft = true
            UpdateCharacter(ref currentLeftCharacter, leftAvatar, ref leftBlinkCoroutine, leftEyesImage, character, true);
            activePanel = speakerPanelLeft;
            GetCurrentLeftCharacter();
            LoadAppearance();

        }
        else if (place == 2)
        {

            UpdateCharacter(ref currentRightCharacter, rightAvatar, ref rightBlinkCoroutine, rightEyesImage, character, false);
            activePanel = speakerPanelRight;
            GetCurrentRightCharacter();

        }
        else if (isNarration)
        {
            activePanel = speakerPanelCenter;
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
            if (!string.IsNullOrEmpty(currentCharacter))
            {
                blinkingManager.StopBlinking(currentCharacter);
            }

            currentCharacter = character;
            UpdateAvatar(avatar, character, isLeft);

            blinkingManager.StartBlinking(currentCharacter, eyesImage);

            Debug.Log($"Starting blinking for character: {currentCharacter} ");
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
        Debug.LogWarning("Name of character is not initialised.");
        StartCoroutine(SmoothDisappear(avatar, false));
        return;
    }

    Sprite loadedSprite = Resources.Load<Sprite>("Characters/" + character + "/" + character);
    if (loadedSprite == null)
    {
        Debug.LogError($"Sprite for {character} hasn't been found in Resources/Characters!");
        return;
    }

    // Проверяем, был ли персонаж уже установлен
    bool isNewCharacter = avatar.sprite == null || avatar.sprite.name != character;
    
    avatar.sprite = loadedSprite;

    if (isNewCharacter)
    {
        float targetX = isLeft ? -3f : 3f;
        StartCoroutine(SmoothAppear(avatar, targetX, character));
    }
    else
    {
        StartCoroutine(SmoothDisappear(avatar, false));
        StartCoroutine(WaitAndShowNewAvatar(avatar, character, isLeft));
    }

    Debug.Log($"Sprite for the character was set: {character}, isNew: {isNewCharacter}");
}



    public void AdjustCharacterAppearance(string sceneName)
    {
        float scaleFactor = 1f; // Значение по умолчанию
        float brightness = 1f;  // Яркость (1 = обычное значение)


        switch (sceneName)
        {

            case "WardrobeScene":
                scaleFactor = 1.2f;
                break;
            case "Scene2":
                brightness = 0.8f; // Добавим холодных тонов
                break;
            default:

                break;
        }

        if (leftAvatar != null)
        {
            leftAvatar.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
            ApplyColorAdjustment(characterLeft, brightness);
        }

        // Применяем изменения к правому персонажу
        if (rightAvatar != null)
        {
            rightAvatar.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
            ApplyColorAdjustment(characterRight, brightness);
        }
    }

    private void ApplyColorAdjustment(GameObject character, float brightness)
    {
        // Получаем все SpriteRenderer внутри персонажа
        SpriteRenderer[] spriteRenderers = character.GetComponentsInChildren<SpriteRenderer>(true); // true это значит включая все отключенные объекты!!!

        if (spriteRenderers.Length == 0)
        {
            Debug.LogError($"❌ SpriteRenderer не найден у {character.name} или его дочерних объектов!");
            return;
        }

        foreach (var spriteRenderer in spriteRenderers)
        {
            // Получаем текущий цвет
            Color originalColor = spriteRenderer.color;

            // Рассчитываем новую яркость
            float newRed = Mathf.Clamp(originalColor.r * brightness, 0, 1);
            float newGreen = Mathf.Clamp(originalColor.g * brightness, 0, 1);
            float newBlue = Mathf.Clamp(originalColor.b * brightness, 0, 1);

            // Применяем изменения к цвету спрайта
            spriteRenderer.color = new Color(newRed, newGreen, newBlue, originalColor.a);
        }

        Debug.Log($" Изменения применены ко всем SpriteRenderer персонажа {character.name}");
    }



    private IEnumerator SmoothAppear(SpriteRenderer avatar, float endPositionX, string character = null)
    {
        
        while (isLeftAvatarAnimating || isRightAvatarAnimating)
        {
            yield return null;
        }

        isLeftAvatarAnimating = true;

        Vector3 startPosition = avatar.transform.position;
        Vector3 endPosition = new Vector3(endPositionX, avatar.transform.position.y, avatar.transform.position.z);


        // Анимация появления
        float duration = 0.1f;
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
        foreach (SpriteRenderer childSprite in avatar.GetComponentsInChildren<SpriteRenderer>())
        {
            childSprite.color = new Color(avatar.color.r, avatar.color.g, avatar.color.b, 1f);
        }

        isLeftAvatarAnimating = false; // или isRightAvatarAnimating в зависимости от того, какой аватар обновляется
    }



    public void SmoothDisappearCharacter(bool smoothDisappear, int place)
    {
        if (place == 1)
        {
            avatarToBeHidden = leftAvatar;
            currentLeftCharacter = null; // Очищаем данные о левом персонаже
            GameStateManager.Instance.SaveCharacterNames(null, GetCurrentRightCharacter());
        }
        else
        {
            avatarToBeHidden = rightAvatar;
            currentRightCharacter = null; // Очищаем данные о правом персонаже
            GameStateManager.Instance.SaveCharacterNames(GetCurrentLeftCharacter(), null);
        }

        StartCoroutine(SmoothDisappear(avatarToBeHidden, smoothDisappear));

        Debug.Log($"После удаления: GetCurrentLeftCharacter(): {GetCurrentLeftCharacter()}, GetCurrentRightCharacter(): {GetCurrentRightCharacter()}");
    }



    private IEnumerator SmoothDisappear(SpriteRenderer avatar, bool smoothDisappear)
    {
        if (avatar == null) yield break;

        while (isLeftAvatarAnimating || isRightAvatarAnimating)
        {
            yield return null;
        }

        float duration = 0.1f; // Время анимации исчезновения
        float elapsedTime = 0f;
        Vector3 startPosition = avatar.transform.position;
        Vector3 endPosition = startPosition; // По умолчанию остается на месте

        // Если нужно смещение, задаем новую позицию
        if (smoothDisappear)
        {
            float targetX = startPosition.x > 0 ? 7f : -7f; // Уход вправо или влево
            endPosition = new Vector3(targetX, startPosition.y, startPosition.z);
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            avatar.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            avatar.color = new Color(avatar.color.r, avatar.color.g, avatar.color.b, Mathf.Lerp(1f, 0f, elapsedTime / duration));
            yield return null;
        }

        avatar.color = new Color(avatar.color.r, avatar.color.g, avatar.color.b, 0f);
        avatar.sprite = null;
        foreach (SpriteRenderer childSprite in avatar.GetComponentsInChildren<SpriteRenderer>())
        {
            childSprite.sprite = null;

        }

        // Если было смещение, возвращаем персонажа в исходную позицию
        if (smoothDisappear)
        {
            avatar.transform.position = startPosition;
        }


        Debug.Log($"GetCurrentLeftCharacter(): {GetCurrentLeftCharacter()},GetCurrentRightCharacter(): {GetCurrentRightCharacter()}");
       
    }





    private IEnumerator WaitAndShowNewAvatar(SpriteRenderer avatar, string character, bool isLeft)
    {
        while (isLeftAvatarAnimating || isRightAvatarAnimating)
        {
            yield return null;
        }

        float targetX = isLeft ? -3f : 3f;
        yield return StartCoroutine(SmoothAppear(avatar, targetX, character));
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

        Debug.Log("All avatars and emotions were hidden.");
    }

    private void OnDisable()
    {
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

    public IEnumerator FadeOutCharacters(Transform charactersParent, float duration = 0.3f)
    {
        if (blinkingManager != null)
        {
            blinkingManager.StopAllBlinking();
        }

        SpriteRenderer[] characters = charactersParent.GetComponentsInChildren<SpriteRenderer>();
        float elapsedTime = 0f;


        while (elapsedTime < duration)
        {
            foreach (var character in characters)
            {
                if (character != null)
                {
                    Color color = character.color;
                    color.a = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                    character.color = color;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator FadeInCharacters(Transform charactersParent, float duration = 0.2f)
    {
        SpriteRenderer[] characters = charactersParent.GetComponentsInChildren<SpriteRenderer>();
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            foreach (var character in characters)
            {
                if (character != null && character.sprite != null) // Проверяем, есть ли спрайт
                {
                    Color color = character.color;
                    color.a = Mathf.Lerp(0f, 1f, elapsedTime / duration);
                    character.color = color;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"После FadeInCharacters: Left = {currentLeftCharacter}, Right = {currentRightCharacter}");

        // Проверяем перед морганием
        if (blinkingManager != null)
        {
            if (!string.IsNullOrEmpty(currentLeftCharacter) && leftEyesImage != null)
            {
                blinkingManager.StartBlinking(currentLeftCharacter, leftEyesImage);
            }

            if (!string.IsNullOrEmpty(currentRightCharacter) && rightEyesImage != null)
            {
                blinkingManager.StartBlinking(currentRightCharacter, rightEyesImage);
            }
        }
    }




}




