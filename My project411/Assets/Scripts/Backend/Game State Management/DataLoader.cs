using System.Collections.Generic;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public VisualNovelData LoadData(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile != null)
        {
            Debug.Log($"[JSON Debug] ����������� ������: {jsonFile.text}"); 
            VisualNovelData data = JsonUtility.FromJson<VisualNovelData>(jsonFile.text);
            Debug.Log($"[JSON Load] ��������� ��������: {data.episodes.Count}");
            return data;
        }
        else
        {
            Debug.LogError("File JSON hasn't been found.");
            return null;
        }
    }



}


