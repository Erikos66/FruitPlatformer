using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour {
    // Keep references to important UI elements and controllers
    private UI_InGame inGameUI;
    private UI_FadeEffect fadeEffect;

    private void Start() {
        // Subscribe to events
        if (GameManager.instance.fruitManager != null) {
            GameManager.instance.fruitManager.OnFruitsUpdated += UpdateFruitUI;
        }

        // Try to find the UI elements
        FindUIElements();
    }

    private void OnDestroy() {
        // Unsubscribe from events
        if (GameManager.instance != null && GameManager.instance.fruitManager != null) {
            GameManager.instance.fruitManager.OnFruitsUpdated -= UpdateFruitUI;
        }
    }

    /// <summary>
    /// Find UI elements in the current scene
    /// </summary>
    public void FindUIElements() {
        // Find in-game UI
        if (inGameUI == null) {
            inGameUI = FindFirstObjectByType<UI_InGame>();
        }

        // Find fade effect
        if (fadeEffect == null) {
            fadeEffect = FindFirstObjectByType<UI_FadeEffect>();
        }
    }

    /// <summary>
    /// Update the fruit counter UI
    /// </summary>
    private void UpdateFruitUI(int collected, int total) {
        if (inGameUI != null) {
            inGameUI.OnFruitCollected();
        }
    }

    /// <summary>
    /// Show the pause menu
    /// </summary>
    public void ShowPauseMenu() {
        if (inGameUI != null) {
            inGameUI.PauseGame();
        }
    }

    /// <summary>
    /// Hide the pause menu and resume gameplay
    /// </summary>
    public void HidePauseMenu() {
        if (inGameUI != null) {
            inGameUI.ResumeGame();
        }
    }

    /// <summary>
    /// Perform a screen fade effect
    /// </summary>
    public void ScreenFade(float targetAlpha, float duration, System.Action onComplete = null) {
        if (fadeEffect == null) {
            fadeEffect = FindFirstObjectByType<UI_FadeEffect>();
        }

        if (fadeEffect != null) {
            fadeEffect.ScreenFadeEffect(targetAlpha, duration, onComplete);
        }
        else if (onComplete != null) {
            // If we can't find the fade effect but have a callback, invoke it after the duration
            StartCoroutine(DelayedCallback(duration, onComplete));
        }
    }

    private IEnumerator DelayedCallback(float delay, System.Action callback) {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }

    /// <summary>
    /// Update the level timer display
    /// </summary>
    public void UpdateTimerDisplay(float time) {
        if (inGameUI != null) {
            // The UI_InGame should handle the formatting of the time
            // This assumes you have a method in UI_InGame to update the timer text
        }
    }

    /// <summary>
    /// Show game over screen
    /// </summary>
    public void ShowGameOver() {
        // Implement game over screen functionality
    }

    /// <summary>
    /// Show level complete screen with stats
    /// </summary>
    public void ShowLevelComplete(float completionTime, int fruitsCollected, int totalFruits) {
        // Implement level complete UI functionality
    }
}