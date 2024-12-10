


using System.IO;
using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    private static string saveFilePath = Application.persistentDataPath + "/game_save.json";

    // ���������� ��������� ����
    public static void SaveGame(GameProgress progress)
    {
        string json = JsonUtility.ToJson(progress, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game saved to: " + saveFilePath);
    }

    // �������� ��������� ����
    public static GameProgress LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            GameProgress progress = JsonUtility.FromJson<GameProgress>(json);
            Debug.Log("Game loaded from: " + saveFilePath);
            return progress;
        }
        else
        {
            Debug.LogWarning("Save file not found!");
            return null;
        }
    }
}

