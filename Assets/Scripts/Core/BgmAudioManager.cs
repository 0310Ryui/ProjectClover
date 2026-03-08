using UnityEngine;

public class BgmAudioManager : MonoBehaviour {
    private const string BgmClipPath = "Sound/BGM/Tea";

    public AudioSource bgmSource;
    public AudioClip bgmClip;

    private void Start() {
        EnsureDependencies();
        LoadClipIfNeeded();
        ApplySavedVolume();
        PlayLoop();
    }

    public void ApplySavedVolume() {
        EnsureDependencies();
        if (bgmSource != null) {
            bgmSource.volume = SaveManager.Current.player.audioSettings.bgmVolume;
        }
    }

    public void PreviewSample() {
        EnsureDependencies();
        LoadClipIfNeeded();
        ApplySavedVolume();

        if (bgmSource == null || bgmClip == null) return;

        if (!bgmSource.isPlaying) {
            bgmSource.Play();
            return;
        }

        bgmSource.UnPause();
    }

    private void EnsureDependencies() {
        if (bgmSource == null) {
            var existingChild = transform.Find("BgmAudioSource");
            if (existingChild != null) {
                bgmSource = existingChild.GetComponent<AudioSource>();
            }

            if (bgmSource == null) {
                var sourceObject = new GameObject("BgmAudioSource");
                sourceObject.transform.SetParent(transform, false);
                bgmSource = sourceObject.AddComponent<AudioSource>();
            }
        }

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
    }

    private void LoadClipIfNeeded() {
        if (bgmClip == null) {
            bgmClip = Resources.Load<AudioClip>(BgmClipPath);
        }

        if (bgmSource != null && bgmClip != null) {
            bgmSource.clip = bgmClip;
        }
    }

    private void PlayLoop() {
        if (bgmSource == null || bgmClip == null) return;
        if (!bgmSource.isPlaying) {
            bgmSource.Play();
        }
    }
}
