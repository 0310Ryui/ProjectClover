using UnityEngine;
using UnityEngine.UI;

public class MagicButtonWrapper : MonoBehaviour {
    public MagicManager magicManager;
    public MagicRecipeData recipeToResearch;

    void Start() {
        // Runtime fallback if references are missing
        if (magicManager == null) {
            magicManager = GameObject.Find("MainManager")?.GetComponent<MagicManager>();
        }
    }

    public void OnClick() {
        if (magicManager != null && recipeToResearch != null) {
            Debug.Log($"[MagicButton] Starting research for {recipeToResearch.recipeId}");
            magicManager.ResearchMagic(recipeToResearch);
        } else {
            Debug.LogWarning($"[MagicButton] Cannot start research. MagicManager: {magicManager != null}, Recipe: {recipeToResearch != null}");
        }
    }
}
