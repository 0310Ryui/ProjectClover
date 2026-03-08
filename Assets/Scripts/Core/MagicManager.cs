using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MagicManager : MonoBehaviour {
    [Header("Dependencies")]
    public RewardManager rewardManager;

    [Header("UI References")]
    public Transform recipeListContainer;
    public GameObject recipeUIPrefab;

    [Header("Data")]
    public List<MagicRecipeData> allRecipes;

    private void Start() {
        if (rewardManager == null) {
            rewardManager = GetComponent<RewardManager>() ?? GameObject.Find("MainManager")?.GetComponent<RewardManager>();
        }
    }

    public bool CanResearch(MagicRecipeData recipe) {
        if (recipe == null) return false;
        
        var inventory = SaveManager.Current.player.inventory;
        var unlocked = SaveManager.Current.player.unlockedMagics;

        if (!recipe.isConsumable && unlocked.Contains(recipe.recipeId)) {
            return false; // Already unlocked
        }

        if (inventory.money < recipe.requiredMoney) {
            return false;
        }

        foreach (var req in recipe.requiredMaterials) {
            if (req.material == null) continue;
            if (inventory.GetMaterialCount(req.material.materialId) < req.amount) {
                return false;
            }
        }

        return true;
    }

    public void ResearchMagic(MagicRecipeData recipe) {
        if (recipe == null) return;

        if (!CanResearch(recipe)) {
            Debug.LogWarning($"[Magic] Cannot research {recipe.recipeId}: requirements not met or already unlocked.");
            return;
        }

        var inventory = SaveManager.Current.player.inventory;
        
        // Final sanity check for material counts before double subtraction
        foreach (var req in recipe.requiredMaterials) {
            if (req.material == null) continue;
            if (inventory.GetMaterialCount(req.material.materialId) < req.amount) {
                Debug.LogError($"[Magic] Critical Error: Material requirement check failed midway for {req.material.materialId}");
                return;
            }
        }

        // Subtract resources
        int oldMoney = inventory.money;
        inventory.money -= recipe.requiredMoney;
        Debug.Log($"[Magic] Consuming Money: {oldMoney} -> {inventory.money} (Req: {recipe.requiredMoney})");

        foreach (var req in recipe.requiredMaterials) {
            if (req.material == null) continue;
            int oldAmt = inventory.GetMaterialCount(req.material.materialId);
            inventory.AddMaterial(req.material.materialId, -req.amount);
            Debug.Log($"[Magic] Consuming {req.material.materialId}: {oldAmt} -> {inventory.GetMaterialCount(req.material.materialId)} (Req: {req.amount})");
        }

        // Persist
        if (!recipe.isConsumable) {
            SaveManager.Current.player.unlockedMagics.Add(recipe.recipeId);
        }
        SaveManager.Save();

        // Refresh UI
        if (rewardManager != null) {
            rewardManager.UpdateInventoryUI();
            Debug.Log("[Magic] Inventory UI updated.");
        } else {
            Debug.LogError("[Magic] FAILED to update UI because rewardManager is null.");
        }

        Debug.Log($"[Magic] MISSION SUCCESS: {recipe.recipeName} researched.");
    }
}
