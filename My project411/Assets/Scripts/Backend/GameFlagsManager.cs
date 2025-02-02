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
            Debug.LogWarning("������� null ������ ������� ������. ������ ������ �������.");
            flags = new Dictionary<string, bool>();
            return;
        }

        flags = new Dictionary<string, bool>(newFlags);
    }



    // ��������� �������� �����
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

        Debug.Log($"���� ����������: {key} = {value}");
    }

    // ��������� ���������� ���� �������
    public bool AreConditionsMet(List<Condition> conditions)
    {
        if (conditions == null || conditions.Count == 0)
        {
            return true; // ���� ������� ���, ��� ��������� ������������
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




