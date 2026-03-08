using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [Header("Panels")]
    public GameObject mainScreenPanel;
    public GameObject timerSettingsOverlay;
    public GameObject todoOverlay;
    public GameObject magicOverlay;
    public GameObject settingsOverlay;

    private Font _runtimeFont;
    private Button _runtimeSettingsButton;

    private void Start() {
        _runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureAudioManagers();
        EnsureSettingsButton();
        ShowMainScreen();
    }

    public void ShowMainScreen() {
        if (mainScreenPanel) mainScreenPanel.SetActive(true);
        if (timerSettingsOverlay) timerSettingsOverlay.SetActive(false);
        if (todoOverlay) todoOverlay.SetActive(false);
        if (magicOverlay) magicOverlay.SetActive(false);
        if (settingsOverlay) settingsOverlay.SetActive(false);
        RefreshButtonSounds();
    }

    public void ShowTimerSettings() {
        if (todoOverlay) todoOverlay.SetActive(false);
        if (magicOverlay) magicOverlay.SetActive(false);
        if (settingsOverlay) settingsOverlay.SetActive(false);
        if (timerSettingsOverlay) timerSettingsOverlay.SetActive(true);
        RefreshButtonSounds();
    }

    public void HideTimerSettings() {
        if (timerSettingsOverlay) timerSettingsOverlay.SetActive(false);
    }

    public void ShowTodoOverlay() {
        if (!todoOverlay) return;
        if (timerSettingsOverlay) timerSettingsOverlay.SetActive(false);
        if (magicOverlay) magicOverlay.SetActive(false);
        if (settingsOverlay) settingsOverlay.SetActive(false);
        TaskUiManager.EnsureSetup(todoOverlay, this);
        todoOverlay.SetActive(true);
        RefreshButtonSounds();
    }

    public void HideTodoOverlay() {
        if (todoOverlay) todoOverlay.SetActive(false);
    }

    public void ShowMagicOverlay() {
        if (!magicOverlay) return;
        if (timerSettingsOverlay) timerSettingsOverlay.SetActive(false);
        if (todoOverlay) todoOverlay.SetActive(false);
        if (settingsOverlay) settingsOverlay.SetActive(false);
        MagicUiManager.EnsureSetup(magicOverlay, FindFirstObjectByType<MagicManager>(), this);
        magicOverlay.SetActive(true);
        RefreshButtonSounds();
    }

    public void HideMagicOverlay() {
        if (magicOverlay) magicOverlay.SetActive(false);
    }

    public void ShowSettingsOverlay() {
        SettingsUiManager.EnsureSetup(this);
        if (timerSettingsOverlay) timerSettingsOverlay.SetActive(false);
        if (todoOverlay) todoOverlay.SetActive(false);
        if (magicOverlay) magicOverlay.SetActive(false);
        if (settingsOverlay) settingsOverlay.SetActive(true);
        RefreshButtonSounds();
    }

    public void HideSettingsOverlay() {
        if (settingsOverlay) settingsOverlay.SetActive(false);
    }

    private void EnsureAudioManagers() {
        if (FindFirstObjectByType<BgmAudioManager>() == null) {
            gameObject.AddComponent<BgmAudioManager>();
        }

        var feedbackManager = FindFirstObjectByType<FeedbackManager>();
        if (feedbackManager == null) {
            feedbackManager = gameObject.AddComponent<FeedbackManager>();
        }

        feedbackManager.BindAllButtonsInScene();
    }

    private void EnsureSettingsButton() {
        if (mainScreenPanel == null) return;

        var existingButton = mainScreenPanel.transform.Find("Btn_SystemSettings");
        if (existingButton != null) {
            _runtimeSettingsButton = existingButton.GetComponent<Button>();
            if (_runtimeSettingsButton != null) {
                _runtimeSettingsButton.onClick.RemoveListener(ShowSettingsOverlay);
                _runtimeSettingsButton.onClick.AddListener(ShowSettingsOverlay);
            }
            return;
        }

        var buttonObject = new GameObject("Btn_SystemSettings", typeof(RectTransform), typeof(Image), typeof(Button));
        var rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.SetParent(mainScreenPanel.transform, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(120f, 120f);

        var todoButton = mainScreenPanel.transform.Find("Btn_Todo") as RectTransform;
        rectTransform.anchoredPosition = todoButton != null ? todoButton.anchoredPosition + new Vector2(160f, 0f) : new Vector2(-255f, 828f);

        var image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.32f, 0.28f, 0.22f, 1f);

        _runtimeSettingsButton = buttonObject.GetComponent<Button>();
        _runtimeSettingsButton.targetGraphic = image;
        _runtimeSettingsButton.onClick.AddListener(ShowSettingsOverlay);

        var textObject = new GameObject("Text", typeof(RectTransform));
        var textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(buttonObject.transform, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 8f);
        textRect.offsetMax = new Vector2(-8f, -8f);

        var text = textObject.AddComponent<Text>();
        text.font = _runtimeFont;
        text.text = "⚙";
        text.fontSize = 54;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.96f, 0.88f, 1f);

        RefreshButtonSounds();
    }

    private void RefreshButtonSounds() {
        var feedbackManager = FindFirstObjectByType<FeedbackManager>();
        if (feedbackManager != null) {
            feedbackManager.BindAllButtonsInScene();
        }
    }
}
