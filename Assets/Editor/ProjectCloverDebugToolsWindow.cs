using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class ProjectCloverDebugToolsWindow : EditorWindow
{
    private const string WindowTitle = "ProjectClover Debug Tools";
    private const string ScenePath = "Assets/Scenes/MainScene.unity";
    private const string RecipeFolderPath = "Assets/Data/Recipes";
    private const string ThreadMaterialPath = "Assets/Data/Materials/Thread.asset";

    [MenuItem("ProjectClover/Debug Tools")]
    public static void Open()
    {
        var window = GetWindow<ProjectCloverDebugToolsWindow>();
        window.titleContent = new GUIContent(WindowTitle);
        window.minSize = new Vector2(420f, 220f);
        window.Show();
    }

    [MenuItem("ProjectClover/Debug Tools/Setup Craft BGM Recipes")]
    public static void SetupCraftRecipesMenu()
    {
        SetupCraftRecipes();
    }

    [MenuItem("ProjectClover/Debug Tools/Reset Debug Save")]
    public static void ResetDebugSaveMenu()
    {
        ResetDebugSave();
    }

    [MenuItem("ProjectClover/Debug Tools/Setup Recipes And Reset Save")]
    public static void SetupRecipesAndResetSaveMenu()
    {
        SetupCraftRecipes();
        ResetDebugSave();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Craft / Save Debug", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Craft 用の BGM レシピを再生成し、MainScene の MagicManager に反映します。必要ならデバッグ用セーブも初期化できます。", MessageType.Info);

        GUILayout.Space(8f);

        if (GUILayout.Button("Craft BGM項目を再生成", GUILayout.Height(36f)))
        {
            SetupCraftRecipes();
        }

        if (GUILayout.Button("デバッグ用セーブを初期化", GUILayout.Height(36f)))
        {
            ResetDebugSave();
        }

        if (GUILayout.Button("レシピ再生成 + セーブ初期化", GUILayout.Height(40f)))
        {
            SetupCraftRecipes();
            ResetDebugSave();
        }
    }

    private static void SetupCraftRecipes()
    {
        EnsureFolderExists(RecipeFolderPath);

        var threadMaterial = AssetDatabase.LoadAssetAtPath<MaterialData>(ThreadMaterialPath);
        if (threadMaterial == null)
        {
            Debug.LogError("[DebugTools] Thread material was not found.");
            return;
        }

        EnsureThreadMaterial(threadMaterial);

        var recipes = new List<MagicRecipeData>
        {
            CreateOrUpdateRecipe("UnlockBGM1", "magic_unlock_bgm1", "Unlock BGM 1", 150, 2, threadMaterial),
            CreateOrUpdateRecipe("UnlockBGM2", "magic_unlock_bgm2", "Unlock BGM 2", 300, 4, threadMaterial),
            CreateOrUpdateRecipe("UnlockBGM3", "magic_unlock_bgm3", "Unlock BGM 3", 500, 6, threadMaterial)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AssignRecipesToMainScene(recipes);
        Debug.Log("[DebugTools] Craft BGM recipes were regenerated and assigned to MainScene.");
    }

    private static void ResetDebugSave()
    {
        var saveData = new GameSaveData();
        saveData.player.inventory.money = 1000;
        saveData.player.inventory.AddMaterial("MAT_THREAD", 12);
        saveData.player.unlockedMagics.Clear();
        saveData.player.selectedCategoryId = string.Empty;
        saveData.player.selectedTaskId = string.Empty;
        saveData.player.taskCategories.Clear();
        saveData.doll.dollName = "My Doll";
        saveData.doll.currentMood = 1;

        var savePath = Path.Combine(Application.persistentDataPath, "savedata.json");
        var json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
        Debug.Log("[DebugTools] Debug save reset: 1000G / Thread 12");

        SaveManager.Load();

        var rewardManager = Object.FindFirstObjectByType<RewardManager>();
        if (rewardManager != null)
        {
            rewardManager.UpdateInventoryUI();
            EditorUtility.SetDirty(rewardManager);
        }

        var magicUiManager = Object.FindFirstObjectByType<MagicUiManager>();
        if (magicUiManager != null)
        {
            magicUiManager.RefreshUI();
            EditorUtility.SetDirty(magicUiManager);
        }
    }

    private static MagicRecipeData CreateOrUpdateRecipe(string assetName, string recipeId, string recipeName, int requiredMoney, int requiredThread, MaterialData threadMaterial)
    {
        var path = RecipeFolderPath + "/" + assetName + ".asset";
        var recipe = AssetDatabase.LoadAssetAtPath<MagicRecipeData>(path);

        if (recipe == null)
        {
            recipe = CreateInstance<MagicRecipeData>();
            AssetDatabase.CreateAsset(recipe, path);
        }

        recipe.recipeId = recipeId;
        recipe.recipeName = recipeName;
        recipe.requiredMoney = requiredMoney;
        recipe.researchTimeSeconds = 0;
        recipe.isConsumable = false;

        if (recipe.requiredMaterials == null)
        {
            recipe.requiredMaterials = new List<MaterialRequirement>();
        }

        recipe.requiredMaterials.Clear();
        recipe.requiredMaterials.Add(new MaterialRequirement
        {
            material = threadMaterial,
            amount = requiredThread
        });

        EditorUtility.SetDirty(recipe);
        return recipe;
    }

    private static void AssignRecipesToMainScene(List<MagicRecipeData> recipes)
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[DebugTools] MainScene assignment was skipped because Unity is in play mode.");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath);
        var magicManager = Object.FindFirstObjectByType<MagicManager>();
        if (magicManager == null)
        {
            Debug.LogError("[DebugTools] MagicManager was not found in MainScene.");
            return;
        }

        magicManager.allRecipes = recipes;
        EditorUtility.SetDirty(magicManager);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void EnsureThreadMaterial(MaterialData threadMaterial)
    {
        threadMaterial.materialId = "MAT_THREAD";
        threadMaterial.materialName = "Thread";
        EditorUtility.SetDirty(threadMaterial);
    }

    private static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        var parent = "Assets";
        var folderName = folderPath.Replace("Assets/", string.Empty);
        AssetDatabase.CreateFolder(parent, folderName);
    }
}
