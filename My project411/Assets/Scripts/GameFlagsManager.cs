using System.Collections.Generic;
using UnityEngine;

public class GameFlagsManager : MonoBehaviour
{
    private Dictionary<string, bool> flags = new Dictionary<string, bool>();

    // ������������� �������� �����
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

    // ��������� �������� �����
    public bool GetFlag(string key)
    {
        return flags.ContainsKey(key) && flags[key];
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




