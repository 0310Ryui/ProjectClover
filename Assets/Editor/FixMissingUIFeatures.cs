using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class FixMissingUIFeatures {
    [MenuItem("ProjectClover/Fix Missing UI Features")]
    public static void DoSetup() {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        
        var canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) return;
        var mainScreen = canvasGo.transform.Find("MainScreen");
        if (mainScreen == null) return;
        var mainManagerGo = GameObject.Find("MainManager");
        if (mainManagerGo == null) return;

        var dm = mainManagerGo.GetComponent<DollManager>();
        var uiM = mainManagerGo.GetComponent<UIManager>();

        // 1. Fix Doll Interaction
        var dollGo = mainScreen.Find("DollImage")?.gameObject;
        if (dollGo != null) {
            var dollImage = dollGo.GetComponent<Image>();
            if (dollImage != null) dollImage.raycastTarget = true; // MUST be true to click

            var dollBtn = dollGo.GetComponent<Button>();
            if (dollBtn == null) dollBtn = dollGo.AddComponent<Button>();
            
            // Clear existing listeners to avoid duplicates, then add
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(dollBtn.onClick, dm.OnDollTapped);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(dollBtn.onClick, dm.OnDollTapped);

            // Ensure Speech Bubble exists and is wired
            var speechGo = dollGo.transform.Find("SpeechBubble")?.gameObject;
            if (speechGo == null) {
                speechGo = new GameObject("SpeechBubble");
                speechGo.transform.SetParent(dollGo.transform, false);
                var sRect = speechGo.AddComponent<RectTransform>();
                sRect.anchoredPosition = new Vector2(200, 300);
                sRect.sizeDelta = new Vector2(450, 180);
                speechGo.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);

                var sTextGo = new GameObject("Text");
                sTextGo.transform.SetParent(speechGo.transform, false);
                var stRect = sTextGo.AddComponent<RectTransform>();
                stRect.anchorMin = Vector2.zero; stRect.anchorMax = Vector2.one;
                stRect.sizeDelta = Vector2.zero;
                var sTxt = sTextGo.AddComponent<Text>();
                sTxt.color = Color.black;
                sTxt.fontSize = 40;
                sTxt.alignment = TextAnchor.MiddleCenter;

                dm.speechBubble = speechGo;
                dm.speechText = sTxt;
                speechGo.SetActive(false);
            }
            // Fix text color in case it was white on white
            if (dm.speechText != null) {
                dm.speechText.color = Color.black;
                dm.speechText.resizeTextForBestFit = true;
                dm.speechText.resizeTextMaxSize = 60;
            }
            EditorUtility.SetDirty(dm);
            Debug.Log("Doll interaction fixed!");
        }

        // 2. Fix Craft Button
        var btnMagicT = mainScreen.Find("Btn_Magic");
        if (btnMagicT == null) {
            var btnMagicGo = new GameObject("Btn_Magic");
            btnMagicGo.transform.SetParent(mainScreen, false);
            var mRect = btnMagicGo.AddComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0.5f, 0.5f); mRect.anchorMax = new Vector2(0.5f, 0.5f);
            mRect.anchoredPosition = new Vector2(-400, -250);
            mRect.sizeDelta = new Vector2(160, 160);
            btnMagicGo.AddComponent<Image>().color = new Color(0.8f, 0.5f, 0.8f, 1f);
            var mBtn = btnMagicGo.AddComponent<Button>();

            var mTxtGo = new GameObject("Text");
            mTxtGo.transform.SetParent(btnMagicGo.transform, false);
            var mTxtRect = mTxtGo.AddComponent<RectTransform>();
            mTxtRect.anchorMin = Vector2.zero; mTxtRect.anchorMax = Vector2.one; mTxtRect.sizeDelta = Vector2.zero;
            var mTxt = mTxtGo.AddComponent<Text>();
            mTxt.text = "Craft";
            mTxt.fontSize = 40;
            mTxt.color = Color.white;
            mTxt.alignment = TextAnchor.MiddleCenter;

            btnMagicT = btnMagicGo.transform;
        }

        // Ensure it's active and visible
        btnMagicT.gameObject.SetActive(true);
        btnMagicT.SetAsLastSibling(); // Bring to front
        var magicBtnImage = btnMagicT.GetComponent<Image>();
        if (magicBtnImage != null) magicBtnImage.raycastTarget = true;
        
        // Ensure its text is visible
        var craftText = btnMagicT.GetComponentInChildren<Text>(true);
        if (craftText != null) {
            craftText.color = Color.white;
            craftText.gameObject.SetActive(true);
        }

        // Re-wire Craft button
        var magicBtnComp = btnMagicT.GetComponent<Button>();
        if (magicBtnComp != null && uiM != null) {
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(magicBtnComp.onClick, uiM.ShowMagicOverlay);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(magicBtnComp.onClick, uiM.ShowMagicOverlay);
        }

        // 3. Prevent TimerBox background from blocking clicks
        var timerBox = mainScreen.Find("TimerBox")?.GetComponent<Image>();
        if (timerBox != null) {
            // The Box itself shouldn't block clicks to the doll if it overlaps, unless it needs to be clicked.
            // Wait, if it has no buttons directly on it, it doesn't need raycast. The buttons inside handle their own raycasts.
            timerBox.raycastTarget = false; 
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("UI Missing Features Fixed!");
    }
}
