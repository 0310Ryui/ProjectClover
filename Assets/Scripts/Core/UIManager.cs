using UnityEngine;

public class UIManager : MonoBehaviour {
    [Header("Panels")]
    public GameObject mainScreenPanel;
    public GameObject timerSettingsOverlay;
    public GameObject todoOverlay;
    public GameObject magicOverlay;

    private void Start() {
        ShowMainScreen();
    }

    public void ShowMainScreen() {
        if (mainScreenPanel) mainScreenPanel.SetActive(true);
        if (timerSettingsOverlay) timerSettingsOverlay.SetActive(false);
        if (todoOverlay) todoOverlay.SetActive(false);
        if (magicOverlay) magicOverlay.SetActive(false);
    }

    public void ShowTimerSettings() {
        if (timerSettingsOverlay) timerSettingsOverlay.SetActive(true);
    }

    public void HideTimerSettings() {
        if (timerSettingsOverlay) timerSettingsOverlay.SetActive(false);
    }

    public void ShowTodoOverlay() {
        if (todoOverlay) todoOverlay.SetActive(true);
    }

    public void HideTodoOverlay() {
        if (todoOverlay) todoOverlay.SetActive(false);
    }

    public void ShowMagicOverlay() {
        if (magicOverlay) magicOverlay.SetActive(true);
    }

    public void HideMagicOverlay() {
        if (magicOverlay) magicOverlay.SetActive(false);
    }
}
