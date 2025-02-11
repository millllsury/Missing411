using UnityEngine;
using System; // Добавляем пространство имен для Action

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public delegate void KeysChangedHandler(); // Создаём явный делегат
    public event KeysChangedHandler OnKeysChanged; // Используем его для события

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }



    public int GetKeys() => GameStateManager.Instance.GetKeys();

    public void SetKeys(int amount)
    {
        GameStateManager.Instance.SetKeys(amount);
        OnKeysChanged?.Invoke(); // Теперь вызов события корректен
    }

    public void AddKeys(int amount)
    {
        GameStateManager.Instance.AddKeys(amount);
        OnKeysChanged?.Invoke();
        Debug.Log($"Добавлено {amount} ключей. Текущее количество: {GetKeys()}");
    }

    public bool SpendKeys(int amount)
    {
        if (GameStateManager.Instance.SpendKeys(amount))
        {
            OnKeysChanged?.Invoke();
            Debug.Log($"Потрачено {amount} ключей. Осталось: {GetKeys()}");
            return true;
        }
        else
        {
            Debug.Log("Недостаточно ключей!");
            return false;
        }
    }
}
