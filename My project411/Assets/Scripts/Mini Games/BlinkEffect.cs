using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlinkEffect : MonoBehaviour
{
    private Image image;
    private bool isBlinking = false;

    void Start()
    {
        image = GetComponent<Image>();
        StartBlinking();
    }

    public void StartBlinking()
    {
        if (!isBlinking)
        {
            isBlinking = true;
            StartCoroutine(Blink());
        }
    }

    public void StopBlinking()
    {
        isBlinking = false;
        Color color = image.color;
        color.a = 1f; // Делаем полностью видимой
        image.color = color;
    }

    private IEnumerator Blink()
    {
        while (isBlinking)
        {
            Color color = image.color;
            color.a = (color.a == 1f) ? 0.5f : 1f;
            image.color = color;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
