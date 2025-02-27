using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class ButtonMover : MonoBehaviour
{
    [SerializeField] private List<Transform> buttonsToMove; // Список кнопок

    private Dictionary<Transform, Vector3> initialPositions = new Dictionary<Transform, Vector3>(); 
    private Dictionary<Transform, bool> buttonStates = new Dictionary<Transform, bool>(); 
    [SerializeField] private float moveDistance = 50f;
    [SerializeField] private float moveSpeed = 0.2f;

    private void Start()
    {
        foreach (Transform button in buttonsToMove)
        {
            if (button != null)
            {
                initialPositions[button] = button.localPosition; 
                buttonStates[button] = false; 
            }
        }
    }

    public void ToggleMove(Transform button)
    {
        if (!buttonStates.ContainsKey(button)) return;

        bool isMoved = buttonStates[button]; 

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

