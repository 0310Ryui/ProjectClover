using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class RobustUIFixer {
    [MenuItem("ProjectClover/Robust UI Fix")]
    public static void DoFix() {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        
        var canvas = GameObject.Find("Canvas")?.transform;
        var mainScreen = canvas?.Find("MainScreen");
        var magicM = GameObject.Find("MainManager")?.GetComponent<MagicManager>();

        if (mainScreen == null || magicM == null) return;

        // Create InventoryBox if it doesn't exist
        var invBoxT = mainScreen.Find("InventoryBox");
        if (invBoxT == null) {
            var go = new GameObject("InventoryBox", typeof(RectTransform));
            go.transform.SetParent(mainScreen, false);
            invBoxT = go.transform;
            
            var rect = invBoxT.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1); rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-20, -20);
            rect.sizeDelta = new Vector2(500, 200);

            var img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.8f);
            
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleRight;
            vlg.childForceExpandHeight = false;
        }

        // Move and fix Text_Money
        var moneyTxt = mainScreen.Find("Text_Money") ?? invBoxT.Find("Text_Money");
        if (moneyTxt != null) {
            moneyTxt.SetParent(invBoxT, false);
            var t = moneyTxt.GetComponent<Text>();
            t.fontSize = 60;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.alignment = TextAnchor.MiddleRight;
        }

        // Move and fix Text_Materials
        var matTxt = mainScreen.Find("Text_Materials") ?? invBoxT.Find("Text_Materials");
        if (matTxt != null) {
            matTxt.SetParent(invBoxT, false);
            var t = matTxt.GetComponent<Text>();
            t.fontSize = 50;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.alignment = TextAnchor.MiddleRight;
        }

        // Fix Craft Button reference
        var btnGo = GameObject.Find("Btn_Research_UnlockBGM1");
        if (btnGo != null) {
            var wrapper = btnGo.GetComponent<MagicButtonWrapper>();
            if (wrapper != null) {
                var so = new SerializedObject(wrapper);
                so.FindProperty("magicManager").objectReferenceValue = magicM;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(wrapper);
                Debug.Log("Fixed MagicManager reference on Craft button.");
            }
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("Robust UI Fix Completed.");
    }
}
