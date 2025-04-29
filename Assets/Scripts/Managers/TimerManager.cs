using UnityEngine;

public class TimerManager : MonoBehaviour {
    // Singleton instance
    public static TimerManager Instance { get; private set; }

    private float levelStartTime;
    private float levelEndTime;
    private bool isTimerRunning = false;

    void Awake() {
        // Singleton setup
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
        }
        else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Start the level timer
    /// </summary>
    public void StartLevelTimer() {
        levelStartTime = Time.time;
        isTimerRunning = true;
    }

    /// <summary>
    /// Stop the level timer
    /// </summary>
    public void StopLevelTimer() {
        if (isTimerRunning) {
            levelEndTime = Time.time;
            isTimerRunning = false;
        }
    }

    /// <summary>
    /// Reset the level timer
    /// </summary>
    public void ResetLevelTimer() {
        levelStartTime = 0f;
        levelEndTime = 0f;
        isTimerRunning = false;
    }

    /// <summary>
    /// Get the current level time (or final time if timer is stopped)
    /// </summary>
    public float GetCurrentLevelTime() {
        if (isTimerRunning) {
            return Time.time - levelStartTime;
        }
        else if (levelEndTime > 0) {
            return levelEndTime - levelStartTime;
        }
        return 0f;
    }

    /// <summary>
    /// Format the time as a string (MM:SS.ms)
    /// </summary>
    public string FormatTime(float timeInSeconds) {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100) % 100);

        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    /// <summary>
    /// Check if the timer is currently running
    /// </summary>
    public bool IsTimerRunning() {
        return isTimerRunning;
    }

    // Called every frame to update the timer UI if needed
    private void Update() {
        if (isTimerRunning) {
            // Update the UI with current time
            // This assumes there's a UIManager with a method to update the timer display
            UIManager.Instance.UpdateTimerDisplay(GetCurrentLevelTime());
        }
    }
}