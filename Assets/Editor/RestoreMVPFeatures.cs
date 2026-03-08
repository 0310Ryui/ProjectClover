using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class RestoreMVPFeatures {
    [MenuItem("ProjectClover/Restore MVP Features")]
    public static void DoSetup() {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        
        var canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) return;
        var mainScreen = GameObject.Find("MainScreen");
        if (mainScreen == null) return;
        var todoOverlay = GameObject.Find("TodoOverlay");
        if (todoOverlay == null) return;
        var mainManagerGo = GameObject.Find("MainManager");
        if (mainManagerGo == null) return;
        
        var uiM = mainManagerGo.GetComponent<UIManager>();
        var rm = mainManagerGo.GetComponent<RewardManager>();
        var dm = mainManagerGo.GetComponent<DollManager>();
        var mm = mainManagerGo.GetComponent<MagicManager>();

        // --- 1. Task Input (TODO) ---
        // Remove dummy text
        var dummyText = GameObject.Find("Text_DummyTask");
        if (dummyText != null) UnityEngine.Object.DestroyImmediate(dummyText);

        // Add Input Field to TodoOverlay
        var inputFieldGo = new GameObject("TaskInputField");
        inputFieldGo.transform.SetParent(todoOverlay.transform, false);
        var inputRect = inputFieldGo.AddComponent<RectTransform>();
        inputRect.anchoredPosition = new Vector2(540, 1400); // Centerish Top
        inputRect.sizeDelta = new Vector2(800, 120);
        var inputImage = inputFieldGo.AddComponent<Image>();
        inputImage.color = new Color(1f, 1f, 1f, 0.1f);
        var inputField = inputFieldGo.AddComponent<InputField>();

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(inputFieldGo.transform, false);
        var tRect = textGo.AddComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero; tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = Vector2.zero;
        var txt = textGo.AddComponent<Text>();
        txt.fontSize = 50;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleLeft;
        
        var placeholderGo = new GameObject("Placeholder");
        placeholderGo.transform.SetParent(inputFieldGo.transform, false);
        var pRect = placeholderGo.AddComponent<RectTransform>();
        pRect.anchorMin = Vector2.zero; pRect.anchorMax = Vector2.one;
        pRect.sizeDelta = Vector2.zero;
        var pTxt = placeholderGo.AddComponent<Text>();
        pTxt.text = "今回のタスクを入力...";
        pTxt.fontSize = 50;
        pTxt.color = new Color(1, 1, 1, 0.5f);
        pTxt.alignment = TextAnchor.MiddleLeft;

        inputField.textComponent = txt;
        inputField.placeholder = pTxt;
        rm.taskInputField = inputField;

        // --- 2. Doll Interaction (Speech Bubble) ---
        var dollGo = GameObject.Find("DollImage");
        if (dollGo != null) {
            // Add Button to Doll
            var dollBtn = dollGo.GetComponent<Button>();
            if (dollBtn == null) dollBtn = dollGo.AddComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(dollBtn.onClick, dm.OnDollTapped);

            // Add Speech Bubble
            var speechGo = GameObject.Find("SpeechBubble");
            if (speechGo != null) UnityEngine.Object.DestroyImmediate(speechGo);

            speechGo = new GameObject("SpeechBubble");
            speechGo.transform.SetParent(dollGo.transform, false);
            var sRect = speechGo.AddComponent<RectTransform>();
            sRect.anchoredPosition = new Vector2(250, 400); // Top Right of doll
            sRect.sizeDelta = new Vector2(400, 150);
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
            dm.dollImage = dollGo.GetComponent<Image>();
            speechGo.SetActive(false); // Hide by default
        }

        // --- 3. Magic/Craft UI ---
        // Button on MainScreen
        var btnMagicGo = GameObject.Find("Btn_Magic");
        if (btnMagicGo != null) UnityEngine.Object.DestroyImmediate(btnMagicGo);
        
        btnMagicGo = new GameObject("Btn_Magic");
        btnMagicGo.transform.SetParent(mainScreen.transform, false);
        var mRect = btnMagicGo.AddComponent<RectTransform>();
        mRect.anchorMin = new Vector2(0, 0); mRect.anchorMax = new Vector2(0, 0);
        mRect.anchoredPosition = new Vector2(180, 260); // Bottom left
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

        // Magic Overlay
        var magicOverlay = GameObject.Find("MagicOverlay");
        if (magicOverlay != null) UnityEngine.Object.DestroyImmediate(magicOverlay);

        magicOverlay = new GameObject("MagicOverlay");
        magicOverlay.transform.SetParent(canvasGo.transform, false);
        var oRect = magicOverlay.AddComponent<RectTransform>();
        oRect.anchorMin = Vector2.zero; oRect.anchorMax = Vector2.one; oRect.sizeDelta = Vector2.zero;
        magicOverlay.AddComponent<Image>().color = new Color(0.2f, 0.1f, 0.3f, 0.95f); // Purple-ish

        var btnCloseMagicGo = new GameObject("Btn_CloseMagic");
        btnCloseMagicGo.transform.SetParent(magicOverlay.transform, false);
        var cRect = btnCloseMagicGo.AddComponent<RectTransform>();
        cRect.anchoredPosition = new Vector2(100, 1720); cRect.sizeDelta = new Vector2(120, 120);
        btnCloseMagicGo.AddComponent<Image>().color = new Color(0.4f, 0.3f, 0.2f, 1f);
        var cBtn = btnCloseMagicGo.AddComponent<Button>();
        var cxGo = new GameObject("Text"); cxGo.transform.SetParent(btnCloseMagicGo.transform, false);
        var cxRect = cxGo.AddComponent<RectTransform>(); cxRect.anchorMin = Vector2.zero; cxRect.anchorMax = Vector2.one; cxRect.sizeDelta = Vector2.zero;
        var cxTxt = cxGo.AddComponent<Text>(); cxTxt.text = "<"; cxTxt.fontSize = 40; cxTxt.color = Color.white; cxTxt.alignment = TextAnchor.MiddleCenter;

        var titleMagicGo = new GameObject("Text_MagicTitle");
        titleMagicGo.transform.SetParent(magicOverlay.transform, false);
        var tmRect = titleMagicGo.AddComponent<RectTransform>();
        tmRect.anchoredPosition = new Vector2(540, 1720); tmRect.sizeDelta = new Vector2(400, 100);
        var tmTxt = titleMagicGo.AddComponent<Text>(); tmTxt.text = "Craft Magic"; tmTxt.fontSize = 60; tmTxt.color = Color.white; tmTxt.alignment = TextAnchor.MiddleCenter;

        // "Unlock BGM 1" Research Button
        var btnResearchGo = new GameObject("Btn_Research");
        btnResearchGo.transform.SetParent(magicOverlay.transform, false);
        var rRect = btnResearchGo.AddComponent<RectTransform>();
        rRect.anchoredPosition = new Vector2(540, 1400); rRect.sizeDelta = new Vector2(800, 200);
        btnResearchGo.AddComponent<Image>().color = new Color(0.7f, 0.2f, 0.5f, 1f);
        var rBtn = btnResearchGo.AddComponent<Button>();
        var rxGo = new GameObject("Text"); rxGo.transform.SetParent(btnResearchGo.transform, false);
        var rxRect = rxGo.AddComponent<RectTransform>(); rxRect.anchorMin = Vector2.zero; rxRect.anchorMax = Vector2.one; rxRect.sizeDelta = Vector2.zero;
        var rxTxt = rxGo.AddComponent<Text>();
        rxTxt.text = "Research: Unlock BGM 1\n(Requires 500 G, 5 Thread)";
        rxTxt.fontSize = 45; rxTxt.color = Color.white; rxTxt.alignment = TextAnchor.MiddleCenter;

        // Wire Magic Actions
        var mbw = mainManagerGo.GetComponent<MagicButtonWrapper>();
        if (mbw == null) mbw = mainManagerGo.AddComponent<MagicButtonWrapper>();
        mbw.magicManager = mm;
        var firstRecipe = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Data/Recipes/UnlockBGM1.asset");
        var soManager = new SerializedObject(mbw);
        soManager.FindProperty("recipeToResearch").objectReferenceValue = firstRecipe;
        soManager.ApplyModifiedProperties();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(rBtn.onClick, mbw.OnClick);

        // Update UIManager
        uiM.magicOverlay = magicOverlay;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(mBtn.onClick, uiM.ShowMagicOverlay);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(cBtn.onClick, uiM.HideMagicOverlay);

        magicOverlay.SetActive(false);

        EditorUtility.SetDirty(uiM);
        EditorUtility.SetDirty(rm);
        EditorUtility.SetDirty(dm);
        EditorUtility.SetDirty(mm);
        if (mbw != null) EditorUtility.SetDirty(mbw);

        EditorSceneManager.SaveScene(scene);
        Debug.Log("MVP Features (Todo Input, Doll Speech, Magic UI) Restored!");
    }
}
