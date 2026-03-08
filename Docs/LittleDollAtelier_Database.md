# Little Doll Atelier: Database & Data Structure Design

This document outlines the concrete database definitions for the MVP, structured as Unity `ScriptableObject` classes for static master data and `JSON` structures for dynamic save data.

## 1. Master Data (ScriptableObjects)

These classes represent immutable game data created and configured in the Unity Editor.

### 1.1 `ItemData` (Base Class)
A base class for anything that can be stored in an inventory or shop.
```csharp
public abstract class ItemData : ScriptableObject
{
    public string id;           // Unique identifier (e.g., "mat_wood_01")
    public string displayName;  // e.g., "Enchanted Wood"
    public Sprite icon;
    [TextArea] public string description;
    public int baseValue;       // Sell/Buy price basis
}
```

### 1.2 `MaterialData` (Inherits ItemData)
Crafting ingredients earned from Pomodoro sessions.
```csharp
[CreateAssetMenu(fileName = "NewMaterial", menuName = "Database/Material")]
public class MaterialData : ItemData
{
    public enum Rarity { Common, Rare, Epic }
    public Rarity rarity;
    // Potentially adds a specific mood bias when used in crafting
}
```

### 1.3 `MusicData` (Inherits ItemData)
Unlockable music tracks.
```csharp
[CreateAssetMenu(fileName = "NewMusic", menuName = "Database/Music")]
public class MusicData : ItemData
{
    public AudioClip audioClip;
    public string composerName; // e.g., "Pachelbel"
}
```

### 1.4 `CosmeticData` (Inherits ItemData)
Outfits, accessories, or furniture.
```csharp
[CreateAssetMenu(fileName = "NewCosmetic", menuName = "Database/Cosmetic")]
public class CosmeticData : ItemData
{
    public enum CosmeticType { Outfit, Accessory, Furniture }
    public CosmeticType type;
    public GameObject prefabToEquip; // For 3D or layered 2D
}
```

### 1.5 `RecipeData`
Magic research recipes to unlock new items or music.
```csharp
[CreateAssetMenu(fileName = "NewRecipe", menuName = "Database/Recipe")]
public class RecipeData : ScriptableObject
{
    public string recipeId;
    public string recipeName;
    [TextArea] public string loreDescription;
    
    // Requirements
    public int requiredGold;
    public List<MaterialRequirement> requiredMaterials;
    public int researchPointsNeeded; // How much "effort" it takes to complete
    
    // Rewards
    public ItemData rewardItem; // Can be MusicData, CosmeticData, etc.
}

[System.Serializable]
public struct MaterialRequirement
{
    public MaterialData material;
    public int amount;
}
```

### 1.6 `MissionData`
Daily/Weekly tasks for the player.
```csharp
[CreateAssetMenu(fileName = "NewMission", menuName = "Database/Mission")]
public class MissionData : ScriptableObject
{
    public string missionId;
    public string missionTitle;
    public enum MissionType { PomodoroCount, CraftCount, PlayTime }
    public MissionType type;
    public int targetValue; // e.g., 3 (Pomodoros)
    
    // Rewards
    public int rewardGold;
    public List<MaterialRequirement> rewardMaterials;
}
```

### 1.7 `ShopInventoryData`
Lists what is available in the town shop.
```csharp
[CreateAssetMenu(fileName = "ShopInventory", menuName = "Database/ShopInventory")]
public class ShopInventoryData : ScriptableObject
{
    public List<ShopListing> listings;
}

[System.Serializable]
public struct ShopListing
{
    public ItemData item; // The thing being sold
    public int priceGold;
    public bool isOneTimePurchase; // True for outfits/music, False for materials
}
```

---

## 2. Save Data (JSON Structures)

These structures will be serialized to JSON and saved locally (`Application.persistentDataPath`). These must only use primitive types, strings, or simple lists/dictionaries (no direct Unity Object references).

### 2.1 The Root Save Object
```csharp
[System.Serializable]
public class GameSaveData
{
    public string lastSaveTimestamp; // ISO 8601, used for offline time calculation
    
    public PlayerSaveData player = new PlayerSaveData();
    public List<DollSaveData> dolls = new List<DollSaveData>();
    public string activeDollId; // Which doll is currently displayed on the home screen
    
    public ShopSaveData shop = new ShopSaveData();
    public MissionSaveData missions = new MissionSaveData();
    public TimerSaveData timer = new TimerSaveData();
}
```

### 2.2 `PlayerSaveData` (Economy & Unlockables)
```csharp
[System.Serializable]
public class PlayerSaveData
{
    public int gold;
    
    // Inventory: Item ID -> Quantity
    public Dictionary<string, int> materials = new Dictionary<string, int>();
    
    // Unlocks: Lists of Item IDs
    public List<string> unlockedRecipes = new List<string>();
    public List<string> unlockedCosmetics = new List<string>();
    public List<string> unlockedMusic = new List<string>();
    
    // Currently active research project
    public string currentResearchRecipeId;
    public int currentResearchProgress;
}
```

### 2.3 `DollSaveData` (Character States)
```csharp
[System.Serializable]
public class DollSaveData
{
    public string id;       // UUID generated at creation
    public string name;     // Player-given name
    
    // Appearance (IDs of equipped items)
    public string equippedOutfitId;
    public string equippedAccessoryId;
    
    // Personality Parameters (0-100)
    public int curiosity;
    public int diligence;
    public int sociability;
    public int emotion;
    public int caution;
    public int confidence;
    
    // Relationship Parameters
    public int trust;
    public int affection;
    public int dependency;
    
    // Mood State (Determines idle animations/dialogue)
    public string currentMood; // e.g., "energy", "calm", "fatigue"
    public string lastInteractionTimestamp;
}
```

### 2.4 `ShopSaveData`
Tracks what has been bought, specifically for one-time purchases so they disappear from the shop.
```csharp
[System.Serializable]
public class ShopSaveData
{
    // List of Item IDs that were purchased and cannot be bought again
    public List<string> purchasedOneTimeItems = new List<string>();
}
```

### 2.5 `MissionSaveData`
Tracks daily/weekly progress.
```csharp
[System.Serializable]
public class MissionSaveData
{
    public string lastDailyResetTimestamp;
    
    // Mission ID -> current progress value
    public Dictionary<string, int> dailyMissionProgress = new Dictionary<string, int>();
    
    // Mission ID -> whether the reward has been claimed today
    public Dictionary<string, bool> dailyMissionClaimed = new Dictionary<string, bool>();
}
```

### 2.6 `TimerSaveData` (For Offline Calculation)
Crucial for Pomodoro games to ensure timers don't break when the user switches apps.
```csharp
[System.Serializable]
public class TimerSaveData
{
    public bool isRunning;
    public string sessionType; // "Pomodoro", "ShortBreak", "LongBreak"
    public float remainingSecondsAtSave;
    public int completedPomodorosToday;
}
```
