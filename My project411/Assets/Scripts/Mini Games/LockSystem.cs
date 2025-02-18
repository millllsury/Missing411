using UnityEngine;
using UnityEngine.UI;

public class LockSystem : MonoBehaviour
{
    [SerializeField] private Text[] digitTexts; // Тексты для 4 цифр
    [SerializeField] private int[] currentDigits = new int[4]; // Текущие числа
    [SerializeField] private int[] correctCode = { 7, 2, 4, 9 }; // Правильный код
    [SerializeField] private Button unlockButton; // Кнопка разблокировки

    private void Start()
    {
        // Обновляем начальное отображение цифр
        UpdateDigits();
        unlockButton.onClick.AddListener(CheckCode);
    }

    public void ChangeDigit(int index, int change)
    {
        currentDigits[index] = (currentDigits[index] + change) % 10; // Меняем цифру (0-9)
        if (currentDigits[index] < 0) currentDigits[index] = 9;
        UpdateDigits();
    }

    private void UpdateDigits()
    {
        for (int i = 0; i < digitTexts.Length; i++)
        {
            digitTexts[i].text = currentDigits[i].ToString();
        }
    }

    private void CheckCode()
    {
        for (int i = 0; i < correctCode.Length; i++)
        {
            if (currentDigits[i] != correctCode[i])
            {
                Debug.Log("Неверный код!"); // Если код неверный
                return;
            }
        }
        Debug.Log("Замок открыт!");
        // Тут можно добавить анимацию или звук открытия
    }
}
