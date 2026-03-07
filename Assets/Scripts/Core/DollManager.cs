using UnityEngine;
using UnityEngine.UI;

public class DollManager : MonoBehaviour {
    [Header("UI References")]
    public GameObject speechBubble;
    public Text speechText;
    public Image dollImage;

    [Header("Settings")]
    public string[] randomLines = new string[] {
        "hello...",
        "I'm sleepy.",
        "thank you.",
        "are you working hard?"
    };
    public float bubbleDisplaySeconds = 3f;

    private float hideBubbleTimer = 0;

    private void Start() {
        if (speechBubble != null) {
            speechBubble.SetActive(false);
        }
        LoadDollData();
    }

    private void Update() {
        if (speechBubble != null && speechBubble.activeSelf) {
            hideBubbleTimer -= Time.deltaTime;
            if (hideBubbleTimer <= 0) {
                speechBubble.SetActive(false);
            }
        }
    }

    private void LoadDollData() {
        var dollData = SaveManager.Current.doll;
        Debug.Log($"Loaded doll: {dollData.dollName}");
    }

    public void OnDollTapped() {
        if (speechBubble != null && speechText != null && randomLines.Length > 0) {
            string line = randomLines[Random.Range(0, randomLines.Length)];
            
            var dollData = SaveManager.Current.doll;
            if (dollData.currentMood > 0) { // arbitrary mood check
                line += " *smiles*";
            }

            speechText.text = line;
            speechBubble.SetActive(true);
            hideBubbleTimer = bubbleDisplaySeconds;
        }
    }
}
