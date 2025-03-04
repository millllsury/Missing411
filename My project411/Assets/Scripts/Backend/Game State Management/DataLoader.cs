using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public VisualNovelData LoadData(int episodeId)
    {
        string fileName = $"Episode_{episodeId}";
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);

        if (jsonFile == null)
        {
            Debug.LogError($"[LoadData] ������: ���� {fileName}.json �� ������ � Resources!");
            return null;
        }

        Debug.Log($"[JSON Debug] ����������� ������ �� {fileName}: {jsonFile.text}");

        try
        {
            var data = JsonConvert.DeserializeObject<VisualNovelData>(jsonFile.text);
            Debug.Log($"[JSON Load] �������� ������ {episodeId}, ����: {data.episodes[0].scenes.Count}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LoadData] ������ �������� JSON: {e.Message}");
            return null;
        }
    }



}



