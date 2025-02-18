using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FrameInteraction : MonoBehaviour
{
    [SerializeField] private RectTransform frameTransform; // Рамка (UI Image)
    [SerializeField] private Image frameBase; // Пустая рамка (без фото)
    [SerializeField] private Image frameImage; // Фото (исчезает при приближении)
    [SerializeField] private Sprite frameWithoutPhoto; // Пустая рамка (без фото)
    [SerializeField] private Sprite backSprite; // Задняя сторона рамки
    [SerializeField] private Vector3 zoomPosition = new Vector3(0, 0, 0); // Позиция при осмотре
    [SerializeField] private float zoomScale = 1.5f; // Во сколько раз увеличивается рамка
    [SerializeField] private float moveDuration = 0.5f; // Время анимации перемещения
    [SerializeField] private Button rotateButton; // Кнопка поворота
    [SerializeField] private Button closeButton; // Кнопка закрытия

    [SerializeField] private Image panel;


    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation; // Исходный поворот
    private bool isZoomed = false;
    private bool isFrontSide = true;
    private bool isPhotoRemoved = false; // Удалено ли фото?

    private void Start()
    {
        originalPosition = frameTransform.anchoredPosition;
        originalScale = frameTransform.localScale;
        originalRotation = frameTransform.rotation; // Сохраняем исходный поворот

        rotateButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);

        rotateButton.onClick.AddListener(ToggleFrameSide);
        closeButton.onClick.AddListener(CloseFrame);
    }

    public void OnFrameClick()
    {

        if (!isZoomed)
        {
            isZoomed = true;
            rotateButton.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(MoveFrame(zoomPosition, Vector3.one * zoomScale, Quaternion.identity)); // Обнуляем поворот
            panel.gameObject.SetActive(true);

            if (!isPhotoRemoved) // Если фото еще не убиралось
            {
                StartCoroutine(FadeOutAndDisable(frameImage)); // Фото исчезает и отключается навсегда
                isPhotoRemoved = true;
            }
        }
    }

    private void ToggleFrameSide()
    {
        isFrontSide = !isFrontSide;
        frameBase.sprite = isFrontSide ? frameWithoutPhoto : backSprite; // Меняем спрайт (фото уже не вернётся)
    }

    private void CloseFrame()
    {
        isZoomed = false;
        rotateButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
        StopAllCoroutines();
        StartCoroutine(MoveFrame(originalPosition, originalScale, originalRotation)); // Возвращаем поворот
        panel.gameObject.SetActive(false) ;
    }

    private IEnumerator MoveFrame(Vector3 targetPosition, Vector3 targetScale, Quaternion targetRotation)
    {
        float elapsedTime = 0;
        Vector3 startPosition = frameTransform.anchoredPosition;
        Vector3 startScale = frameTransform.localScale;
        Quaternion startRotation = frameTransform.rotation;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            frameTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            frameTransform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / moveDuration);
            frameTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / moveDuration);
            yield return null;
        }

        frameTransform.anchoredPosition = targetPosition;
        frameTransform.localScale = targetScale;
        frameTransform.rotation = targetRotation;
    }

    private IEnumerator FadeOutAndDisable(Image img)
    {
        float fadeDuration = 0.5f;
        float elapsedTime = 0;
        Color color = img.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            img.color = color;
            yield return null;
        }

        img.gameObject.SetActive(false); // Полностью убираем фото
    }
}

