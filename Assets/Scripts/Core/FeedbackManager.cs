using UnityEngine;

public class FeedbackManager : MonoBehaviour {
    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip clickClip;
    public AudioClip timerCompleteClip;
    public AudioClip magicCompleteClip;

    [Header("VFX References (Assign in Inspector)")]
    public ParticleSystem rewardVFX;
    public ParticleSystem magicVFX;

    // Call from UI Buttons OnClick
    public void PlayClickSFX() {
        if (sfxSource != null && clickClip != null) {
            sfxSource.PlayOneShot(clickClip);
        }
    }

    // Call from RewardManager or TimerManager when completed
    public void PlayTimerCompleteFeedback() {
        if (sfxSource != null && timerCompleteClip != null) {
            sfxSource.PlayOneShot(timerCompleteClip);
        }
        if (rewardVFX != null) {
            rewardVFX.Play();
        }
    }

    // Call from MagicManager when research completes
    public void PlayMagicCompleteFeedback() {
        if (sfxSource != null && magicCompleteClip != null) {
            sfxSource.PlayOneShot(magicCompleteClip);
        }
        if (magicVFX != null) {
            magicVFX.Play();
        }
    }
}
