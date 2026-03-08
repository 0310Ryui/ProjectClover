using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using System.Collections;

public class FinalVerification : MonoBehaviour {
    [MenuItem("ProjectClover/Run Final Verification")]
    public static void Run() {
        // Fix scene first
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        var canvas = GameObject.Find("Canvas");
        var mainScreen = canvas?.transform.Find("MainScreen");
        
        // Move inventory box so it doesn't overlap Todo
        var invBox = mainScreen?.Find("InventoryBox");
        if (invBox != null) {
            var rect = invBox.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(-20, -150); // Move down
            EditorUtility.SetDirty(invBox);
        }

        EditorSceneManager.SaveScene(scene);
        
        // Start Test
        var go = new GameObject("TestRunner");
        go.AddComponent<FinalVerification>();
        EditorApplication.isPlaying = true;
    }

    void Start() {
        StartCoroutine(TestSequence());
    }

    IEnumerator TestSequence() {
        yield return new WaitForSeconds(1f);
        
        var inventory = SaveManager.Current.player.inventory;
        inventory.money = 1000;
        inventory.AddMaterial("MAT_THREAD", 10);
        
        var rewardManager = GameObject.Find("MainManager")?.GetComponent<RewardManager>();
        rewardManager?.UpdateInventoryUI();
        
        Debug.Log("[PROOF] Balance BEFORE: " + inventory.money);
        
        var btnGo = GameObject.Find("Btn_Research_UnlockBGM1");
        var btn = btnGo?.GetComponent<Button>();
        
        if (btn != null) {
            Debug.Log("[PROOF] Clicking Craft Button...");
            btn.onClick.Invoke();
        } else {
            Debug.LogError("[PROOF] Craft Button not found!");
        }

        yield return new WaitForSeconds(1f);
        
        Debug.Log("[PROOF] Balance AFTER: " + inventory.money);
        
        if (inventory.money < 1000) {
            Debug.Log("==== ULOOP_VERIFICATION_SUCCESS: MONEY CONSUMED ====");
        } else {
            Debug.LogError("==== ULOOP_VERIFICATION_FAILED: MONEY NOT CONSUMED ====");
        }

        yield return new WaitForSeconds(1f);
        EditorApplication.isPlaying = false;
        Destroy(gameObject);
    }
}
