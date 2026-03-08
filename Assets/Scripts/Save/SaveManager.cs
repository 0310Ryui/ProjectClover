using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

        EnsureDefaults();
    }

    public static void Save() {
        if (_currentData == null) return;

        EnsureDefaults();

        try {
            string json = JsonUtility.ToJson(_currentData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Saved data to {SavePath}");
        } catch (System.Exception e) {
            Debug.LogError($"Save Write Error: {e.Message}");
        }
    }

    private static void EnsureDefaults() {
        if (_currentData == null) {
            _currentData = new GameSaveData();
        }

        if (_currentData.player == null) {
            _currentData.player = new PlayerSaveData();
        }

        if (_currentData.player.inventory == null) {
            _currentData.player.inventory = new InventorySaveData();
        }

        if (_currentData.player.unlockedMagics == null) {
            _currentData.player.unlockedMagics = new List<string>();
        }

        if (_currentData.player.taskCategories == null) {
            _currentData.player.taskCategories = new List<TaskCategory>();
        }

        if (_currentData.player.audioSettings == null) {
            _currentData.player.audioSettings = new AudioSettingsSaveData();
        }

        if (!_currentData.player.audioSettings.initialized) {
            _currentData.player.audioSettings.initialized = true;
            _currentData.player.audioSettings.seVolume = 0.5f;
            _currentData.player.audioSettings.bgmVolume = 0.5f;
        }

        if (_currentData.doll == null) {
            _currentData.doll = new DollSaveData();
        }
    }
}
