using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour {
	// Singleton instance
	public static SaveManager Instance { get; private set; }

	// PlayerPrefs keys
	private const string KEY_LEVEL_UNLOCKED_PREFIX = "Level_Unlocked_";
	private const string KEY_LEVEL_COMPLETED_PREFIX = "Level_Completed_";
	private const string KEY_LEVEL_ALL_FRUITS_PREFIX = "Level_AllFruits_";
	private const string KEY_LEVEL_BEST_TIME_PREFIX = "Level_BestTime_";
	private const string KEY_SELECTED_SKIN = "Selected_Skin_Index";
	private const string KEY_LEVEL_FRUITS_COLLECTED_PREFIX = "Level_FruitsCollected_";
	private const string KEY_LEVEL_TOTAL_FRUITS_PREFIX = "Level_TotalFruits_";
	private const string KEY_GAME_DIFFICULTY = "Game_Difficulty";

	private void Awake() {
		// Singleton setup
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
		}
		else if (Instance != this) {
			Destroy(gameObject);
		}
		// Initialize the first level as unlocked by default
		if (!PlayerPrefs.HasKey(KEY_LEVEL_UNLOCKED_PREFIX + "Level_1")) {
			PlayerPrefs.SetInt(KEY_LEVEL_UNLOCKED_PREFIX + "Level_1", 1);
			PlayerPrefs.Save();
		}
	}

	#region Level Progression

	/// <summary>
	/// Set a level as completed
	/// </summary>
	public void SetLevelComplete(string levelName) {
		PlayerPrefs.SetInt(KEY_LEVEL_COMPLETED_PREFIX + levelName, 1);

		// Unlock the next level
		UnlockNextLevel(levelName);

		PlayerPrefs.Save();
	}

	/// <summary>
	/// Check if a level has been completed
	/// </summary>
	public bool IsLevelCompleted(string levelName) {
		return PlayerPrefs.GetInt(KEY_LEVEL_COMPLETED_PREFIX + levelName, 0) == 1;
	}

	/// <summary>
	/// Unlock the next level in sequence
	/// </summary>
	private void UnlockNextLevel(string currentLevelName) {
		List<string> levelNames = LevelManager.Instance.GetAllLevelNames();
		int currentIndex = levelNames.IndexOf(currentLevelName);

		if (currentIndex >= 0 && currentIndex < levelNames.Count - 1) {
			string nextLevelName = levelNames[currentIndex + 1];
			PlayerPrefs.SetInt(KEY_LEVEL_UNLOCKED_PREFIX + nextLevelName, 1);
		}
	}

	/// <summary>
	/// Check if a level is unlocked
	/// </summary>
	public bool IsLevelUnlocked(string levelName) {
		return PlayerPrefs.GetInt(KEY_LEVEL_UNLOCKED_PREFIX + levelName, 0) == 1;
	}

	/// <summary>
	/// Unlock all levels in the game
	/// </summary>
	public void UnlockAllLevels(List<string> levelNames) {
		foreach (string levelName in levelNames) {
			PlayerPrefs.SetInt(KEY_LEVEL_UNLOCKED_PREFIX + levelName, 1);
		}
		PlayerPrefs.Save();
	}

	#endregion

	#region Fruit Collection

	/// <summary>
	/// Mark that all fruits were collected in a level
	/// </summary>
	public void SetAllFruitsCollected(string levelName) {
		PlayerPrefs.SetInt(KEY_LEVEL_ALL_FRUITS_PREFIX + levelName, 1);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Check if all fruits were collected in a level
	/// </summary>
	public bool WereAllFruitsCollected(string levelName) {
		return PlayerPrefs.GetInt(KEY_LEVEL_ALL_FRUITS_PREFIX + levelName, 0) == 1;
	}

	/// <summary>
	/// Save the number of collected fruits in a level
	/// </summary>
	public void SaveCollectedFruitsCount(string levelName, int count) {
		PlayerPrefs.SetInt(KEY_LEVEL_FRUITS_COLLECTED_PREFIX + levelName, count);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Get the number of collected fruits in a level
	/// </summary>
	public int GetCollectedFruitsCount(string levelName) {
		return PlayerPrefs.GetInt(KEY_LEVEL_FRUITS_COLLECTED_PREFIX + levelName, 0);
	}

	/// <summary>
	/// Save the total number of fruits in a level
	/// </summary>
	public void SaveTotalFruitsCount(string levelName, int count) {
		PlayerPrefs.SetInt(KEY_LEVEL_TOTAL_FRUITS_PREFIX + levelName, count);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Get the total number of fruits in a level
	/// </summary>
	public int GetTotalFruitsCount(string levelName) {
		return PlayerPrefs.GetInt(KEY_LEVEL_TOTAL_FRUITS_PREFIX + levelName, 0);
	}

	/// <summary>
	/// Check if the level has been played before
	/// </summary>
	public bool HasLevelBeenPlayed(string levelName) {
		// A level is considered played if we have any data about collected fruits
		return PlayerPrefs.HasKey(KEY_LEVEL_FRUITS_COLLECTED_PREFIX + levelName);
	}

	#endregion

	#region Level Times

	/// <summary>
	/// Save the best time for a level
	/// </summary>
	public void SaveLevelBestTime(string levelName, float time) {
		float currentBestTime = GetLevelBestTime(levelName);

		// Only save if this is the first time or a better time
		if (currentBestTime <= 0 || time < currentBestTime) {
			PlayerPrefs.SetFloat(KEY_LEVEL_BEST_TIME_PREFIX + levelName, time);
			PlayerPrefs.Save();
		}
	}

	/// <summary>
	/// Get the best time for a level
	/// </summary>
	public float GetLevelBestTime(string levelName) {
		return PlayerPrefs.GetFloat(KEY_LEVEL_BEST_TIME_PREFIX + levelName, 0);
	}

	#endregion

	#region Player Skins

	/// <summary>
	/// Save the selected skin index
	/// </summary>
	public void SaveSelectedSkinIndex(int skinIndex) {
		PlayerPrefs.SetInt(KEY_SELECTED_SKIN, skinIndex);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Get the selected skin index
	/// </summary>
	public int GetSelectedSkinIndex() {
		return PlayerPrefs.GetInt(KEY_SELECTED_SKIN, 0);
	}

	#endregion

	#region Game Difficulty

	/// <summary>
	/// Save the game difficulty setting
	/// </summary>
	public void SaveGameDifficulty(GameManager.GameDifficulty difficulty) {
		// Save as int since PlayerPrefs doesn't directly support enums
		PlayerPrefs.SetInt(KEY_GAME_DIFFICULTY, (int)difficulty);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Get the saved game difficulty setting
	/// </summary>
	public GameManager.GameDifficulty GetGameDifficulty() {
		// Default to Easy if no difficulty has been saved
		int savedDifficulty = PlayerPrefs.GetInt(KEY_GAME_DIFFICULTY, 0);
		return (GameManager.GameDifficulty)savedDifficulty;
	}

	#endregion

	/// <summary>
	/// Reset all game progress
	/// </summary>
	public void ResetAllProgress() {
		// Clear all PlayerPrefs - be careful with this!
		List<string> levelNames = LevelManager.Instance.GetAllLevelNames();

		foreach (string levelName in levelNames) {
			PlayerPrefs.DeleteKey(KEY_LEVEL_COMPLETED_PREFIX + levelName);
			PlayerPrefs.DeleteKey(KEY_LEVEL_UNLOCKED_PREFIX + levelName);
			PlayerPrefs.DeleteKey(KEY_LEVEL_ALL_FRUITS_PREFIX + levelName);
			PlayerPrefs.DeleteKey(KEY_LEVEL_BEST_TIME_PREFIX + levelName);
			PlayerPrefs.DeleteKey(KEY_LEVEL_FRUITS_COLLECTED_PREFIX + levelName);
			PlayerPrefs.DeleteKey(KEY_LEVEL_TOTAL_FRUITS_PREFIX + levelName);
		}

		// Make sure the first level is still unlocked
		PlayerPrefs.SetInt(KEY_LEVEL_UNLOCKED_PREFIX + "Level_1", 1);

		PlayerPrefs.Save();
	}
}
