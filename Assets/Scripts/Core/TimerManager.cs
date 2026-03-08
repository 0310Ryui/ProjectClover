using System;
using UnityEngine;
using UnityEngine.UI;

public enum TimerState {
    Idle,
    Work,
    WaitingForBreak,
    Break
}

public class TimerManager : MonoBehaviour {
    public Action<TimerState> onTimerCompleted;

    [Header("UI References")]
    public Text mainTimerText;
    public Text timerStatusText;
    public Text workSettingsText;
    public Text breakSettingsText;

    [Header("External References")]
    public UIManager uiManager;

    [Header("Settings - Minutes")]
    public int workDurationMinutes = 25;
    public int breakDurationMinutes = 5;

    public TimerState CurrentState { get; private set; } = TimerState.Idle;
    public float RemainingSeconds { get; private set; }
    public bool isPaused { get; private set; }

    private DateTime _lastTimeTracker;
    private DollManager _dollManager;

    private void Start() {
        _dollManager = FindFirstObjectByType<DollManager>();
        UpdateUI();
    }

    public void AdjustWorkTime(int minuteDelta) {
        if (CurrentState == TimerState.Idle) {
            workDurationMinutes = Mathf.Clamp(workDurationMinutes + minuteDelta, 1, 120);
            UpdateUI();
        }
    }

    public void AdjustBreakTime(int minuteDelta) {
        if (CurrentState == TimerState.Idle || CurrentState == TimerState.WaitingForBreak) {
            breakDurationMinutes = Mathf.Clamp(breakDurationMinutes + minuteDelta, 1, 60);
            UpdateUI();
        }
    }

    public void StartTimerFromSettings() {
        if (CurrentState == TimerState.Idle) {
            CurrentState = TimerState.Work;
            StartTimer(workDurationMinutes * 60f);
            ShowDollSpeech(DollSpeechState.WorkStart);
            if (uiManager != null) uiManager.HideTimerSettings();
        } else if (CurrentState == TimerState.WaitingForBreak) {
            CurrentState = TimerState.Break;
            StartTimer(breakDurationMinutes * 60f);
            ShowDollSpeech(DollSpeechState.Break);
            if (uiManager != null) uiManager.HideTimerSettings();
        }
    }

    public void StopCurrentTimer() {
        CurrentState = TimerState.Idle;
        RemainingSeconds = 0f;
        isPaused = false;
        SaveManager.Save();
        UpdateUI();
    }

    public void TogglePause() {
        if (CurrentState == TimerState.Work || CurrentState == TimerState.Break) {
            isPaused = !isPaused;
            if (!isPaused) {
                _lastTimeTracker = DateTime.Now;
            }

            UpdateUI();
        }
    }

    private void StartTimer(float duration) {
        RemainingSeconds = duration;
        isPaused = false;
        _lastTimeTracker = DateTime.Now;
        UpdateUI();
    }

    private void Update() {
        if (CurrentState == TimerState.Idle || CurrentState == TimerState.WaitingForBreak || isPaused) return;

        float delta = Time.unscaledDeltaTime;
        RemainingSeconds -= delta;

        if (CurrentState == TimerState.Work) {
            var player = SaveManager.Current.player;
            if (!string.IsNullOrEmpty(player.selectedTaskId) && !string.IsNullOrEmpty(player.selectedCategoryId)) {
                var category = player.taskCategories.Find(c => c.categoryId == player.selectedCategoryId);
                var task = category?.tasks.Find(t => t.taskId == player.selectedTaskId);
                if (task != null) {
                    task.timeSpentSeconds += delta;
                }
            }
        }

        if (RemainingSeconds <= 0f) {
            OnTimerCompleted();
        }

        UpdateUI();
    }

    private void OnTimerCompleted() {
        RemainingSeconds = 0f;

        if (CurrentState == TimerState.Work) {
            CurrentState = TimerState.WaitingForBreak;
            onTimerCompleted?.Invoke(TimerState.Work);
            ShowDollSpeech(DollSpeechState.Break);
            Debug.Log("Work completed. Waiting for break...");
        } else if (CurrentState == TimerState.Break) {
            CurrentState = TimerState.Idle;
            onTimerCompleted?.Invoke(TimerState.Break);
            ShowDollSpeech(DollSpeechState.DayEnd);
            Debug.Log("Break completed.");
        }

        SaveManager.Save();
        UpdateUI();
    }

    private void OnApplicationPause(bool isAppPaused) {
        if (!isAppPaused && !isPaused && (CurrentState == TimerState.Work || CurrentState == TimerState.Break)) {
            TimeSpan elapsed = DateTime.Now - _lastTimeTracker;
            RemainingSeconds -= (float)elapsed.TotalSeconds;
            _lastTimeTracker = DateTime.Now;

            if (RemainingSeconds <= 0f) {
                OnTimerCompleted();
            }
        } else if (isAppPaused) {
            _lastTimeTracker = DateTime.Now;
        }
    }

    private void UpdateUI() {
        if (workSettingsText != null) workSettingsText.text = $"{workDurationMinutes:00}:00";
        if (breakSettingsText != null) breakSettingsText.text = $"{breakDurationMinutes:00}:00";

        if (mainTimerText != null) {
            if (CurrentState == TimerState.Idle) {
                mainTimerText.text = $"{workDurationMinutes:00}:00";
            } else if (CurrentState == TimerState.WaitingForBreak) {
                mainTimerText.text = $"{breakDurationMinutes:00}:00";
            } else {
                int min = Mathf.FloorToInt(Mathf.Max(0f, RemainingSeconds) / 60f);
                int sec = Mathf.FloorToInt(Mathf.Max(0f, RemainingSeconds) % 60f);
                mainTimerText.text = $"{min:00}:{sec:00}";
            }
        }

        if (timerStatusText != null) {
            switch (CurrentState) {
                case TimerState.Idle: timerStatusText.text = "Idle"; break;
                case TimerState.Work: timerStatusText.text = "Focusing"; break;
                case TimerState.WaitingForBreak: timerStatusText.text = "Waiting for Break"; break;
                case TimerState.Break: timerStatusText.text = "Resting"; break;
            }
        }
    }

    private void ShowDollSpeech(DollSpeechState state) {
        if (_dollManager == null) {
            _dollManager = FindFirstObjectByType<DollManager>();
        }

        if (_dollManager != null) {
            _dollManager.ShowStateMessage(state);
        }
    }
}
