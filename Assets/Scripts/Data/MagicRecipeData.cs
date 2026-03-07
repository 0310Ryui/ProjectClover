using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct MaterialRequirement {
    public MaterialData material;
    public int amount;
}

[CreateAssetMenu(fileName = "NewMagicRecipe", menuName = "LittleDollAtelier/MagicRecipeData")]
public class MagicRecipeData : ScriptableObject
{
    public string recipeId;
    public string recipeName;
    public List<MaterialRequirement> requiredMaterials;
    public int requiredMoney;
    public int researchTimeSeconds;
}
