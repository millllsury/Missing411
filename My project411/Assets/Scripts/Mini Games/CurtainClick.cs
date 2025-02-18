using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CurtainClick : MonoBehaviour
{
    [SerializeField] private CanvasGroup windowCanvas; // Canvas окна
    [SerializeField] private Image wolfImage; // Волк (UI Image)
    [SerializeField] private Image curtainOpenImage; // Открытые занавески
    [SerializeField] private Image curtainClosedImage; // Закрытые занавески
    [SerializeField] private float fadeDuration = 0.2f; // Длительность появления
    [SerializeField] private float curtainCloseDelay = 2f; // Задержка перед закрытием занавесок
    [SerializeField] private Animator wolfnAnimator;
    [SerializeField] private Button closeWindow;

    public bool scenePlayed = false;
    public void CurtainOnClick()
    {
        StartCoroutine(ShowWindowScene());
    }


    private IEnumerator ShowWindowScene()
    {
        // Включаем Canvas и затемняем сцену
        windowCanvas.gameObject.SetActive(true);
        yield return StartCoroutine(FadeCanvas(windowCanvas, 0f, 1f, fadeDuration));

        if(scenePlayed )
        {
            yield return null;
        }

        yield return StartCoroutine(ShowWolfScene());

    }

    private IEnumerator ShowWolfScene()
    {
        // Ждём перед закрытием занавесок
        yield return new WaitForSeconds(curtainCloseDelay);

        wolfnAnimator.SetTrigger("Run");
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(FadeOutImage(curtainClosedImage));
        StartCoroutine(FadeInImage(curtainOpenImage));

        yield return new WaitForSeconds(0.5f); // Волк исчезает чуть позже
        yield return StartCoroutine(FadeOutImage(wolfImage));
        scenePlayed = true;
        curtainClosedImage.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        closeWindow.gameObject.SetActive(true);
    }

    public void CloseWindow()
    {
        windowCanvas.gameObject.SetActive(false);
    }

    private IEnumerator FadeCanvas(CanvasGroup canvas, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }
        canvas.alpha = endAlpha;
    }

   private IEnumerator FadeInImage(Image image)
    {
        float elapsedTime = 0;
        Color color = image.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            image.color = color;
            yield return null;
        }
        image.color = color;
    }

    private IEnumerator FadeOutImage(Image image)
    {
        float elapsedTime = 0;
        Color color = image.color;
        wolfImage.gameObject.SetActive(false);
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            image.color = color;
            yield return null;
        }

        color.a = 0;
        image.color = color;
    }

}