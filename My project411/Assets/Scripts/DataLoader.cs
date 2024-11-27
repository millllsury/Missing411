using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public VisualNovelData LoadData(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile != null)
        {
            VisualNovelData data = JsonUtility.FromJson<VisualNovelData>(jsonFile.text);
            Debug.Log("JSON загружен успешно.");
            return data;
        }
        else
        {
            Debug.LogError("Файл JSON не найден.");
            return null;
        }
    }
}


