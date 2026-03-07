#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WitchTrials.Editor
{
    public sealed class AssetUsageFinderWindow : EditorWindow
    {
        private const string MenuPath = "Tools/WitchTrials/Assets/アセット参照ファインダー";
        private const string WindowTitle = "アセット参照ファインダー";
        private const string ProgressTitle = "アセット参照解析";

        private readonly List<Object> droppedObjects = new();
        private readonly List<TargetResult> results = new();
        private readonly List<string> globalMessages = new();
        private Vector2 scrollPosition;
        private bool analyzed;
        private bool wasCancelled;

        [MenuItem(MenuPath, priority = 30020)]
        private static void Open()
        {
            GetWindow<AssetUsageFinderWindow>(WindowTitle);
        }

        private void OnGUI()
        {
            DrawDropArea();
            DrawInputSummary();
            DrawDroppedObjectList();
            DrawActions();
            DrawGlobalMessages();
            DrawResults();
        }

        private void DrawDropArea()
        {
            Rect dropArea = GUILayoutUtility.GetRect(0f, 72f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "ここにファイル / アセット / フォルダをドラッグ&ドロップ");

            Event evt = Event.current;
            if (!dropArea.Contains(evt.mousePosition))
            {
                return;
            }

            if (evt.type is EventType.DragUpdated or EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    AddDroppedObjects(DragAndDrop.objectReferences);
                }

                evt.Use();
            }
        }

        private void DrawInputSummary()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"ドロップ数: {droppedObjects.Count}", EditorStyles.boldLabel);

                if (GUILayout.Button("選択を追加", GUILayout.Width(120f)))
                {
                    AddDroppedObjects(Selection.objects);
                }
            }
        }

        private void DrawDroppedObjectList()
        {
            if (droppedObjects.Count == 0)
            {
                return;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("ドロップ済み対象", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            for (int i = 0; i < droppedObjects.Count; i++)
            {
                Object obj = droppedObjects[i];
                string path = AssetDatabase.GetAssetPath(obj);
                bool isFolder = !string.IsNullOrWhiteSpace(path) && AssetDatabase.IsValidFolder(path);
                string kindLabel = isFolder ? "フォルダ" : "アセット";

                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(obj, typeof(Object), false, GUILayout.Width(220f));
                }

                EditorGUILayout.LabelField(kindLabel, GUILayout.Width(60f));
                EditorGUILayout.SelectableLabel(path, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

                if (GUILayout.Button("選択", GUILayout.Width(56f)))
                {
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }

                if (GUILayout.Button("削除", GUILayout.Width(56f)))
                {
                    droppedObjects.RemoveAt(i);
                    Repaint();
                    EditorGUILayout.EndHorizontal();
                    continue;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("解析", GUILayout.Height(28f)))
                {
                    Analyze();
                }

                if (GUILayout.Button("クリア", GUILayout.Height(28f), GUILayout.Width(120f)))
                {
                    droppedObjects.Clear();
                    results.Clear();
                    globalMessages.Clear();
                    analyzed = false;
                    wasCancelled = false;
                }
            }
        }

        private void DrawGlobalMessages()
        {
            foreach (string message in globalMessages)
            {
                EditorGUILayout.HelpBox(message, MessageType.Warning);
            }

            if (wasCancelled)
            {
                EditorGUILayout.HelpBox("解析はキャンセルされました。", MessageType.Info);
            }
        }

        private void DrawResults()
        {
            if (!analyzed)
            {
                EditorGUILayout.HelpBox("アセットをドロップして「解析」を押してください。", MessageType.Info);
                return;
            }

            if (results.Count == 0)
            {
                EditorGUILayout.HelpBox("解析可能な対象が見つかりませんでした。", MessageType.Info);
                return;
            }

            int deleteResultIndex = -1;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < results.Count; i++)
            {
                TargetResult result = results[i];
                EditorGUILayout.BeginVertical("box");

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField("Target", result.TargetObject, typeof(Object), false);
                }

                EditorGUILayout.LabelField($"種類: {result.KindLabel}");
                DrawPathRow(result.TargetPath, result.TargetObject);

                if (result.IsUnused)
                {
                    EditorGUILayout.HelpBox("未使用: このアセットは参照されていません。", MessageType.Warning);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("未使用素材をDelete", GUILayout.Width(180f)))
                        {
                            if (TryDeleteUnusedAsset(result))
                            {
                                deleteResultIndex = i;
                            }
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField($"参照数: {result.Usages.Count}", EditorStyles.boldLabel);
                    foreach (UsageEntry usage in result.Usages)
                    {
                        EditorGUILayout.BeginHorizontal();
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUILayout.ObjectField(usage.ReferrerObject, typeof(Object), false);
                        }

                        if (GUILayout.Button(usage.ReferrerPath, EditorStyles.linkLabel))
                        {
                            if (usage.ReferrerObject)
                            {
                                Selection.activeObject = usage.ReferrerObject;
                                EditorGUIUtility.PingObject(usage.ReferrerObject);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (result.Errors.Count > 0)
                {
                    foreach (string error in result.Errors)
                    {
                        EditorGUILayout.HelpBox(error, MessageType.Warning);
                    }
                }

                EditorGUILayout.EndVertical();

                if (deleteResultIndex >= 0)
                {
                    break;
                }
            }
            EditorGUILayout.EndScrollView();

            if (deleteResultIndex >= 0)
            {
                results.RemoveAt(deleteResultIndex);
                Repaint();
            }
        }

        private bool TryDeleteUnusedAsset(TargetResult result)
        {
            if (!result.IsUnused || string.IsNullOrWhiteSpace(result.TargetPath))
            {
                return false;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "未使用素材の削除確認",
                $"次の未使用アセットを削除します。\n\n{result.TargetPath}\n\nこの操作は取り消せません。実行しますか？",
                "Delete",
                "キャンセル");

            if (!confirmed)
            {
                return false;
            }

            string targetPath = result.TargetPath;
            bool deleted = AssetDatabase.DeleteAsset(targetPath);
            if (!deleted)
            {
                globalMessages.Add($"削除に失敗しました: {targetPath}");
                return false;
            }

            RemoveDroppedObjectByPath(targetPath);
            RemoveUsageEntriesByReferrerPath(targetPath);

            globalMessages.Add($"削除しました: {targetPath}");
            AssetDatabase.Refresh();
            return true;
        }

        private void RemoveDroppedObjectByPath(string path)
        {
            droppedObjects.RemoveAll(obj => AssetDatabase.GetAssetPath(obj) == path);
        }

        private void RemoveUsageEntriesByReferrerPath(string path)
        {
            foreach (TargetResult result in results)
            {
                result.Usages.RemoveAll(usage => usage.ReferrerPath == path);
            }
        }

        private static void DrawPathRow(string path, Object targetObject)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel(path, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

            if (GUILayout.Button("選択", GUILayout.Width(80f)))
            {
                if (targetObject)
                {
                    Selection.activeObject = targetObject;
                    EditorGUIUtility.PingObject(targetObject);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AddDroppedObjects(IEnumerable<Object> objects)
        {
            foreach (Object obj in objects.Where(static o => o != null))
            {
                if (droppedObjects.Contains(obj))
                {
                    continue;
                }

                droppedObjects.Add(obj);
            }

            Repaint();
        }

        private void Analyze()
        {
            results.Clear();
            globalMessages.Clear();
            analyzed = true;
            wasCancelled = false;

            if (droppedObjects.Count == 0)
            {
                globalMessages.Add("ドロップされた対象がありません。アセットをドロップしてから解析してください。");
                return;
            }

            Dictionary<string, Object> targetByPath = BuildTargetsFromDroppedObjects();
            if (targetByPath.Count == 0)
            {
                globalMessages.Add("ドロップ対象から有効なアセットを取得できませんでした。");
                return;
            }

            string[] candidatePaths = AssetDatabase
                .GetAllAssetPaths()
                .Where(static path =>
                    path.StartsWith("Assets/", StringComparison.Ordinal) &&
                    !path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) &&
                    !AssetDatabase.IsValidFolder(path))
                .ToArray();

            string[] orderedTargetPaths = targetByPath.Keys.OrderBy(static p => p, StringComparer.Ordinal).ToArray();
            int totalSteps = Math.Max(1, orderedTargetPaths.Length * Math.Max(1, candidatePaths.Length));
            int currentStep = 0;
            bool cancelled = false;

            try
            {
                foreach (string targetPath in orderedTargetPaths)
                {
                    if (cancelled)
                    {
                        break;
                    }

                    Object targetObject = targetByPath[targetPath];
                    var uniqueReferrerPaths = new HashSet<string>(StringComparer.Ordinal);
                    var errors = new List<string>();

                    foreach (string candidatePath in candidatePaths)
                    {
                        currentStep++;
                        float progress = currentStep / (float)totalSteps;
                        bool cancelPressed = EditorUtility.DisplayCancelableProgressBar(
                            ProgressTitle,
                            $"解析中: {targetPath}\n確認中: {candidatePath}",
                            progress);
                        if (cancelPressed)
                        {
                            cancelled = true;
                            break;
                        }

                        if (candidatePath == targetPath)
                        {
                            continue;
                        }

                        try
                        {
                            string[] dependencies = AssetDatabase.GetDependencies(candidatePath, false);
                            if (Array.IndexOf(dependencies, targetPath) >= 0)
                            {
                                uniqueReferrerPaths.Add(candidatePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = $"'{candidatePath}' の確認中にエラーが発生しました: {ex.Message}";
                            if (!errors.Contains(message))
                            {
                                errors.Add(message);
                            }
                        }
                    }

                    List<UsageEntry> usages = uniqueReferrerPaths
                        .OrderBy(static p => p, StringComparer.Ordinal)
                        .Select(path => new UsageEntry
                        {
                            ReferrerPath = path,
                            ReferrerObject = AssetDatabase.LoadMainAssetAtPath(path)
                        })
                        .ToList();

                    results.Add(new TargetResult
                    {
                        TargetObject = targetObject,
                        TargetPath = targetPath,
                        KindLabel = GetKindLabel(targetPath, targetObject),
                        Usages = usages,
                        Errors = errors
                    });
                }

                if (cancelled)
                {
                    wasCancelled = true;
                    globalMessages.Add("解析をユーザー操作で中断しました。");
                }

                results.Sort(static (a, b) =>
                {
                    int kindCompare = StringComparer.OrdinalIgnoreCase.Compare(a.KindLabel, b.KindLabel);
                    if (kindCompare != 0)
                    {
                        return kindCompare;
                    }

                    return StringComparer.Ordinal.Compare(a.TargetPath, b.TargetPath);
                });
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static string GetKindLabel(string path, Object obj)
        {
            if (obj is SceneAsset)
            {
                return "Scene";
            }

            if (obj is GameObject)
            {
                string lowerPath = path.ToLowerInvariant();
                if (lowerPath.EndsWith(".prefab", StringComparison.Ordinal))
                {
                    return "Prefab";
                }

                return "GameObject Asset";
            }

            if (obj is Material)
            {
                return "Material";
            }

            if (obj is Texture or Texture2D)
            {
                return "Texture";
            }

            if (obj is AudioClip)
            {
                return "Audio";
            }

            if (obj is AnimationClip)
            {
                return "Animation";
            }

            if (obj is MonoScript)
            {
                return "Script";
            }

            if (obj is Shader)
            {
                return "Shader";
            }

            if (obj is ScriptableObject)
            {
                return "ScriptableObject";
            }

            return obj != null ? obj.GetType().Name : "Unknown";
        }

        private Dictionary<string, Object> BuildTargetsFromDroppedObjects()
        {
            var targetByPath = new Dictionary<string, Object>(StringComparer.Ordinal);

            foreach (Object droppedObject in droppedObjects)
            {
                string droppedPath = AssetDatabase.GetAssetPath(droppedObject);
                if (string.IsNullOrWhiteSpace(droppedPath))
                {
                    globalMessages.Add($"アセットパスを解決できないためスキップしました: {droppedObject.name}");
                    continue;
                }

                if (AssetDatabase.IsValidFolder(droppedPath))
                {
                    AddTargetsFromFolder(droppedPath, targetByPath);
                    continue;
                }

                if (droppedPath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!targetByPath.ContainsKey(droppedPath))
                {
                    targetByPath.Add(droppedPath, droppedObject);
                }
            }

            return targetByPath;
        }

        private static void AddTargetsFromFolder(string folderPath, Dictionary<string, Object> targetByPath)
        {
            string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { folderPath });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    continue;
                }

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    continue;
                }

                if (assetPath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (targetByPath.ContainsKey(assetPath))
                {
                    continue;
                }

                targetByPath.Add(assetPath, AssetDatabase.LoadMainAssetAtPath(assetPath));
            }
        }

        [Serializable]
        private sealed class TargetResult
        {
            public Object TargetObject;
            public string TargetPath = string.Empty;
            public string KindLabel = string.Empty;
            public List<UsageEntry> Usages = new();
            public List<string> Errors = new();
            public bool IsUnused => Usages.Count == 0;
        }

        [Serializable]
        private sealed class UsageEntry
        {
            public Object ReferrerObject;
            public string ReferrerPath = string.Empty;
        }
    }
}
#endif
