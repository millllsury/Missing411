using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonMover : MonoBehaviour
{
    [SerializeField] private List<Transform> buttonsToMove; // Список кнопок

    private Dictionary<Transform, Vector3> initialPositions = new Dictionary<Transform, Vector3>(); // Запоминаем исходные позиции кнопок
    private Dictionary<Transform, bool> buttonStates = new Dictionary<Transform, bool>(); // Хранит состояния кнопок (сдвинута/нет)
    private float moveDistance = 50f; // Насколько двигаем кнопку влево
    private float moveSpeed = 0.2f; // Скорость движения

    private void Start()
    {
        foreach (Transform button in buttonsToMove)
        {
            if (button != null)
            {
                initialPositions[button] = button.localPosition; // Запоминаем исходную позицию
                buttonStates[button] = false; // Все кнопки изначально не сдвинуты
            }
        }
    }

    public void ToggleMove(Transform button)
    {
        if (!buttonStates.ContainsKey(button)) return;

        bool isMoved = buttonStates[button]; // Получаем текущее состояние кнопки

        StopAllCoroutines();
        if (isMoved)
        {
            StartCoroutine(MoveButton(button, initialPositions[button])); // Возвращаем в исходное положение
        }
        else
        {
            StartCoroutine(MoveButton(button, initialPositions[button] + new Vector3(-moveDistance, 0, 0))); // Сдвигаем влево
        }

        buttonStates[button] = !isMoved; // Инвертируем состояние
    }

    private IEnumerator MoveButton(Transform button, Vector3 targetPosition)
    {
        Vector3 startPosition = button.localPosition;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / moveSpeed;
            button.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        button.localPosition = targetPosition;
    }
}

