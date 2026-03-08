using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class FinalFixAndTest {
    private static int step = 0;
    private static int startMoney = 0;
    private static int startThread = 0;

    [MenuItem("ProjectClover/Final Fix and Test")]
    public static void DoEverything() {
        FixInfrastructure();
        StartTest();
    }

    private static void FixInfrastructure() {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        
        var canvas = GameObject.Find("Canvas")?.transform;
        var mainScreen = canvas?.Find("MainScreen");
        var magicM = GameObject.Find("MainManager")?.GetComponent<MagicManager>();

        if (mainScreen == null || magicM == null) {
            Debug.LogError("Required managers or screens missing!");
            return;
        }

        // 1. Create a proper Inventory Box for visibility
        var invBoxT = mainScreen.Find("InventoryBox");
        if (invBoxT == null) {
            var go = new GameObject("InventoryBox");
            go.transform.SetParent(mainScreen, false);
            invBoxT = go.transform;
        }

        var rect = invBoxT.GetComponent<RectTransform>() ?? invBoxT.gameObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1); rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-20, -20);
        rect.sizeDelta = new Vector2(400, 150);

        var img = invBoxT.GetComponent<Image>() ?? invBoxT.gameObject.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.8f);

        // Move existing texts inside
        var moneyTxt = mainScreen.Find("Text_Money");
        if (moneyTxt != null) moneyTxt.SetParent(invBoxT, false);
        var matTxt = mainScreen.Find("Text_Materials");
        if (matTxt != null) matTxt.SetParent(invBoxT, false);

        // 2. Fix Button reference
        var btnGo = GameObject.Find("Btn_Research_UnlockBGM1");
        if (btnGo != null) {
            var wrapper = btnGo.GetComponent<MagicButtonWrapper>();
            if (wrapper != null) {
                var so = new SerializedObject(wrapper);
                so.FindProperty("magicManager").objectReferenceValue = magicM;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(wrapper);
            }
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FinalFix] Infrastructure fixed and saved.");
    }

    private static void StartTest() {
        step = 0;
        EditorApplication.update += OnUpdate;
        EditorApplication.isPlaying = true;
    }

    private static void OnUpdate() {
        if (!Application.isPlaying) return;

        var inventory = SaveManager.Current?.player?.inventory;
        if (inventory == null) return;

        if (step == 0) {
            // Setup test funds
            inventory.money = 1000;
            inventory.AddMaterial("MAT_THREAD", 10);
            startMoney = inventory.money;
            startThread = inventory.GetMaterialCount("MAT_THREAD");
            Debug.Log($"[Test] Starting Test. Money: {startMoney}, Thread: {startThread}");
            step = 1;
        } 
        else if (step == 1) {
            var btnGo = GameObject.Find("Btn_Research_UnlockBGM1");
            if (btnGo != null) {
                var btn = btnGo.GetComponent<Button>();
                if (btn != null) {
                    Debug.Log("[Test] Clicking Craft Button...");
                    btn.onClick.Invoke();
                    step = 2;
                }
            }
        }
        else if (step == 2) {
            int endMoney = inventory.money;
            int endThread = inventory.GetMaterialCount("MAT_THREAD");
            Debug.Log($"[Test] Final State - Money: {endMoney}, Thread: {endThread}");
            
            if (endMoney < startMoney) {
                Debug.Log("==== ULOOP_VERIFICATION_SUCCESS: CONSUMPTION CONFIRMED ====");
            } else {
                Debug.LogError("==== ULOOP_VERIFICATION_FAILED: NO CONSUMPTION ====");
            }
            
            step = 3;
            EditorApplication.isPlaying = false;
        }
        else if (step == 3) {
            EditorApplication.update -= OnUpdate;
        }
    }
}
