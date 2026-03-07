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
public class PlayerSaveData {
    public InventorySaveData inventory = new InventorySaveData();
    public List<string> unlockedMagics = new List<string>();
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
