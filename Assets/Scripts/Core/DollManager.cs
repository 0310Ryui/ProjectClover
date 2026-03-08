using UnityEngine;
using UnityEngine.UI;

public enum DollSpeechState {
    Idle,
    WorkStart,
    Working,
    Break,
    DayEnd
}

public class DollManager : MonoBehaviour {
    private static readonly string[] IdleLines = {
        "今日のお勉強も、いっしょに少しずつ進めてまいりましょう。",
        "あわてなくても大丈夫です。ひとつずつ覚えていけば、きっと身につきます。",
        "わたし、隣でちゃんと応援していますから。",
        "今日はどのページから始めましょうか。わたしもわくわくしています。",
        "わたしも見習いですから、いっしょに学べるのがうれしいです。",
        "知識の材料、こつこつ集めてまいりましょう。",
        "あせらず、ていねいに、ひとつずつ。",
        "未完成同士、いっしょに頑張りましょう。"
    };

    private static readonly string[] WorkStartLines = {
        "おはようございます。今日はどんな学びを始めましょうか。",
        "机の準備はできましたか。では、わたしたちの小さな勉強会を始めましょう。",
        "今日もあなたと学べるなんて、なんだか嬉しいです。",
        "まずは深呼吸をひとつ。気持ちを整えてから始めましょう。",
        "ほんの少しでも大丈夫です。今日の分を、一緒に積み上げましょう。",
        "では、本日の錬成――いえ、お勉強を始めましょう。"
    };

    private static readonly string[] WorkingLines = {
        "あなたのがんばり、わたしはちゃんと見ています。",
        "投げ出したくなる時ほど、少しだけ進めると立派です。",
        "完璧でなくても大丈夫です。今日は“昨日より少し良い”を目指しましょう。",
        "すぐ結果が出なくても、積み重ねはちゃんと魔法みたいに効いてきます。",
        "知識をひとしずくずつ集めれば、やがて立派な叡智の薬になります。",
        "勉強は、見えない素材を集める錬金術に少し似ていますね。",
        "焦りは火加減を誤るもとです。ゆっくり、丁寧にまいりましょう。",
        "今日の一歩が、未来の魔法になります。",
        "わたしが隣にいます。だから大丈夫です。"
    };

    private static readonly string[] BreakLines = {
        "ここまでよく頑張りました。少しお茶にいたしましょうか。",
        "頭を休ませると、知識もきれいに整うそうですよ。",
        "がんばりすぎは禁物です。見習いは、無茶をしないのも大事なお仕事です。",
        "少し伸びをしてまいりましょう。体が楽になると、頭も働きやすくなります。",
        "休憩のあとは、またゆっくり再開しましょうね。"
    };

    private static readonly string[] DayEndLines = {
        "今日もお疲れさまでした。ちゃんと進められましたね。",
        "たくさんでなくても大丈夫です。今日の積み重ねは、ちゃんと意味があります。",
        "頑張ったあとのあなたは、とても素敵です。",
        "今日はここまでにいたしましょう。続きはまた明日、いっしょに。",
        "どうか安心して休んでください。今日覚えたことが、眠っているあいだに馴染んでいきますように。",
        "学びの灯は、小さくても消えません。"
    };

    [Header("UI References")]
    public GameObject speechBubble;
    public Text speechText;
    public Image dollImage;

    [Header("Settings")]
    public float bubbleDisplaySeconds = 3f;

    private float hideBubbleTimer;
    private TimerManager _timerManager;

    private void Start() {
        if (speechBubble != null) {
            speechBubble.SetActive(false);
        }

        _timerManager = FindFirstObjectByType<TimerManager>();
        LoadDollData();
    }

    private void Update() {
        if (speechBubble == null || !speechBubble.activeSelf) return;

        hideBubbleTimer -= Time.deltaTime;
        if (hideBubbleTimer <= 0f) {
            speechBubble.SetActive(false);
        }
    }

    private void LoadDollData() {
        var dollData = SaveManager.Current.doll;
        Debug.Log($"Loaded doll: {dollData.dollName}");
    }

    public void OnDollTapped() {
        if (_timerManager == null) {
            _timerManager = FindFirstObjectByType<TimerManager>();
        }

        var state = ResolveTapSpeechState();
        ShowStateMessage(state);
    }

    public void ShowStateMessage(DollSpeechState state) {
        var lines = GetLinesForState(state);
        if (lines == null || lines.Length == 0) return;

        ShowMessage(lines[Random.Range(0, lines.Length)]);
    }

    public void ShowMessage(string message) {
        if (speechBubble == null || speechText == null || string.IsNullOrEmpty(message)) return;

        speechText.text = message;
        speechBubble.SetActive(true);
        hideBubbleTimer = bubbleDisplaySeconds;
    }

    private DollSpeechState ResolveTapSpeechState() {
        if (_timerManager == null) {
            return DollSpeechState.Idle;
        }

        switch (_timerManager.CurrentState) {
            case TimerState.Work:
                return DollSpeechState.Working;
            case TimerState.Break:
            case TimerState.WaitingForBreak:
                return DollSpeechState.Break;
            default:
                return DollSpeechState.Idle;
        }
    }

    private static string[] GetLinesForState(DollSpeechState state) {
        switch (state) {
            case DollSpeechState.WorkStart:
                return WorkStartLines;
            case DollSpeechState.Working:
                return WorkingLines;
            case DollSpeechState.Break:
                return BreakLines;
            case DollSpeechState.DayEnd:
                return DayEndLines;
            default:
                return IdleLines;
        }
    }
}
