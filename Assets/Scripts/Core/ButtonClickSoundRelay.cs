using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSoundRelay : MonoBehaviour {
    private Button _button;
    private FeedbackManager _feedbackManager;

    public void Initialize(FeedbackManager feedbackManager) {
        _feedbackManager = feedbackManager;
        CacheButton();
        Rebind();
    }

    private void OnEnable() {
        if (_feedbackManager == null) {
            _feedbackManager = FindFirstObjectByType<FeedbackManager>();
        }

        CacheButton();
        Rebind();
    }

    private void CacheButton() {
        if (_button == null) {
            _button = GetComponent<Button>();
        }
    }

    private void Rebind() {
        if (_button == null) return;
        _button.onClick.RemoveListener(HandleClick);
        _button.onClick.AddListener(HandleClick);
    }

    private void HandleClick() {
        if (_feedbackManager == null) {
            _feedbackManager = FindFirstObjectByType<FeedbackManager>();
        }

        if (_feedbackManager != null) {
            _feedbackManager.PlayClickSFX();
        }
    }
}
