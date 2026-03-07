using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class FixMagicOverlay {
    [MenuItem("ProjectClover/Fix Magic Overlay")]
    public static void DoSetup() {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        
        var canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) return;
        var canvasT = canvasGo.transform;

        var mainManagerGo = GameObject.Find("MainManager");
        if (mainManagerGo == null) return;
        var uiM = mainManagerGo.GetComponent<UIManager>();
        var magicM = mainManagerGo.GetComponent<MagicManager>();

        var magicOverT = canvasT.Find("MagicOverlay");
        if (magicOverT == null) {
            var mGo = new GameObject("MagicOverlay");
            mGo.transform.SetParent(canvasT, false);
            var mRect = mGo.AddComponent<RectTransform>();
            mRect.anchorMin = Vector2.zero; mRect.anchorMax = Vector2.one;
            mRect.offsetMin = Vector2.zero; mRect.offsetMax = Vector2.zero;
            mGo.AddComponent<Image>().color = new Color(0.2f, 0.1f, 0.3f, 0.95f);
            
            magicOverT = mGo.transform;

            var titleGo = new GameObject("Text_MagicTitle");
            titleGo.transform.SetParent(magicOverT, false);
            var tRect = titleGo.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.5f, 1f); tRect.anchorMax = new Vector2(0.5f, 1f);
            tRect.pivot = new Vector2(0.5f, 1f);
            tRect.anchoredPosition = new Vector2(0, -100);
            tRect.sizeDelta = new Vector2(600, 100);
            var tTxt = titleGo.AddComponent<Text>();
            tTxt.text = "Magic Research";
            tTxt.fontSize = 60;
            tTxt.color = Color.white;
            tTxt.alignment = TextAnchor.MiddleCenter;

            var closeGo = new GameObject("Btn_CloseMagic");
            closeGo.transform.SetParent(magicOverT, false);
            var cRect = closeGo.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0, 1); cRect.anchorMax = new Vector2(0, 1);
            cRect.pivot = new Vector2(0, 1);
            cRect.anchoredPosition = new Vector2(50, -50);
            cRect.sizeDelta = new Vector2(120, 120);
            closeGo.AddComponent<Image>().color = Color.red;
            var cBtn = closeGo.AddComponent<Button>();
            var cTxtGo = new GameObject("Text");
            cTxtGo.transform.SetParent(closeGo.transform, false);
            var ctRect = cTxtGo.AddComponent<RectTransform>();
            ctRect.anchorMin = Vector2.zero; ctRect.anchorMax = Vector2.one; ctRect.sizeDelta = Vector2.zero;
            var cTxt = cTxtGo.AddComponent<Text>();
            cTxt.text = "X";
            cTxt.fontSize = 50; cTxt.color = Color.white; cTxt.alignment = TextAnchor.MiddleCenter;

            UnityEditor.Events.UnityEventTools.AddPersistentListener(cBtn.onClick, uiM.HideMagicOverlay);

            // Unlock BGM button
            var resGo = new GameObject("Btn_Research_UnlockBGM1");
            resGo.transform.SetParent(magicOverT, false);
            var resRect = resGo.AddComponent<RectTransform>();
            resRect.anchorMin = new Vector2(0.5f, 0.5f); resRect.anchorMax = new Vector2(0.5f, 0.5f);
            resRect.anchoredPosition = new Vector2(0, 0);
            resRect.sizeDelta = new Vector2(800, 200);
            resGo.AddComponent<Image>().color = new Color(0.2f, 0.6f, 1f, 1f);
            var resBtn = resGo.AddComponent<Button>();
            var rmbw = resGo.AddComponent<MagicButtonWrapper>();
            
            var testRecipe = AssetDatabase.LoadAssetAtPath<MagicRecipeData>("Assets/Scenes/NewMagicRecipe.asset");
            if (testRecipe != null) {
                var so = new SerializedObject(rmbw);
                so.FindProperty("recipeToResearch").objectReferenceValue = testRecipe;
                so.ApplyModifiedProperties();
            }

            UnityEditor.Events.UnityEventTools.AddPersistentListener(resBtn.onClick, rmbw.OnClick);

            var rTxtGo = new GameObject("Text");
            rTxtGo.transform.SetParent(resGo.transform, false);
            var rTxtRect = rTxtGo.AddComponent<RectTransform>();
            rTxtRect.anchorMin = Vector2.zero; rTxtRect.anchorMax = Vector2.one; rTxtRect.sizeDelta = Vector2.zero;
            var rTxt = rTxtGo.AddComponent<Text>();
            rTxt.text = "Unlock BGM 1\nCost: 500 G, 5 Thread";
            rTxt.fontSize = 45; rTxt.color = Color.white; rTxt.alignment = TextAnchor.MiddleCenter;
        }

        uiM.magicOverlay = magicOverT.gameObject;
        EditorUtility.SetDirty(uiM);
        magicOverT.gameObject.SetActive(false);

        EditorSceneManager.SaveScene(scene);
        Debug.Log("Magic Overlay Recreated and Wired!");
    }
}
