using UnityEngine;
using UnityEngine.UI;

public class TaskUiManager : MonoBehaviour {
    [Header("UI Containers")]
    public Transform listContainer;
    public GameObject categoryPrefab;
    public GameObject subTaskPrefab;

    [Header("Input Controls")]
    public InputField taskInputField;
    public Button addBtn;
    public Text addBtnText;

    [Header("Context")]
    public Text currentPathText;
    public Button backBtn;

    private GameObject _detailPanel;
    private Text _detailText;
    private string _currentCategoryId = "";

    void Start() {
        BuildDetailPanel();
        RefreshUI();
        backBtn.onClick.AddListener(OnBackClicked);
        addBtn.onClick.AddListener(OnAddClicked);
    }

    void BuildDetailPanel() {
        _detailPanel = new GameObject("TaskDetailPanel");
        _detailPanel.transform.SetParent(transform.parent, false);

        RectTransform rt = _detailPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0.3f);
        rt.anchorMax = new Vector2(0.9f, 0.7f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        _detailPanel.AddComponent<CanvasRenderer>();
        _detailPanel.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        // Detail text
        GameObject textGo = new GameObject("DetailText");
        textGo.transform.SetParent(_detailPanel.transform, false);
        _detailText = textGo.AddComponent<Text>();
        _detailText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        _detailText.color = Color.white;
        _detailText.fontSize = 18;
        _detailText.alignment = TextAnchor.MiddleCenter;
        _detailText.rectTransform.anchorMin = new Vector2(0, 0.2f);
        _detailText.rectTransform.anchorMax = Vector2.one;
        _detailText.rectTransform.offsetMin = _detailText.rectTransform.offsetMax = Vector2.zero;

        // Close button
        GameObject closeBtnGo = new GameObject("Btn_CloseDetail");
        closeBtnGo.transform.SetParent(_detailPanel.transform, false);
        RectTransform closeBtnRT = closeBtnGo.AddComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(0.3f, 0.05f);
        closeBtnRT.anchorMax = new Vector2(0.7f, 0.18f);
        closeBtnRT.offsetMin = closeBtnRT.offsetMax = Vector2.zero;
        closeBtnGo.AddComponent<Image>().color = new Color(0.8f, 0.3f, 0.3f);
        Button closeBtn = closeBtnGo.AddComponent<Button>();
        closeBtn.onClick.AddListener(() => _detailPanel.SetActive(false));

        GameObject closeTxtGo = new GameObject("Text");
        closeTxtGo.transform.SetParent(closeBtnGo.transform, false);
        Text closeTxt = closeTxtGo.AddComponent<Text>();
        closeTxt.text = "閉じる";
        closeTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        closeTxt.color = Color.white;
        closeTxt.alignment = TextAnchor.MiddleCenter;
        closeTxt.rectTransform.anchorMin = Vector2.zero;
        closeTxt.rectTransform.anchorMax = Vector2.one;
        closeTxt.rectTransform.offsetMin = closeTxt.rectTransform.offsetMax = Vector2.zero;

        _detailPanel.SetActive(false);
    }

    public void RefreshUI() {
        foreach (Transform child in listContainer) {
            Destroy(child.gameObject);
        }

        var player = SaveManager.Current.player;

        if (string.IsNullOrEmpty(_currentCategoryId)) {
            currentPathText.text = "TODO: マスタ項目";
            addBtnText.text = "大項目を追加";
            backBtn.gameObject.SetActive(false);

            foreach (var category in player.taskCategories) {
                var go = Instantiate(categoryPrefab, listContainer);
                go.GetComponentInChildren<Text>().text = category.categoryName;
                string capturedId = category.categoryId;
                go.GetComponent<Button>().onClick.AddListener(() => OnCategorySelected(capturedId));
            }
        } else {
            var category = player.taskCategories.Find(c => c.categoryId == _currentCategoryId);
            if (category == null) { _currentCategoryId = ""; RefreshUI(); return; }

            currentPathText.text = $"TODO: {category.categoryName}";
            addBtnText.text = "小項目を追加";
            backBtn.gameObject.SetActive(true);

            foreach (var task in category.tasks) {
                var go = Instantiate(subTaskPrefab, listContainer);
                var checkbox = go.GetComponentInChildren<Toggle>();
                var taskNameText = go.transform.Find("Label").GetComponent<Text>();
                var detailBtn = go.transform.Find("Btn_Detail").GetComponent<Button>();

                taskNameText.text = task.taskName;
                checkbox.isOn = task.isCompleted;
                taskNameText.color = (player.selectedTaskId == task.taskId) ? Color.blue : Color.black;

                TaskItem capturedTask = task;
                checkbox.onValueChanged.AddListener((val) => {
                    capturedTask.isCompleted = val;
                    SaveManager.Save();
                });

                var labelBtn = taskNameText.gameObject.GetComponent<Button>() ?? taskNameText.gameObject.AddComponent<Button>();
                labelBtn.onClick.AddListener(() => {
                    player.selectedTaskId = capturedTask.taskId;
                    player.selectedCategoryId = _currentCategoryId;
                    SaveManager.Save();
                    RefreshUI();
                });

                detailBtn.onClick.AddListener(() => ShowTaskDetail(capturedTask));
            }
        }
    }

    void OnCategorySelected(string id) {
        _currentCategoryId = id;
        SaveManager.Current.player.selectedCategoryId = id;
        SaveManager.Current.player.selectedTaskId = "";
        SaveManager.Save();
        RefreshUI();
    }

    void OnBackClicked() {
        _currentCategoryId = "";
        RefreshUI();
    }

    void OnAddClicked() {
        string input = taskInputField.text;
        if (string.IsNullOrEmpty(input)) {
            if (!string.IsNullOrEmpty(_currentCategoryId)) {
                var cat = SaveManager.Current.player.taskCategories.Find(c => c.categoryId == _currentCategoryId);
                input = (cat.tasks.Count + 1).ToString();
            } else {
                return;
            }
        }

        if (input.Length > 15) input = input.Substring(0, 15);

        if (string.IsNullOrEmpty(_currentCategoryId)) {
            SaveManager.Current.player.taskCategories.Add(new TaskCategory(input));
        } else {
            var cat = SaveManager.Current.player.taskCategories.Find(c => c.categoryId == _currentCategoryId);
            cat.tasks.Add(new TaskItem(input));
        }

        SaveManager.Save();
        taskInputField.text = "";
        RefreshUI();
    }

    void ShowTaskDetail(TaskItem task) {
        if (_detailPanel == null) BuildDetailPanel();

        int totalSec = Mathf.RoundToInt(task.timeSpentSeconds);
        int h = totalSec / 3600;
        int m = (totalSec % 3600) / 60;
        int s = totalSec % 60;
        string timeStr = h > 0 ? $"{h}時間 {m}分 {s}秒" : (m > 0 ? $"{m}分 {s}秒" : $"{s}秒");
        string status = task.isCompleted ? "✔ 完了" : "⬜ 進行中";

        _detailText.text = $"【{task.taskName}】\n\n累計作業時間\n{timeStr}\n\n{status}";
        _detailPanel.SetActive(true);
    }
}
