using UnityEngine;
using UnityEngine.UI;

public class RewardManager : MonoBehaviour {
    [Header("Dependencies")]
    public TimerManager timerManager;

    [Header("UI References")]
    public Text moneyText;
    public Text materialText;
    public InputField taskInputField;

    [Header("Reward Settings")]
    public int baseMoneyReward = 100;
    public int breakBonusMoney = 50;
    public MaterialData commonMaterial;

    private void Start() {
        if (timerManager != null) {
            timerManager.onTimerCompleted += OnTimerComplete;
        }
        UpdateInventoryUI();
    }

    private void OnDestroy() {
        if (timerManager != null) {
            timerManager.onTimerCompleted -= OnTimerComplete;
        }
    }

    private void OnTimerComplete(TimerState completedState) {
        if (completedState == TimerState.Work) {
            GrantRewards();
            SaveManager.Save();
            UpdateInventoryUI();
            
            if (taskInputField != null) {
                Debug.Log($"Task Completed: {taskInputField.text}");
                taskInputField.text = ""; 
            }
        } else if (completedState == TimerState.WaitingForBreak) {
            // Reaping the reward for starting the break
            var inventory = SaveManager.Current.player.inventory;
            inventory.money += breakBonusMoney;
            SaveManager.Save();
            UpdateInventoryUI();
            Debug.Log($"Granted {breakBonusMoney}G for taking a break!");
        }
    }

    private void GrantRewards() {
        var inventory = SaveManager.Current.player.inventory;
        inventory.money += baseMoneyReward;
        
        if (commonMaterial != null) {
            inventory.AddMaterial(commonMaterial.materialId, 1);
        }
        
        Debug.Log($"Granted {baseMoneyReward}G and {(commonMaterial != null ? "1x " + commonMaterial.materialName : "no material")}.");
    }

    public void UpdateInventoryUI() {
        var inventory = SaveManager.Current.player.inventory;
        if (moneyText != null) {
            moneyText.text = $"{inventory.money} G";
        }
        if (materialText != null) {
            int totalItems = 0;
            foreach (var count in inventory.materialCounts) totalItems += count;
            materialText.text = $"Items: {totalItems}";
        }
    }
}
