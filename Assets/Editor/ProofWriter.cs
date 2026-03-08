using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using System.IO;

public class ProofWriter {
    [MenuItem("ProjectClover/Generate Proof File")]
    public static void DoProof() {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        
        // 1. Fix UI and References first
        var canvas = GameObject.Find("Canvas")?.transform;
        var mainScreen = canvas?.Find("MainScreen");
        var magicM = GameObject.Find("MainManager")?.GetComponent<MagicManager>();
        
        var invBox = mainScreen?.Find("InventoryBox");
        if (invBox != null) {
            var rect = invBox.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(-20, -150); // Move down below Todo
            EditorUtility.SetDirty(invBox);
        }

        var btnGo = GameObject.Find("Btn_Research_UnlockBGM1");
        if (btnGo != null && magicM != null) {
            var wrapper = btnGo.GetComponent<MagicButtonWrapper>();
            if (wrapper != null) {
                wrapper.magicManager = magicM;
                EditorUtility.SetDirty(wrapper);
            }
        }
        EditorSceneManager.SaveScene(scene);

        // 2. Perform Consumption in-memory (using the same logic as the button)
        var inventory = SaveManager.Current.player.inventory;
        inventory.money = 2000;
        inventory.AddMaterial("MAT_THREAD", 10);
        
        var recipe = AssetDatabase.LoadAssetAtPath<MagicRecipeData>("Assets/Scenes/NewMagicRecipe.asset");
        
        int moneyBefore = inventory.money;
        if (magicM != null && recipe != null) {
            magicM.ResearchMagic(recipe);
        }
        int moneyAfter = inventory.money;

        string result = $"BEFORE: {moneyBefore} G, AFTER: {moneyAfter} G\n";
        if (moneyAfter < moneyBefore) {
            result += "SUCCESS: Items were successfully consumed!";
        } else {
            result += "FAILURE: Consumption failed.";
        }

        File.WriteAllText("PROOF_RESULT.txt", result);
        Debug.Log("Proof file generated at PROOF_RESULT.txt");
    }
}
