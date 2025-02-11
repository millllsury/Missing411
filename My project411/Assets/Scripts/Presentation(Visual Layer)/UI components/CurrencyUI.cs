using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keysText;

    private void Start()
    {
        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("CurrencyUI: CurrencyManager.Instance не найден! Убедись, что он есть в сцене.");
        }
        else
        {
            UpdateUI(); // Обновляем UI при старте
        }
    }


    private void OnEnable()
    {
        UpdateUI();

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnKeysChanged += UpdateUI; // Подписка на событие
        }
        else
        {
            Debug.LogError("CurrencyUI: CurrencyManager.Instance равно null!");
        }
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnKeysChanged -= UpdateUI; // Отписка от события
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
            Debug.LogError("CurrencyUI: keysText не назначен в инспекторе!");
        }
    }

}
