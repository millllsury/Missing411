using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CanvasGroup[] targetImages;
    [SerializeField] private float fadeDuration = 0.5f; // Время для плавного перехода

    private void Start()
    {
        foreach (var canvasGroup in targetImages)
        {
            canvasGroup.alpha = 0f;  // Скрываем все элементы в начале
            canvasGroup.gameObject.SetActive(true);  // Убедитесь, что объекты активны
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var image in targetImages)
        {
            StartCoroutine(FadeIn(image));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (var image in targetImages)
        {
            StartCoroutine(FadeOut(image));
        }
    }

    private IEnumerator FadeIn(CanvasGroup canvasGroup)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut(CanvasGroup canvasGroup)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}
