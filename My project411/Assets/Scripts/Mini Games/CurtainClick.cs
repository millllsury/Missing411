using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CurtainClick : MonoBehaviour
{
    [SerializeField] private CanvasGroup windowCanvas; // Canvas ����
    [SerializeField] private Image wolfImage; // ���� (UI Image)
    [SerializeField] private Image curtainOpenImage; // �������� ���������
    [SerializeField] private Image curtainClosedImage; // �������� ���������
    [SerializeField] private float fadeDuration = 0.2f; // ������������ ���������
    [SerializeField] private float curtainCloseDelay = 2f; // �������� ����� ��������� ���������
    [SerializeField] private Animator wolfnAnimator;
    [SerializeField] private Button closeWindow;

    public bool scenePlayed = false;
    public void CurtainOnClick()
    {
        StartCoroutine(ShowWindowScene());
    }


    private IEnumerator ShowWindowScene()
    {
        // �������� Canvas � ��������� �����
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
        // ��� ����� ��������� ���������
        yield return new WaitForSeconds(curtainCloseDelay);

        wolfnAnimator.SetTrigger("Run");
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(FadeOutImage(curtainClosedImage));
        StartCoroutine(FadeInImage(curtainOpenImage));

        yield return new WaitForSeconds(0.5f); // ���� �������� ���� �����
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