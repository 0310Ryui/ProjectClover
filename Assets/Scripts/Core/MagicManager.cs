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

    public bool CanResearch(MagicRecipeData recipe) {
        if (SaveManager.Current.player.unlockedMagics.Contains(recipe.recipeId)) {
            return false; // Already unlocked
        }

        var inventory = SaveManager.Current.player.inventory;
        if (inventory.money < recipe.requiredMoney) {
            return false;
        }

        foreach (var req in recipe.requiredMaterials) {
            if (inventory.GetMaterialCount(req.material.materialId) < req.amount) {
                return false;
            }
        }

        return true;
    }

    public void ResearchMagic(MagicRecipeData recipe) {
        if (!CanResearch(recipe)) {
            Debug.LogWarning("Cannot research magic: requirements not met.");
            return;
        }

        var inventory = SaveManager.Current.player.inventory;
        inventory.money -= recipe.requiredMoney;
        
        foreach (var req in recipe.requiredMaterials) {
            inventory.AddMaterial(req.material.materialId, -req.amount);
        }

        SaveManager.Current.player.unlockedMagics.Add(recipe.recipeId);
        SaveManager.Save();

        // Update Global UI
        if (rewardManager != null) {
            rewardManager.UpdateInventoryUI();
        }

        Debug.Log($"Researched Magic: {recipe.recipeName}!");
    }
}
