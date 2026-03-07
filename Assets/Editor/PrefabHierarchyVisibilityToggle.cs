using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws a visibility checkbox in the Hierarchy window.
/// The checkbox toggles the GameObject active state and supports Undo.
/// </summary>
[InitializeOnLoad]
internal static class PrefabHierarchyVisibilityToggle
{
    private const float ToggleWidth = 16f;
    private const float RightPadding = 2f;
    private const string PrefKeyEnabled = "PrefabHierarchyVisibilityToggle.Enabled";
    private const string PrefKeyPrefabOnly = "PrefabHierarchyVisibilityToggle.PrefabOnly";
    private const string MenuEnabled = "Tools/Hierarchy/Prefab Visibility Checkbox/Enabled";
    private const string MenuPrefabOnly = "Tools/Hierarchy/Prefab Visibility Checkbox/Prefab Instances Only";

    static PrefabHierarchyVisibilityToggle()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
    }

    [MenuItem(MenuEnabled)]
    private static void ToggleFeature()
    {
        var next = !IsEnabled();
        EditorPrefs.SetBool(PrefKeyEnabled, next);
        EditorApplication.RepaintHierarchyWindow();
    }

    [MenuItem(MenuEnabled, true)]
    private static bool ToggleFeatureValidate()
    {
        Menu.SetChecked(MenuEnabled, IsEnabled());
        return true;
    }

    [MenuItem(MenuPrefabOnly)]
    private static void TogglePrefabOnly()
    {
        var next = !IsPrefabOnly();
        EditorPrefs.SetBool(PrefKeyPrefabOnly, next);
        EditorApplication.RepaintHierarchyWindow();
    }

    [MenuItem(MenuPrefabOnly, true)]
    private static bool TogglePrefabOnlyValidate()
    {
        Menu.SetChecked(MenuPrefabOnly, IsPrefabOnly());
        return true;
    }

    private static bool IsEnabled() => EditorPrefs.GetBool(PrefKeyEnabled, true);
    private static bool IsPrefabOnly() => EditorPrefs.GetBool(PrefKeyPrefabOnly, false);

    private static void OnHierarchyWindowItemOnGUI(int instanceId, Rect selectionRect)
    {
        if (!IsEnabled()) return;

        var obj = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
        if (!obj) return;
        if (IsPrefabOnly() && !PrefabUtility.IsAnyPrefabInstanceRoot(obj)) return;

        var toggleRect = new Rect(
            selectionRect.xMax - ToggleWidth - RightPadding,
            selectionRect.y + (selectionRect.height - ToggleWidth) * 0.5f,
            ToggleWidth,
            ToggleWidth);

        EditorGUI.BeginChangeCheck();
        var nextActive = GUI.Toggle(toggleRect, obj.activeSelf, GUIContent.none);
        if (!EditorGUI.EndChangeCheck()) return;

        Undo.RecordObject(obj, "Toggle Prefab Visibility");
        obj.SetActive(nextActive);
        EditorUtility.SetDirty(obj);
    }
}
