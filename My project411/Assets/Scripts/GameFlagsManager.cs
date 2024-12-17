using System.Collections.Generic;
using UnityEngine;

public class GameFlagsManager : MonoBehaviour
{
    private Dictionary<string, bool> flags = new Dictionary<string, bool>();

    public Dictionary<string, bool> GetAllFlags()
    {
        return new Dictionary<string, bool>(flags);
    }

    public void SetAllFlags(Dictionary<string, bool> newFlags)
    {
        if (newFlags == null)
        {
            Debug.LogWarning("Передан null вместо словаря флагов. Создаю пустой словарь.");
            flags = new Dictionary<string, bool>();
            return;
        }

        flags = new Dictionary<string, bool>(newFlags);
    }



    // Проверяем значение флага
    public bool GetFlag(string key)
    {
        return flags.ContainsKey(key) && flags[key];
    }

    public void SetFlag(string key, bool value)
    {
        if (flags.ContainsKey(key))
        {
            flags[key] = value;
        }
        else
        {
            flags.Add(key, value);
        }

        Debug.Log($"Флаг установлен: {key} = {value}");
    }

    // Проверяем выполнение всех условий
    public bool AreConditionsMet(List<Condition> conditions)
    {
        if (conditions == null || conditions.Count == 0)
        {
            return true; // Если условий нет, они считаются выполненными
        }

        foreach (var condition in conditions)
        {
            if (!GetFlag(condition.key) == condition.value)
            {
                return false;
            }
        }
        return true;
    }
}




