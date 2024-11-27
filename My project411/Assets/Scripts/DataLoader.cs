using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public VisualNovelData LoadData(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile != null)
        {
            VisualNovelData data = JsonUtility.FromJson<VisualNovelData>(jsonFile.text);
            Debug.Log("JSON �������� �������.");
            return data;
        }
        else
        {
            Debug.LogError("���� JSON �� ������.");
            return null;
        }
    }
}


