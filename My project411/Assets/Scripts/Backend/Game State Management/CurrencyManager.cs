using UnityEngine;
using System; // ��������� ������������ ���� ��� Action

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public delegate void KeysChangedHandler(); // ������ ����� �������
    public event KeysChangedHandler OnKeysChanged; // ���������� ��� ��� �������

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
        OnKeysChanged?.Invoke(); // ������ ����� ������� ���������
    }

    public void AddKeys(int amount)
    {
        GameStateManager.Instance.AddKeys(amount);
        OnKeysChanged?.Invoke();
        Debug.Log($"��������� {amount} ������. ������� ����������: {GetKeys()}");
    }

    public bool SpendKeys(int amount)
    {
        if (GameStateManager.Instance.SpendKeys(amount))
        {
            OnKeysChanged?.Invoke();
            Debug.Log($"��������� {amount} ������. ��������: {GetKeys()}");
            return true;
        }
        else
        {
            Debug.Log("������������ ������!");
            return false;
        }
    }
}
