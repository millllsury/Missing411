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
            Debug.LogError($"[LoadData] Ошибка: Файл {fileName}.json не найден в Resources!");
            return null;
        }

        Debug.Log($"[JSON Debug] Загруженные данные из {fileName}: {jsonFile.text}");

        try
        {
            var data = JsonConvert.DeserializeObject<VisualNovelData>(jsonFile.text);
            Debug.Log($"[JSON Load] Загружен эпизод {episodeId}, сцен: {data.episodes[0].scenes.Count}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LoadData] Ошибка парсинга JSON: {e.Message}");
            return null;
        }
    }



}



