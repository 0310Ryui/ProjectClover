using UnityEngine;
using UnityEngine.UI;
using System;

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
    public Text timerStatusText; // Shows "Focusing" etc.
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

    private void Start() {
        // Button listeners will be wired via script or inspector
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
            if (uiManager != null) uiManager.HideTimerSettings();
        } else if (CurrentState == TimerState.WaitingForBreak) {
            CurrentState = TimerState.Break;
            StartTimer(breakDurationMinutes * 60f);
            if (uiManager != null) uiManager.HideTimerSettings();
        }
    }

    public void StopCurrentTimer() {
        CurrentState = TimerState.Idle;
        RemainingSeconds = 0;
        isPaused = false;
        UpdateUI();
    }

    public void TogglePause() {
        if (CurrentState == TimerState.Work || CurrentState == TimerState.Break) {
            isPaused = !isPaused;
            if (!isPaused) {
                // unpaused, reset tracker so it doesn't jump
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

        // Track time for the selected task
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

        if (RemainingSeconds <= 0) {
            OnTimerCompleted();
        }
        UpdateUI();
    }

    private void OnTimerCompleted() {
        RemainingSeconds = 0;
        
        if (CurrentState == TimerState.Work) {
            CurrentState = TimerState.WaitingForBreak;
            onTimerCompleted?.Invoke(TimerState.Work);
            Debug.Log("Work completed. Waiting for break...");
        } else if (CurrentState == TimerState.Break) {
            CurrentState = TimerState.Idle;
            onTimerCompleted?.Invoke(TimerState.Break);
            Debug.Log("Break completed.");
        }

        // 完了時にタスクの累計時間を保存
        SaveManager.Save();
        UpdateUI();
    }

    private void OnApplicationPause(bool isAppPaused) {
        if (!isAppPaused && !isPaused && (CurrentState == TimerState.Work || CurrentState == TimerState.Break)) {
            TimeSpan elapsed = DateTime.Now - _lastTimeTracker;
            RemainingSeconds -= (float)elapsed.TotalSeconds;
            _lastTimeTracker = DateTime.Now;

            if (RemainingSeconds <= 0) {
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
                int min = Mathf.FloorToInt(Mathf.Max(0, RemainingSeconds) / 60);
                int sec = Mathf.FloorToInt(Mathf.Max(0, RemainingSeconds) % 60);
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
}
