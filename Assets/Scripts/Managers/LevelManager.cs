using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {
    // List of all level scenes in order
    [SerializeField] private List<string> levelSceneNames = new List<string>();

    // Special scene names
    private const string MAIN_MENU_SCENE = "MainMenu";
    private const string CREDITS_SCENE = "The_End";

    // Level state flags
    [HideInInspector] public bool showLevelSelectOnMainMenu = false;

    private void Start() {
        // Initialize level names if empty
        if (levelSceneNames.Count == 0) {
            // Add default level names (replace with your actual level scene names)
            levelSceneNames.Add("Level_1");
            levelSceneNames.Add("Level_2");
            levelSceneNames.Add("Level_3");
            // Add more levels as needed
        }
    }

    public void LoadNextLevel() {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Find the index of the current scene in our level list
        int currentLevelIndex = levelSceneNames.IndexOf(currentSceneName);

        if (currentLevelIndex >= 0 && currentLevelIndex < levelSceneNames.Count - 1) {
            // Load the next level in sequence
            string nextLevelName = levelSceneNames[currentLevelIndex + 1];
            LoadLevel(nextLevelName);
        }
        else {
            // If we can't find the current level or it's the last level, go to credits
            LoadCredits();
        }
    }

    public void LoadLevel(string levelName) {
        GameManager.instance.timerManager.ResetLevelTimer();
        // Reset the first spawn flag so timer starts when player spawns
        GameManager.instance.playerManager.ResetFirstSpawnFlag();
        StartCoroutine(LoadLevelRoutine(levelName));
    }

    private IEnumerator LoadLevelRoutine(string levelName) {
        // Fade out
        UI_FadeEffect fadeEffect = FindFirstObjectByType<UI_FadeEffect>();
        if (fadeEffect != null) {
            fadeEffect.ScreenFadeEffect(1f, 1.5f);
            yield return new WaitForSeconds(1.5f);
        }

        // Load the scene
        SceneManager.LoadScene(levelName);

        // Wait for next frame to ensure scene is fully loaded
        yield return null;

        // Reset the fruit counter after the new scene is loaded
        if (GameManager.instance != null && GameManager.instance.fruitManager != null) {
            GameManager.instance.fruitManager.ResetFruitCounter();
        }
    }

    public void ReturnToMainMenu() {
        StartCoroutine(LoadMainMenuRoutine());
    }

    private IEnumerator LoadMainMenuRoutine() {
        // Fade out
        UI_FadeEffect fadeEffect = FindFirstObjectByType<UI_FadeEffect>();
        if (fadeEffect != null) {
            fadeEffect.ScreenFadeEffect(1f, 1.5f);
            yield return new WaitForSeconds(1.5f);
        }

        // Reset time scale in case we're paused
        Time.timeScale = 1f;

        // Load main menu
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }

    public void LoadCredits() {
        SceneManager.LoadScene(CREDITS_SCENE);
    }

    public void LevelFinished() {
        // Store the completed level
        string currentLevel = SceneManager.GetActiveScene().name;
        GameManager.instance.saveManager.SetLevelComplete(currentLevel);

        // Save the time for this level if it's better than previous
        float levelTime = GameManager.instance.timerManager.GetCurrentLevelTime();
        GameManager.instance.saveManager.SaveLevelBestTime(currentLevel, levelTime);

        // Wait a bit before loading next level
        StartCoroutine(LoadNextLevelAfterDelay(3f));
    }

    private IEnumerator LoadNextLevelAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        LoadNextLevel();
    }

    public void RestartLevel() {
        string currentLevel = SceneManager.GetActiveScene().name;
        LoadLevel(currentLevel);
    }

    public bool IsLevelUnlocked(string levelName) {
        return GameManager.instance.saveManager.IsLevelUnlocked(levelName);
    }

    public void UnlockAllLevels() {
        GameManager.instance.saveManager.UnlockAllLevels(levelSceneNames);
    }

    public List<string> GetAllLevelNames() {
        return levelSceneNames;
    }

    public int GetTotalLevelCount() {
        return levelSceneNames.Count;
    }
}