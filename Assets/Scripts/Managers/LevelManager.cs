using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {
	// Singleton instance
	public static LevelManager Instance { get; private set; }
	[SerializeField] private bool showAllLevels;

	// List of all level scenes in order
	[SerializeField] private List<string> levelSceneNames = new List<string>();

	// Special scene names
	private const string MAIN_MENU_SCENE = "MainMenu";
	private const string CREDITS_SCENE = "The_End";

	// Level state flags
	[HideInInspector] public bool showLevelSelectOnMainMenu = false;

	private void Awake() {

		// Singleton setup
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
		}
		else if (Instance != this) {
			Destroy(gameObject);
		}

		// Initialize level names if empty
		if (levelSceneNames.Count == 0) {
			// Add default level names (replace with your actual level scene names)
			levelSceneNames.Add("Level_1");
			levelSceneNames.Add("Level_2");
			levelSceneNames.Add("Level_3");
			levelSceneNames.Add("Level_4");
			levelSceneNames.Add("Level_5");
			levelSceneNames.Add("Level_6");
			levelSceneNames.Add("Level_7");
			levelSceneNames.Add("Level_8");
			levelSceneNames.Add("Level_9");
			levelSceneNames.Add("Level_10");
			// Add more levels as needed
		}
	}

	void OnEnable() {
		if (showAllLevels) {
			// Unlock all levels if the flag is set
			UnlockAllLevels();
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
			LoadLevel(CREDITS_SCENE);
		}
	}

	public void LoadLevel(string levelName) {
		TimerManager.Instance.ResetLevelTimer();
		// Reset the first spawn flag so timer starts when player spawns
		PlayerManager.Instance.ResetFirstSpawnFlag();
		StartCoroutine(LoadLevelRoutine(levelName));
	}

	private IEnumerator LoadLevelRoutine(string levelName) {
		// Fade out
		UI_FadeEffect fadeEffect = FindFirstObjectByType<UI_FadeEffect>();
		if (fadeEffect != null) {
			fadeEffect.ScreenFadeEffect(1f, 0.5f);
			yield return new WaitForSeconds(0.5f);
		}

		// Load the scene
		SceneManager.LoadScene(levelName);

		// Wait for next frame to ensure scene is fully loaded
		yield return null;

		// Reset the fruit counter after the new scene is loaded
		if (FruitManager.Instance != null) {
			FruitManager.Instance.ResetFruitCounter();
		}
	}

	public void ReturnToMainMenu() {
		// Mark the current level as unlocked if it's a playable level
		string currentSceneName = SceneManager.GetActiveScene().name;
		if (levelSceneNames.Contains(currentSceneName) && SaveManager.Instance != null) {
			// Mark this level as unlocked when returning to main menu
			SaveManager.Instance.UnlockLevel(currentSceneName);
		}
		Time.timeScale = 1f; // Reset time scale to normal
		SceneManager.LoadScene(MAIN_MENU_SCENE);
	}

	public void LoadCredits() {
		SceneManager.LoadScene(CREDITS_SCENE);
	}

	public void LevelFinished() {
		// Store the completed level
		string currentLevel = SceneManager.GetActiveScene().name;
		SaveManager.Instance.SetLevelComplete(currentLevel);

		// Save the time for this level if it's better than previous
		float levelTime = TimerManager.Instance.GetCurrentLevelTime();
		SaveManager.Instance.SaveLevelBestTime(currentLevel, levelTime);

		// Wait a bit before loading next level
		StartCoroutine(LoadNextLevelAfterDelay(0f));
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
		return SaveManager.Instance.IsLevelUnlocked(levelName);
	}

	public void UnlockAllLevels() {
		SaveManager.Instance.UnlockAllLevels(levelSceneNames);
	}

	public List<string> GetAllLevelNames() {
		return levelSceneNames;
	}

	public int GetTotalLevelCount() {
		return levelSceneNames.Count;
	}
}
