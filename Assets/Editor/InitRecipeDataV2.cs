using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class InitRecipeData {
    [MenuItem("ProjectClover/Init Recipe Data V2")]
    public static void Init() {
        var recipe = AssetDatabase.LoadAssetAtPath<MagicRecipeData>("Assets/Scenes/NewMagicRecipe.asset");
        var threadMat = AssetDatabase.LoadAssetAtPath<MaterialData>("Assets/Data/Materials/Thread.asset");

        if (recipe == null) {
            Debug.LogError("NewMagicRecipe.asset not found at Assets/Scenes/NewMagicRecipe.asset!");
            return;
        }

        if (threadMat == null) {
            Debug.LogError("Thread material not found at Assets/Data/Materials/Thread.asset!");
            return;
        }

        // Use SerializedObject for robust saving
        SerializedObject soRecipe = new SerializedObject(recipe);
        soRecipe.FindProperty("recipeId").stringValue = "BGM_01";
        soRecipe.FindProperty("recipeName").stringValue = "Unlock BGM 1";
        soRecipe.FindProperty("requiredMoney").intValue = 300;
        soRecipe.FindProperty("isConsumable").boolValue = true;

        SerializedProperty matReqList = soRecipe.FindProperty("requiredMaterials");
        matReqList.ClearArray();
        matReqList.InsertArrayElementAtIndex(0);
        SerializedProperty element = matReqList.GetArrayElementAtIndex(0);
        element.FindPropertyRelative("material").objectReferenceValue = threadMat;
        element.FindPropertyRelative("amount").intValue = 5;

        soRecipe.ApplyModifiedProperties();

        // Also ensure Material has a proper ID and name
        SerializedObject soMat = new SerializedObject(threadMat);
        soMat.FindProperty("materialId").stringValue = "MAT_THREAD";
        soMat.FindProperty("materialName").stringValue = "Thread";
        soMat.ApplyModifiedProperties();

        AssetDatabase.SaveAssets();
        Debug.Log("Recipe data (V2) initialized with 300G and 5 Thread requirement.");
    }
}
