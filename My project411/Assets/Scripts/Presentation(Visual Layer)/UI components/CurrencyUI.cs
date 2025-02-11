using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keysText;

    private void Start()
    {
        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("CurrencyUI: CurrencyManager.Instance �� ������! �������, ��� �� ���� � �����.");
        }
        else
        {
            UpdateUI(); // ��������� UI ��� ������
        }
    }


    private void OnEnable()
    {
        UpdateUI();

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnKeysChanged += UpdateUI; // �������� �� �������
        }
        else
        {
            Debug.LogError("CurrencyUI: CurrencyManager.Instance ����� null!");
        }
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnKeysChanged -= UpdateUI; // ������� �� �������
        }
    }


    private void UpdateUI()
    {
        if (keysText != null)
        {
            keysText.text = $"{CurrencyManager.Instance.GetKeys()}";
        }
        else
        {
            Debug.LogError("CurrencyUI: keysText �� �������� � ����������!");
        }
    }

}
