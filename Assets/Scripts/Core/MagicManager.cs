using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MagicManager : MonoBehaviour {
    [Header("Dependencies")]
    public RewardManager rewardManager;
    public DollManager dollManager;

    [Header("UI References")]
    public Transform recipeListContainer;
    public GameObject recipeUIPrefab;

    [Header("Data")]
    public List<MagicRecipeData> allRecipes;

    private void Start() {
        if (rewardManager == null) {
            rewardManager = GetComponent<RewardManager>() ?? GameObject.Find("MainManager")?.GetComponent<RewardManager>();
        }
        if (dollManager == null) {
            dollManager = FindFirstObjectByType<DollManager>();
        }
    }

    private void OnEnable() {
        RefreshMagicUi();
    }

    public bool CanResearch(MagicRecipeData recipe) {
        if (recipe == null) return false;

        var inventory = SaveManager.Current.player.inventory;
        var unlocked = SaveManager.Current.player.unlockedMagics;

        if (!recipe.isConsumable && unlocked.Contains(recipe.recipeId)) {
            return false;
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
            ShowResearchFailureMessage(recipe);
            return;
        }

        var inventory = SaveManager.Current.player.inventory;

        foreach (var req in recipe.requiredMaterials) {
            if (req.material == null) continue;
            if (inventory.GetMaterialCount(req.material.materialId) < req.amount) {
                Debug.LogError($"[Magic] Critical Error: Material requirement check failed midway for {req.material.materialId}");
                return;
            }
        }

        var oldMoney = inventory.money;
        inventory.money -= recipe.requiredMoney;
        Debug.Log($"[Magic] Consuming Money: {oldMoney} -> {inventory.money} (Req: {recipe.requiredMoney})");

        foreach (var req in recipe.requiredMaterials) {
            if (req.material == null) continue;
            var oldAmount = inventory.GetMaterialCount(req.material.materialId);
            inventory.AddMaterial(req.material.materialId, -req.amount);
            Debug.Log($"[Magic] Consuming {req.material.materialId}: {oldAmount} -> {inventory.GetMaterialCount(req.material.materialId)} (Req: {req.amount})");
        }

        if (!recipe.isConsumable) {
            SaveManager.Current.player.unlockedMagics.Add(recipe.recipeId);
        }
        SaveManager.Save();

        if (rewardManager != null) {
            rewardManager.UpdateInventoryUI();
            Debug.Log("[Magic] Inventory UI updated.");
        } else {
            Debug.LogError("[Magic] FAILED to update UI because rewardManager is null.");
        }

        RefreshMagicUi();
        Debug.Log($"[Magic] MISSION SUCCESS: {recipe.recipeName} researched.");
    }

    private void ShowResearchFailureMessage(MagicRecipeData recipe) {
        if (recipe == null || dollManager == null) return;

        var unlocked = SaveManager.Current.player.unlockedMagics;
        if (!recipe.isConsumable && unlocked.Contains(recipe.recipeId)) {
            dollManager.ShowMessage("その研究はもう終わってるよ！");
            return;
        }

        var inventory = SaveManager.Current.player.inventory;
        var missingMoney = inventory.money < recipe.requiredMoney;
        var missingMaterials = false;

        foreach (var req in recipe.requiredMaterials) {
            if (req.material == null) continue;
            if (inventory.GetMaterialCount(req.material.materialId) < req.amount) {
                missingMaterials = true;
                break;
            }
        }

        if (missingMoney || missingMaterials) {
            dollManager.ShowMessage("お金とアイテムが足りないよ！");
        }
    }

    private void RefreshMagicUi() {
        var ui = FindFirstObjectByType<MagicUiManager>();
        if (ui != null) {
            ui.RefreshUI();
        }
    }
}
