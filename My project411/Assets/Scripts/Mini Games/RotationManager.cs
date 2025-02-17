using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RotationManager : MonoBehaviour
{
    [SerializeField] private List<RectTransform> buttonsToRotate; // Список кнопок

    private Dictionary<RectTransform, bool> rotationStates = new Dictionary<RectTransform, bool>(); // Запоминаем состояние
    private float rotationAngle = -30f; // Насколько градусов поворачивать
    private float rotationSpeed = 0.2f; // Скорость поворота

    private void Start()
    {
        foreach (RectTransform button in buttonsToRotate)
        {
            if (button != null)
            {
                rotationStates[button] = false; // Все кнопки изначально не повернуты
            }
        }
    }

    public void ToggleRotation(GameObject buttonObject)
    {
        RectTransform button = buttonObject.GetComponent<RectTransform>();
        if (button == null || !rotationStates.ContainsKey(button)) return;

        bool isRotated = rotationStates[button]; // Получаем текущее состояние

        StopAllCoroutines();
        StartCoroutine(RotateSmoothly(button, isRotated ? 0f : rotationAngle));

        rotationStates[button] = !isRotated; // Инвертируем состояние
    }

    private IEnumerator RotateSmoothly(RectTransform button, float targetZRotation)
    {
        Quaternion startRotation = button.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZRotation);
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / rotationSpeed;
            button.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        button.localRotation = targetRotation;
    }
}
