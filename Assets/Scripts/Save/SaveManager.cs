using UnityEngine;
using System.IO;

public static class SaveManager {
    private static string SavePath => Path.Combine(Application.persistentDataPath, "savedata.json");
    private static GameSaveData _currentData;
    
    public static GameSaveData Current {
        get {
            if (_currentData == null) Load();
            return _currentData;
        }
    }

    public static void Load() {
        if (File.Exists(SavePath)) {
            try {
                string json = File.ReadAllText(SavePath);
                _currentData = JsonUtility.FromJson<GameSaveData>(json);
            } catch (System.Exception e) {
                Debug.LogError($"Save Read Error: {e.Message}");
                _currentData = new GameSaveData();
            }
        } else {
            _currentData = new GameSaveData();
        }
    }

    public static void Save() {
        if (_currentData == null) return;
        try {
            string json = JsonUtility.ToJson(_currentData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Saved data to {SavePath}");
        } catch (System.Exception e) {
            Debug.LogError($"Save Write Error: {e.Message}");
        }
    }
}
