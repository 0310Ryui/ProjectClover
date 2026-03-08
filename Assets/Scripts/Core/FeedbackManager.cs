using UnityEngine;
using UnityEngine.UI;

public class FeedbackManager : MonoBehaviour {
    private const string ClickClipPath = "Sound/SE/Anime_Pui";
    private const string WorkCompleteClipPath = "Sound/SE/Bell_Accent15-High";
    private const string BreakCompleteClipPath = "Sound/SE/Bell_Accent11";

    [Header("Audio")]
    public TimerManager timerManager;
    public AudioSource sfxSource;
    public AudioClip clickClip;
    public AudioClip timerCompleteClip;
    public AudioClip breakCompleteClip;
    public AudioClip magicCompleteClip;

    [Header("VFX References (Assign in Inspector)")]
    public ParticleSystem rewardVFX;
    public ParticleSystem magicVFX;

    private float _nextBindTime;

    private void Start() {
        EnsureDependencies();
        LoadClipsIfNeeded();
        ApplySavedVolume();
        SubscribeTimer();
        BindAllButtonsInScene();
    }

    private void Update() {
        if (Time.unscaledTime < _nextBindTime) return;
        _nextBindTime = Time.unscaledTime + 1f;
        BindAllButtonsInScene();
    }

    private void OnDestroy() {
        UnsubscribeTimer();
    }

    public void BindAllButtonsInScene() {
        var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++) {
            var relay = buttons[i].GetComponent<ButtonClickSoundRelay>();
            if (relay == null) {
                relay = buttons[i].gameObject.AddComponent<ButtonClickSoundRelay>();
            }

            relay.Initialize(this);
        }
    }

    public void ApplySavedVolume() {
        EnsureDependencies();
        if (sfxSource != null) {
            sfxSource.volume = SaveManager.Current.player.audioSettings.seVolume;
        }
    }

    public void PlaySePreview() {
        PlayClickSFX();
    }

    public void PlayClickSFX() {
        PlayClip(clickClip);
    }

    public void PlayTimerCompleteFeedback() {
        PlayClip(timerCompleteClip);
        if (rewardVFX != null) {
            rewardVFX.Play();
        }
    }

    public void PlayBreakCompleteFeedback() {
        PlayClip(breakCompleteClip);
    }

    public void PlayMagicCompleteFeedback() {
        PlayClip(magicCompleteClip);
        if (magicVFX != null) {
            magicVFX.Play();
        }
    }

    private void EnsureDependencies() {
        if (timerManager == null) {
            timerManager = FindFirstObjectByType<TimerManager>();
        }

        if (sfxSource == null) {
            var existingChild = transform.Find("SfxAudioSource");
            if (existingChild != null) {
                sfxSource = existingChild.GetComponent<AudioSource>();
            }

            if (sfxSource == null) {
                var sourceObject = new GameObject("SfxAudioSource");
                sourceObject.transform.SetParent(transform, false);
                sfxSource = sourceObject.AddComponent<AudioSource>();
            }
        }

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
    }

    private void LoadClipsIfNeeded() {
        if (clickClip == null) {
            clickClip = Resources.Load<AudioClip>(ClickClipPath);
        }

        if (timerCompleteClip == null) {
            timerCompleteClip = Resources.Load<AudioClip>(WorkCompleteClipPath);
        }

        if (breakCompleteClip == null) {
            breakCompleteClip = Resources.Load<AudioClip>(BreakCompleteClipPath);
        }
    }

    private void SubscribeTimer() {
        if (timerManager != null) {
            timerManager.onTimerCompleted -= HandleTimerCompleted;
            timerManager.onTimerCompleted += HandleTimerCompleted;
        }
    }

    private void UnsubscribeTimer() {
        if (timerManager != null) {
            timerManager.onTimerCompleted -= HandleTimerCompleted;
        }
    }

    private void HandleTimerCompleted(TimerState completedState) {
        if (completedState == TimerState.Work) {
            PlayTimerCompleteFeedback();
            return;
        }

        if (completedState == TimerState.Break) {
            PlayBreakCompleteFeedback();
        }
    }

    private void PlayClip(AudioClip clip) {
        EnsureDependencies();
        LoadClipsIfNeeded();
        ApplySavedVolume();

        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
