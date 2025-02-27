using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FrameInteraction : MonoBehaviour
{
    [SerializeField] private RectTransform frameTransform;
    [SerializeField] private Image frameImage;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private Vector3 zoomPosition = new Vector3(0, 0, 0);
    [SerializeField] private float zoomScale = 1.5f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private Button rotateButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Image panel;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private bool isZoomed = false;
    private bool isFrontSide = true;
    private Sprite originalSprite;

    private void Start()
    {
        originalPosition = frameTransform.anchoredPosition;
        originalScale = frameTransform.localScale;
        originalRotation = frameTransform.rotation;
        originalSprite = frameImage.sprite; // Сохраняем изначальный спрайт

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
            StartCoroutine(MoveFrame(zoomPosition, Vector3.one * zoomScale, Quaternion.identity));
            panel.gameObject.SetActive(true);
        }
    }

    private void ToggleFrameSide()
    {
        isFrontSide = !isFrontSide;
        frameImage.sprite = isFrontSide ? originalSprite : backSprite; // Переключаем спрайт
    }

    private void CloseFrame()
    {
        isZoomed = false;
        rotateButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
        StopAllCoroutines();
        StartCoroutine(MoveFrame(originalPosition, originalScale, originalRotation));
        panel.gameObject.SetActive(false);
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
}



