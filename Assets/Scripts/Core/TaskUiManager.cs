using System;
using UnityEngine;
using UnityEngine.UI;

public class TaskUiManager : MonoBehaviour
{
    private const int MaxCategoryLength = 15;
    private const string GeneratedRootName = "TaskUiRoot";

    private UIManager _uiManager;
    private Font _font;

    private RectTransform _generatedRoot;
    private RectTransform _listContainer;
    private InputField _taskInputField;
    private Text _addButtonText;
    private Text _currentPathText;
    private Button _backButton;
    private GameObject _detailPanel;
    private Text _detailText;
    private GameObject _confirmDialog;
    private Text _confirmText;
    private Action _pendingConfirmAction;

    private string _currentCategoryId = string.Empty;

    public static TaskUiManager EnsureSetup(GameObject overlay, UIManager uiManager = null)
    {
        if (overlay == null) return null;

        var manager = overlay.GetComponent<TaskUiManager>();
        if (manager == null)
        {
            manager = overlay.AddComponent<TaskUiManager>();
        }

        manager._uiManager = uiManager;
        manager.InitializeIfNeeded();
        return manager;
    }

    private void Awake()
    {
        InitializeIfNeeded();
    }

    private void OnEnable()
    {
        InitializeIfNeeded();
        RefreshUI();
    }

    private void InitializeIfNeeded()
    {
        if (_generatedRoot != null) return;

        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        HideLegacyOverlayChildren();
        BuildLayout();
        RefreshUI();
    }

    private void HideLegacyOverlayChildren()
    {
        foreach (Transform child in transform)
        {
            if (child.name == GeneratedRootName) continue;
            child.gameObject.SetActive(false);
        }
    }

    private void BuildLayout()
    {
        _generatedRoot = CreateUIObject(GeneratedRootName, transform);
        Stretch(_generatedRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        _generatedRoot.gameObject.AddComponent<Image>().color = new Color(0.96f, 0.91f, 0.83f, 1f);

        var header = CreateUIObject("Header", _generatedRoot);
        Stretch(header, new Vector2(0f, 0.88f), new Vector2(1f, 1f), new Vector2(32f, -24f), new Vector2(-32f, -24f));

        _currentPathText = CreateText("CurrentPath", header, "TODO: 大項目", 34, TextAnchor.MiddleLeft, new Color(0.22f, 0.16f, 0.1f));
        Stretch(_currentPathText.rectTransform, new Vector2(0f, 0f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);

        _backButton = CreateButton("Btn_Back", header, "戻る", new Color(0.55f, 0.39f, 0.24f), OnBackClicked);
        Stretch((RectTransform)_backButton.transform, new Vector2(0.72f, 0.18f), new Vector2(0.86f, 0.82f), Vector2.zero, new Vector2(-12f, 0f));

        var closeButton = CreateButton("Btn_CloseTodoGenerated", header, "閉じる", new Color(0.75f, 0.31f, 0.27f), OnCloseClicked);
        Stretch((RectTransform)closeButton.transform, new Vector2(0.86f, 0.18f), new Vector2(1f, 0.82f), new Vector2(12f, 0f), Vector2.zero);

        var inputRow = CreateUIObject("InputRow", _generatedRoot);
        Stretch(inputRow, new Vector2(0f, 0.77f), new Vector2(1f, 0.87f), new Vector2(32f, 0f), new Vector2(-32f, 0f));

        _taskInputField = CreateInputField("TaskInput", inputRow, "名前を入力");
        Stretch((RectTransform)_taskInputField.transform, new Vector2(0f, 0f), new Vector2(0.72f, 1f), Vector2.zero, new Vector2(-12f, 0f));

        var addButton = CreateButton("Btn_Add", inputRow, "大項目を追加", new Color(0.28f, 0.53f, 0.3f), OnAddClicked);
        Stretch((RectTransform)addButton.transform, new Vector2(0.72f, 0f), new Vector2(1f, 1f), new Vector2(12f, 0f), Vector2.zero);
        _addButtonText = addButton.GetComponentInChildren<Text>();

        var listViewport = CreateUIObject("ListViewport", _generatedRoot);
        Stretch(listViewport, new Vector2(0f, 0f), new Vector2(1f, 0.75f), new Vector2(32f, 32f), new Vector2(-32f, -32f));
        listViewport.gameObject.AddComponent<Image>().color = new Color(1f, 0.98f, 0.94f, 0.92f);
        listViewport.gameObject.AddComponent<Mask>().showMaskGraphic = true;

        var scrollRect = listViewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;

        var content = CreateUIObject("Content", listViewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 16f;
        layout.padding = new RectOffset(0, 0, 8, 8);

        var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = listViewport;
        scrollRect.content = content;
        _listContainer = content;

        BuildDetailPanel();
        BuildConfirmDialog();
    }

    private void BuildDetailPanel()
    {
        _detailPanel = CreateUIObject("TaskDetailPanel", _generatedRoot).gameObject;
        var rt = (RectTransform)_detailPanel.transform;
        Stretch(rt, new Vector2(0.16f, 0.22f), new Vector2(0.84f, 0.74f), Vector2.zero, Vector2.zero);
        _detailPanel.AddComponent<Image>().color = new Color(0.17f, 0.13f, 0.1f, 0.97f);

        _detailText = CreateText("DetailText", rt, string.Empty, 24, TextAnchor.MiddleCenter, Color.white);
        Stretch(_detailText.rectTransform, new Vector2(0f, 0.22f), new Vector2(1f, 1f), new Vector2(28f, 20f), new Vector2(-28f, -20f));

        var closeButton = CreateButton("Btn_CloseDetail", rt, "閉じる", new Color(0.74f, 0.31f, 0.29f), () => _detailPanel.SetActive(false));
        Stretch((RectTransform)closeButton.transform, new Vector2(0.32f, 0.06f), new Vector2(0.68f, 0.17f), Vector2.zero, Vector2.zero);

        _detailPanel.SetActive(false);
    }

    private void BuildConfirmDialog()
    {
        _confirmDialog = CreateUIObject("ConfirmDialog", _generatedRoot).gameObject;
        var rt = (RectTransform)_confirmDialog.transform;
        Stretch(rt, new Vector2(0.22f, 0.3f), new Vector2(0.78f, 0.62f), Vector2.zero, Vector2.zero);
        _confirmDialog.AddComponent<Image>().color = new Color(0.2f, 0.14f, 0.12f, 0.98f);

        _confirmText = CreateText("ConfirmText", rt, string.Empty, 24, TextAnchor.MiddleCenter, Color.white);
        Stretch(_confirmText.rectTransform, new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.88f), Vector2.zero, Vector2.zero);

        var cancelButton = CreateButton("Btn_Cancel", rt, "キャンセル", new Color(0.45f, 0.42f, 0.38f), HideConfirmDialog);
        Stretch((RectTransform)cancelButton.transform, new Vector2(0.12f, 0.1f), new Vector2(0.42f, 0.26f), Vector2.zero, Vector2.zero);

        var confirmButton = CreateButton("Btn_ConfirmDelete", rt, "削除する", new Color(0.75f, 0.28f, 0.24f), ConfirmDelete);
        Stretch((RectTransform)confirmButton.transform, new Vector2(0.58f, 0.1f), new Vector2(0.88f, 0.26f), Vector2.zero, Vector2.zero);

        _confirmDialog.SetActive(false);
    }

    public void RefreshUI()
    {
        if (_listContainer == null) return;

        foreach (Transform child in _listContainer)
        {
            Destroy(child.gameObject);
        }

        var player = SaveManager.Current.player;
        if (!string.IsNullOrEmpty(_currentCategoryId) && player.taskCategories.Find(c => c.categoryId == _currentCategoryId) == null)
        {
            _currentCategoryId = string.Empty;
        }

        if (string.IsNullOrEmpty(_currentCategoryId))
        {
            _currentPathText.text = "TODO: 大項目";
            _addButtonText.text = "大項目を追加";
            _backButton.gameObject.SetActive(false);

            foreach (var category in player.taskCategories)
            {
                CreateCategoryRow(category);
            }
        }
        else
        {
            var category = player.taskCategories.Find(c => c.categoryId == _currentCategoryId);
            if (category == null)
            {
                _currentCategoryId = string.Empty;
                RefreshUI();
                return;
            }

            _currentPathText.text = "TODO: " + category.categoryName;
            _addButtonText.text = "小項目を追加";
            _backButton.gameObject.SetActive(true);

            for (var i = 0; i < category.tasks.Count; i++)
            {
                CreateTaskRow(category, category.tasks[i], i + 1);
            }
        }
    }

    private void CreateCategoryRow(TaskCategory category)
    {
        var row = CreateUIObject("Category_" + category.categoryId, _listContainer);
        row.gameObject.AddComponent<Image>().color = new Color(0.96f, 0.88f, 0.75f, 1f);
        var layoutElement = row.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 84f;

        var selectButton = CreateButton("Btn_SelectCategory", row, category.categoryName, new Color(0.85f, 0.67f, 0.44f), () => OnCategorySelected(category.categoryId));
        Stretch((RectTransform)selectButton.transform, new Vector2(0.02f, 0.1f), new Vector2(0.76f, 0.9f), Vector2.zero, Vector2.zero);
        var text = selectButton.GetComponentInChildren<Text>();
        text.fontSize = 28;
        text.color = new Color(0.2f, 0.12f, 0.08f);

        var deleteButton = CreateButton("Btn_DeleteCategory", row, "削除", new Color(0.72f, 0.3f, 0.28f), () => PromptDeleteCategory(category));
        Stretch((RectTransform)deleteButton.transform, new Vector2(0.8f, 0.16f), new Vector2(0.96f, 0.84f), Vector2.zero, Vector2.zero);
    }

    private void CreateTaskRow(TaskCategory category, TaskItem task, int index)
    {
        var row = CreateUIObject("TaskRow_" + index, _listContainer);
        row.gameObject.AddComponent<Image>().color = new Color(1f, 0.97f, 0.9f, 1f);
        var layoutElement = row.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 96f;

        CreateToggle(task, row);

        var taskLabel = CreateButton("TaskName", row, GetTaskDisplayName(task, index), Color.clear, () => SelectTask(category, task));
        Stretch((RectTransform)taskLabel.transform, new Vector2(0.16f, 0.12f), new Vector2(0.64f, 0.88f), Vector2.zero, Vector2.zero);
        var taskText = taskLabel.GetComponentInChildren<Text>();
        taskText.alignment = TextAnchor.MiddleLeft;
        taskText.fontSize = 26;
        taskText.color = SaveManager.Current.player.selectedTaskId == task.taskId ? new Color(0.16f, 0.41f, 0.86f) : new Color(0.18f, 0.15f, 0.12f);

        var detailButton = CreateButton("Btn_Detail", row, "詳細", new Color(0.36f, 0.47f, 0.7f), () => ShowTaskDetail(task, index));
        Stretch((RectTransform)detailButton.transform, new Vector2(0.66f, 0.18f), new Vector2(0.8f, 0.82f), Vector2.zero, Vector2.zero);

        var deleteButton = CreateButton("Btn_Delete", row, "削除", new Color(0.72f, 0.3f, 0.28f), () => PromptDeleteTask(category, task, index));
        Stretch((RectTransform)deleteButton.transform, new Vector2(0.82f, 0.18f), new Vector2(0.96f, 0.82f), Vector2.zero, Vector2.zero);
    }

    private void CreateToggle(TaskItem task, RectTransform parent)
    {
        var toggleObject = CreateUIObject("Toggle", parent).gameObject;
        var toggleRect = (RectTransform)toggleObject.transform;
        Stretch(toggleRect, new Vector2(0.03f, 0.2f), new Vector2(0.12f, 0.8f), Vector2.zero, Vector2.zero);

        var toggle = toggleObject.AddComponent<Toggle>();

        var background = CreateUIObject("Background", toggleRect).gameObject;
        Stretch((RectTransform)background.transform, new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f), Vector2.zero, Vector2.zero);
        var backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = Color.white;

        var checkmark = CreateText("Checkmark", (RectTransform)background.transform, "✓", 34, TextAnchor.MiddleCenter, new Color(0.18f, 0.55f, 0.21f));
        Stretch(checkmark.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        toggle.graphic = checkmark;
        toggle.targetGraphic = backgroundImage;
        toggle.isOn = task.isCompleted;
        checkmark.enabled = task.isCompleted;
        toggle.onValueChanged.AddListener(value =>
        {
            task.isCompleted = value;
            checkmark.enabled = value;
            SaveManager.Save();
        });
    }

    private void OnCategorySelected(string categoryId)
    {
        _currentCategoryId = categoryId;
        SaveManager.Current.player.selectedCategoryId = categoryId;
        SaveManager.Current.player.selectedTaskId = string.Empty;
        HideConfirmDialog();
        SaveManager.Save();
        RefreshUI();
    }

    private void OnBackClicked()
    {
        _currentCategoryId = string.Empty;
        HideConfirmDialog();
        RefreshUI();
    }

    private void OnCloseClicked()
    {
        _detailPanel.SetActive(false);
        HideConfirmDialog();
        if (_uiManager != null) _uiManager.HideTodoOverlay();
        else gameObject.SetActive(false);
    }

    private void OnAddClicked()
    {
        var player = SaveManager.Current.player;
        var input = _taskInputField.text == null ? string.Empty : _taskInputField.text.Trim();

        if (string.IsNullOrEmpty(_currentCategoryId))
        {
            if (string.IsNullOrEmpty(input)) return;
            if (input.Length > MaxCategoryLength) input = input.Substring(0, MaxCategoryLength);
            player.taskCategories.Add(new TaskCategory(input));
        }
        else
        {
            var category = player.taskCategories.Find(c => c.categoryId == _currentCategoryId);
            if (category == null) return;

            if (string.IsNullOrEmpty(input))
            {
                input = (category.tasks.Count + 1).ToString();
            }

            category.tasks.Add(new TaskItem(input));
        }

        SaveManager.Save();
        _taskInputField.text = string.Empty;
        RefreshUI();
    }

    private void SelectTask(TaskCategory category, TaskItem task)
    {
        var player = SaveManager.Current.player;
        player.selectedCategoryId = category.categoryId;
        player.selectedTaskId = task.taskId;
        HideConfirmDialog();
        SaveManager.Save();
        RefreshUI();
    }

    private void PromptDeleteTask(TaskCategory category, TaskItem task, int index)
    {
        ShowConfirmDialog("「" + GetTaskDisplayName(task, index) + "」を削除しますか？", () => DeleteTask(category, task));
    }

    private void PromptDeleteCategory(TaskCategory category)
    {
        ShowConfirmDialog("「" + category.categoryName + "」と中の項目を削除しますか？", () => DeleteCategory(category));
    }

    private void ShowConfirmDialog(string message, Action onConfirm)
    {
        if (_confirmDialog == null || _confirmText == null) return;
        _pendingConfirmAction = onConfirm;
        _confirmText.text = message;
        _confirmDialog.SetActive(true);
    }

    private void HideConfirmDialog()
    {
        _pendingConfirmAction = null;
        if (_confirmDialog != null)
        {
            _confirmDialog.SetActive(false);
        }
    }

    private void ConfirmDelete()
    {
        var action = _pendingConfirmAction;
        HideConfirmDialog();
        if (action != null)
        {
            action.Invoke();
        }
    }

    private void DeleteTask(TaskCategory category, TaskItem task)
    {
        if (category == null || task == null) return;

        var player = SaveManager.Current.player;
        if (!category.tasks.Remove(task)) return;

        if (player.selectedTaskId == task.taskId)
        {
            player.selectedTaskId = string.Empty;
            if (player.selectedCategoryId == category.categoryId)
            {
                player.selectedCategoryId = string.Empty;
            }
        }

        _detailPanel.SetActive(false);
        SaveManager.Save();
        RefreshUI();
    }

    private void DeleteCategory(TaskCategory category)
    {
        if (category == null) return;

        var player = SaveManager.Current.player;
        if (!player.taskCategories.Remove(category)) return;

        bool hadSelectedTaskInCategory = category.tasks.Exists(task => task.taskId == player.selectedTaskId);
        if (_currentCategoryId == category.categoryId)
        {
            _currentCategoryId = string.Empty;
        }

        if (player.selectedCategoryId == category.categoryId || hadSelectedTaskInCategory)
        {
            player.selectedCategoryId = string.Empty;
            player.selectedTaskId = string.Empty;
        }

        _detailPanel.SetActive(false);
        SaveManager.Save();
        RefreshUI();
    }

    private void ShowTaskDetail(TaskItem task, int index)
    {
        var totalSec = Mathf.RoundToInt(task.timeSpentSeconds);
        var hours = totalSec / 3600;
        var minutes = (totalSec % 3600) / 60;
        var seconds = totalSec % 60;

        var displayName = GetTaskDisplayName(task, index);
        var status = task.isCompleted ? "完了" : "未完了";
        var timeText = hours > 0 ? hours + "時間 " + minutes + "分 " + seconds + "秒" : minutes > 0 ? minutes + "分 " + seconds + "秒" : seconds + "秒";

        _detailText.text = "【" + displayName + "】\n\n累計作業時間\n" + timeText + "\n\n状態: " + status;
        _detailPanel.SetActive(true);
        HideConfirmDialog();
    }

    private static string GetTaskDisplayName(TaskItem task, int index)
    {
        return string.IsNullOrWhiteSpace(task.taskName) ? index.ToString() : task.taskName;
    }

    private Button CreateButton(string objectName, Transform parent, string label, Color backgroundColor, UnityEngine.Events.UnityAction onClick)
    {
        var buttonObject = CreateUIObject(objectName, parent).gameObject;
        var image = buttonObject.AddComponent<Image>();
        image.color = backgroundColor;

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var text = CreateText("Text", (RectTransform)button.transform, label, 24, TextAnchor.MiddleCenter, Color.white);
        Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f));

        return button;
    }

    private InputField CreateInputField(string objectName, Transform parent, string placeholder)
    {
        var inputObject = CreateUIObject(objectName, parent).gameObject;
        inputObject.AddComponent<Image>().color = Color.white;

        var inputField = inputObject.AddComponent<InputField>();

        var placeholderText = CreateText("Placeholder", (RectTransform)inputObject.transform, placeholder, 24, TextAnchor.MiddleLeft, new Color(0.52f, 0.48f, 0.42f));
        Stretch(placeholderText.rectTransform, Vector2.zero, Vector2.one, new Vector2(20f, 10f), new Vector2(-20f, -10f));
        placeholderText.fontStyle = FontStyle.Italic;

        var text = CreateText("Text", (RectTransform)inputObject.transform, string.Empty, 24, TextAnchor.MiddleLeft, new Color(0.18f, 0.15f, 0.12f));
        Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(20f, 10f), new Vector2(-20f, -10f));

        inputField.textComponent = text;
        inputField.placeholder = placeholderText;
        inputField.lineType = InputField.LineType.SingleLine;
        return inputField;
    }

    private Text CreateText(string objectName, Transform parent, string value, int fontSize, TextAnchor alignment, Color color)
    {
        var textObject = CreateUIObject(objectName, parent).gameObject;
        var text = textObject.AddComponent<Text>();
        text.font = _font;
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private static RectTransform CreateUIObject(string objectName, Transform parent)
    {
        var go = new GameObject(objectName, typeof(RectTransform));
        var rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.localScale = Vector3.one;
        return rectTransform;
    }

    private static void Stretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
    }
}
