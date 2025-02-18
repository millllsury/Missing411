using UnityEngine;
using UnityEngine.UI;

public class LockSystem : MonoBehaviour
{
    [SerializeField] private Text[] digitTexts; // ������ ��� 4 ����
    [SerializeField] private int[] currentDigits = new int[4]; // ������� �����
    [SerializeField] private int[] correctCode = { 7, 2, 4, 9 }; // ���������� ���
    [SerializeField] private Button unlockButton; // ������ �������������

    private void Start()
    {
        // ��������� ��������� ����������� ����
        UpdateDigits();
        unlockButton.onClick.AddListener(CheckCode);
    }

    public void ChangeDigit(int index, int change)
    {
        currentDigits[index] = (currentDigits[index] + change) % 10; // ������ ����� (0-9)
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
                Debug.Log("�������� ���!"); // ���� ��� ��������
                return;
            }
        }
        Debug.Log("����� ������!");
        // ��� ����� �������� �������� ��� ���� ��������
    }
}
