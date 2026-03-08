using UnityEngine;
using UnityEditor;

public class AddTestFunds {
    [MenuItem("ProjectClover/Add Test Funds")]
    public static void DoAdd() {
        if (!Application.isPlaying) {
            Debug.LogWarning("Please press Play before adding test funds!");
            return;
        }

        var inventory = SaveManager.Current.player.inventory;
        inventory.money += 5000;
        inventory.AddMaterial("MAT_THREAD", 50);
        SaveManager.Save();

        var mainManagerGo = GameObject.Find("MainManager");
        if (mainManagerGo != null) {
            var rewardManager = mainManagerGo.GetComponent<RewardManager>();
            if (rewardManager != null) {
                rewardManager.UpdateInventoryUI();
            }
        }

        Debug.Log("Added 5000 G and 50 Thread for testing!");
    }
}
