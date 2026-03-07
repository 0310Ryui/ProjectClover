using UnityEngine;
using UnityEngine.UI;

public class MagicButtonWrapper : MonoBehaviour {
    public MagicManager magicManager;
    public MagicRecipeData recipeToResearch;

    public void OnClick() {
        if (magicManager != null && recipeToResearch != null) {
            magicManager.ResearchMagic(recipeToResearch);
        } else {
            Debug.LogWarning("MagicManager or Recipe is not assigned on the button wrapper.");
        }
    }
}
