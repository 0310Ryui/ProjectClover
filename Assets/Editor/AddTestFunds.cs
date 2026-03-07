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
        inventory.money += 1000;
        inventory.AddMaterial("MAT_THREAD", 20);
        SaveManager.Save();

        var mainManagerGo = GameObject.Find("MainManager");
        if (mainManagerGo != null) {
            var rewardManager = mainManagerGo.GetComponent<RewardManager>();
            if (rewardManager != null) {
                rewardManager.UpdateInventoryUI();
            }
        }

        Debug.Log("Added 1000 G and 20 Thread for testing!");
    }
}
