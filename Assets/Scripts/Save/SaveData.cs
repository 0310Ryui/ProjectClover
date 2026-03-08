using System.Collections.Generic;

[System.Serializable]
public class InventorySaveData {
    public int money;
    public List<string> materialIds = new List<string>();
    public List<int> materialCounts = new List<int>();

    public void AddMaterial(string id, int amount) {
        int index = materialIds.IndexOf(id);
        if (index >= 0) {
            materialCounts[index] += amount;
        } else {
            materialIds.Add(id);
            materialCounts.Add(amount);
        }
    }

    public int GetMaterialCount(string id) {
        int index = materialIds.IndexOf(id);
        return index >= 0 ? materialCounts[index] : 0;
    }
}

[System.Serializable]
public class TaskItem {
    public string taskId;
    public string taskName;
    public bool isCompleted;
    public float timeSpentSeconds;

    public TaskItem(string name) {
        taskId = System.Guid.NewGuid().ToString();
        taskName = name;
        isCompleted = false;
        timeSpentSeconds = 0;
    }
}

[System.Serializable]
public class TaskCategory {
    public string categoryId;
    public string categoryName;
    public List<TaskItem> tasks = new List<TaskItem>();

    public TaskCategory(string name) {
        categoryId = System.Guid.NewGuid().ToString();
        categoryName = name;
    }
}

[System.Serializable]
public class AudioSettingsSaveData {
    public bool initialized;
    public float seVolume = 0.5f;
    public float bgmVolume = 0.5f;
}

[System.Serializable]
public class PlayerSaveData {
    public InventorySaveData inventory = new InventorySaveData();
    public List<string> unlockedMagics = new List<string>();
    public AudioSettingsSaveData audioSettings = new AudioSettingsSaveData();
    public List<TaskCategory> taskCategories = new List<TaskCategory>();
    public string selectedCategoryId;
    public string selectedTaskId;
}

[System.Serializable]
public class DollSaveData {
    public string dollName = "My Doll";
    public int currentMood;
}

[System.Serializable]
public class GameSaveData {
    public PlayerSaveData player = new PlayerSaveData();
    public DollSaveData doll = new DollSaveData();
}
