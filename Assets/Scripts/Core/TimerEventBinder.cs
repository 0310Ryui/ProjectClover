using UnityEngine;

public class TimerEventBinder : MonoBehaviour {
    public TimerManager timerManager;
    
    public void SubWork() { if(timerManager != null) timerManager.AdjustWorkTime(-1); }
    public void AddWork() { if(timerManager != null) timerManager.AdjustWorkTime(1); }
    public void SubBreak() { if(timerManager != null) timerManager.AdjustBreakTime(-1); }
    public void AddBreak() { if(timerManager != null) timerManager.AdjustBreakTime(1); }
}
