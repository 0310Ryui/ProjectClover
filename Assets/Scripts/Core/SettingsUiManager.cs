using UnityEngine;
using UnityEngine.UI;

public class SettingsUiManager : MonoBehaviour {
    private const string GeneratedRootName = "SettingsUiRoot";
    private const float DefaultVolume = 0.5f;

    private UIManager _uiManager;
    private Font _font;

    private RectTransform _generatedRoot;
    private Slider _seSlider;
    private Slider _bgmSlider;
    private Text _seValueText;
    private Text _bgmValueText;

    public static SettingsUiManager EnsureSetup(UIManager uiManager) {
        if (uiManager == null) return null;

        if (uiManager.settingsOverlay == null) {
            uiManager.settingsOverlay = CreateOverlay(uiManager);
        }

        var manager = uiManager.settingsOverlay.GetComponent<SettingsUiManager>();
        if (manager == null) {
            manager = uiManager.settingsOverlay.AddComponent<SettingsUiManager>();
        }

        manager._uiManager = uiManager;
        manager.InitializeIfNeeded();
        return manager;
    }

    private void OnEnable() {
        InitializeIfNeeded();
        RefreshFromSave();
    }

    private void InitializeIfNeeded() {
        if (_generatedRoot != null) return;

        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        HideLegacyChildren();
        BuildLayout();
        RefreshFromSave();
    }

    private void RefreshFromSave() {
        var audioSettings = SaveManager.Current.player.audioSettings;

        if (_seSlider != null) {
            _seSlider.SetValueWithoutNotify(audioSettings.seVolume);
        }

        if (_bgmSlider != null) {
            _bgmSlider.SetValueWithoutNotify(audioSettings.bgmVolume);
        }

        UpdateValueLabels();
        ApplyAudioVolumes();
    }

    private void HideLegacyChildren() {
        foreach (Transform child in transform) {
            if (child.name == GeneratedRootName) continue;
            child.gameObject.SetActive(false);
        }
    }

    private void BuildLayout() {
        _generatedRoot = CreateUIObject(GeneratedRootName, transform);
        Stretch(_generatedRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        _generatedRoot.gameObject.AddComponent<Image>().color = new Color(0.13f, 0.12f, 0.1f, 0.94f);

        var panel = CreateUIObject("Panel", _generatedRoot);
        Stretch(panel, new Vector2(0.16f, 0.14f), new Vector2(0.84f, 0.86f), Vector2.zero, Vector2.zero);
        panel.gameObject.AddComponent<Image>().color = new Color(0.97f, 0.93f, 0.86f, 1f);

        var title = CreateText("Title", panel, "Settings", 36, TextAnchor.MiddleCenter, new Color(0.19f, 0.14f, 0.1f));
        Stretch(title.rectTransform, new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.94f), Vector2.zero, Vector2.zero);

        CreateSliderRow(panel, "SE 音量", out _seSlider, out _seValueText, 0.60f, OnSeVolumeChanged);
        CreateSliderRow(panel, "BGM 音量", out _bgmSlider, out _bgmValueText, 0.40f, OnBgmVolumeChanged);

        var previewButton = CreateButton("Btn_SePreview", panel, "SE 試聴", new Color(0.36f, 0.57f, 0.74f), PlaySePreview);
        Stretch((RectTransform)previewButton.transform, new Vector2(0.16f, 0.24f), new Vector2(0.40f, 0.32f), Vector2.zero, Vector2.zero);

        var defaultButton = CreateButton("Btn_Default", panel, "Default に戻す", new Color(0.44f, 0.48f, 0.7f), ResetToDefault);
        Stretch((RectTransform)defaultButton.transform, new Vector2(0.42f, 0.24f), new Vector2(0.68f, 0.32f), Vector2.zero, Vector2.zero);

        var closeButton = CreateButton("Btn_CloseSettingsGenerated", panel, "閉じる", new Color(0.72f, 0.31f, 0.27f), Close);
        Stretch((RectTransform)closeButton.transform, new Vector2(0.70f, 0.24f), new Vector2(0.84f, 0.32f), Vector2.zero, Vector2.zero);
    }

    private void CreateSliderRow(Transform parent, string label, out Slider slider, out Text valueText, float yMin, UnityEngine.Events.UnityAction<float> onChanged) {
        var row = CreateUIObject(label.Replace(" ", string.Empty), parent);
        Stretch(row, new Vector2(0.1f, yMin), new Vector2(0.9f, yMin + 0.14f), Vector2.zero, Vector2.zero);

        var labelText = CreateText("Label", row, label, 24, TextAnchor.MiddleLeft, new Color(0.2f, 0.16f, 0.12f));
        Stretch(labelText.rectTransform, new Vector2(0f, 0.58f), new Vector2(0.65f, 1f), Vector2.zero, Vector2.zero);

        valueText = CreateText("Value", row, "50%", 22, TextAnchor.MiddleRight, new Color(0.34f, 0.28f, 0.22f));
        Stretch(valueText.rectTransform, new Vector2(0.68f, 0.58f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

        slider = CreateSlider("Slider", row, onChanged);
        Stretch((RectTransform)slider.transform, new Vector2(0f, 0f), new Vector2(1f, 0.46f), new Vector2(12f, 0f), new Vector2(-12f, 0f));
    }

    private Slider CreateSlider(string objectName, Transform parent, UnityEngine.Events.UnityAction<float> onChanged) {
        var sliderObject = CreateUIObject(objectName, parent).gameObject;
        var slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        var background = CreateUIObject("Background", sliderObject.transform).gameObject;
        Stretch((RectTransform)background.transform, new Vector2(0f, 0.3f), new Vector2(1f, 0.7f), Vector2.zero, Vector2.zero);
        var backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.75f, 0.71f, 0.64f, 1f);

        var fillArea = CreateUIObject("Fill Area", sliderObject.transform);
        Stretch(fillArea, new Vector2(0f, 0.3f), new Vector2(1f, 0.7f), new Vector2(10f, 0f), new Vector2(-10f, 0f));

        var fill = CreateUIObject("Fill", fillArea);
        Stretch(fill, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        var fillImage = fill.gameObject.AddComponent<Image>();
        fillImage.color = new Color(0.45f, 0.62f, 0.39f, 1f);

        var handleSlideArea = CreateUIObject("Handle Slide Area", sliderObject.transform);
        Stretch(handleSlideArea, Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));

        var handle = CreateUIObject("Handle", handleSlideArea).gameObject;
        Stretch((RectTransform)handle.transform, new Vector2(0f, 0.1f), new Vector2(0f, 0.9f), new Vector2(-14f, 0f), new Vector2(14f, 0f));
        var handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(0.84f, 0.45f, 0.3f, 1f);

        slider.targetGraphic = handleImage;
        slider.handleRect = (RectTransform)handle.transform;
        slider.fillRect = fill;
        slider.direction = Slider.Direction.LeftToRight;
        slider.onValueChanged.AddListener(onChanged);

        return slider;
    }

    private void OnSeVolumeChanged(float value) {
        SaveManager.Current.player.audioSettings.seVolume = value;
        SaveManager.Save();
        UpdateValueLabels();
        ApplyAudioVolumes();
    }

    private void OnBgmVolumeChanged(float value) {
        SaveManager.Current.player.audioSettings.bgmVolume = value;
        SaveManager.Save();
        UpdateValueLabels();
        ApplyAudioVolumes();

        var bgmAudioManager = FindFirstObjectByType<BgmAudioManager>();
        if (bgmAudioManager != null) {
            bgmAudioManager.PreviewSample();
        }
    }

    private void PlaySePreview() {
        var feedbackManager = FindFirstObjectByType<FeedbackManager>();
        if (feedbackManager != null) {
            feedbackManager.PlaySePreview();
        }
    }

    private void ResetToDefault() {
        var audioSettings = SaveManager.Current.player.audioSettings;
        audioSettings.seVolume = DefaultVolume;
        audioSettings.bgmVolume = DefaultVolume;
        SaveManager.Save();
        RefreshFromSave();
    }

    private void Close() {
        if (_uiManager != null) {
            _uiManager.HideSettingsOverlay();
        } else {
            gameObject.SetActive(false);
        }
    }

    private void UpdateValueLabels() {
        if (_seValueText != null && _seSlider != null) {
            _seValueText.text = Mathf.RoundToInt(_seSlider.value * 100f) + "%";
        }

        if (_bgmValueText != null && _bgmSlider != null) {
            _bgmValueText.text = Mathf.RoundToInt(_bgmSlider.value * 100f) + "%";
        }
    }

    private void ApplyAudioVolumes() {
        var feedbackManager = FindFirstObjectByType<FeedbackManager>();
        if (feedbackManager != null) {
            feedbackManager.ApplySavedVolume();
            feedbackManager.BindAllButtonsInScene();
        }

        var bgmAudioManager = FindFirstObjectByType<BgmAudioManager>();
        if (bgmAudioManager != null) {
            bgmAudioManager.ApplySavedVolume();
        }
    }

    private static GameObject CreateOverlay(UIManager uiManager) {
        Transform parent = null;
        if (uiManager.timerSettingsOverlay != null) {
            parent = uiManager.timerSettingsOverlay.transform.parent;
        } else if (uiManager.mainScreenPanel != null) {
            parent = uiManager.mainScreenPanel.transform.parent;
        }

        var overlay = new GameObject("SettingsOverlay", typeof(RectTransform));
        var rectTransform = overlay.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        overlay.SetActive(false);
        return overlay;
    }

    private Button CreateButton(string objectName, Transform parent, string label, Color backgroundColor, UnityEngine.Events.UnityAction onClick) {
        var buttonObject = CreateUIObject(objectName, parent).gameObject;
        var image = buttonObject.AddComponent<Image>();
        image.color = backgroundColor;

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var text = CreateText("Text", (RectTransform)button.transform, label, 24, TextAnchor.MiddleCenter, Color.white);
        Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 8f), new Vector2(-10f, -8f));
        return button;
    }

    private Text CreateText(string objectName, Transform parent, string value, int fontSize, TextAnchor alignment, Color color) {
        var textObject = CreateUIObject(objectName, parent).gameObject;
        var text = textObject.AddComponent<Text>();
        text.font = _font;
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static RectTransform CreateUIObject(string objectName, Transform parent) {
        var go = new GameObject(objectName, typeof(RectTransform));
        var rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.localScale = Vector3.one;
        return rectTransform;
    }

    private static void Stretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax) {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
    }
}
