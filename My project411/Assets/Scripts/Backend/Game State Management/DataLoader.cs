using System.Collections.Generic;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public VisualNovelData LoadData(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile != null)
        {
            VisualNovelData data = JsonUtility.FromJson<VisualNovelData>(jsonFile.text);
            Debug.Log("JSON was loaded successfully.");
            return data;
        }
        else
        {
            Debug.LogError("File JSON hasn't been found.");
            return null;
        }
    }


}


