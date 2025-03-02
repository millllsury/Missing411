using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class MoveData
{
    public Transform button; // Ссылка на кнопку
    public float moveDistance = 50f; // Дистанция движения (по умолчанию 50)
}

public class ButtonMover : MonoBehaviour
{
    [SerializeField] private List<MoveData> buttonDataList; // Список кнопок с их дистанцией движения

    private Dictionary<Transform, Vector3> initialPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, bool> buttonStates = new Dictionary<Transform, bool>();
    private Dictionary<Transform, float> buttonMoveDistances = new Dictionary<Transform, float>();

    [SerializeField] private float moveSpeed = 0.2f; // Скорость движения

    private void Start()
    {
        foreach (MoveData data in buttonDataList)
        {
            if (data.button != null)
            {
                initialPositions[data.button] = data.button.localPosition;
                buttonStates[data.button] = false;
                buttonMoveDistances[data.button] = data.moveDistance;
            }
        }
    }

    public void ToggleMove(Transform button)
    {
        if (!buttonStates.ContainsKey(button)) return;

        bool isMoved = buttonStates[button];
        float moveDistance = buttonMoveDistances.ContainsKey(button) ? buttonMoveDistances[button] : 50f;

        StopAllCoroutines();
        if (isMoved)
        {
            StartCoroutine(MoveButton(button, initialPositions[button]));
        }
        else
        {
            StartCoroutine(MoveButton(button, initialPositions[button] + new Vector3(-moveDistance, 0, 0)));
        }

        buttonStates[button] = !isMoved;
    }

    private bool moved = false;
    private IEnumerator MoveButton(Transform button, Vector3 targetPosition)
    {
        if (button.name == "Books")
        {
            moved = !moved;

            if (moved)
            {

                button.transform.SetAsFirstSibling();
            }
            else
            {
                button.transform.SetAsLastSibling();
            }
        }


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


