using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_MainMenu : MonoBehaviour {
	[SerializeField] private GameObject firstSelectedButton;
	[SerializeField] private Button continueButton; // Reference to continue button

	// Keys that match the ones in SaveManager
	private const string KEY_LEVEL_COMPLETED_PREFIX = "Level_Completed_";
	private const string KEY_LEVEL_UNLOCKED_PREFIX = "Level_Unlocked_";
	private const string LEVEL_1 = "Level_1";

	void OnEnable() {
		EventSystem.current.SetSelectedGameObject(firstSelectedButton);
		CheckContinueButtonStatus();
	}

	private void CheckContinueButtonStatus() {
		// Check first if SaveManager is accessible
		if (SaveManager.Instance != null && LevelManager.Instance != null) {
			// Check if any level is unlocked or completed, not just Level_1
			bool hasProgress = false;

			// Get all available levels from LevelManager
			List<string> allLevels = LevelManager.Instance.GetAllLevelNames();

			foreach (string levelName in allLevels) {
				if (SaveManager.Instance.IsLevelUnlocked(levelName) ||
					SaveManager.Instance.IsLevelCompleted(levelName) ||
					SaveManager.Instance.HasLevelBeenPlayed(levelName)) {
					hasProgress = true;
					break;
				}
			}

			// Enable continue button if any progress is found
			continueButton.interactable = hasProgress;

			Debug.Log($"Continue button status: {continueButton.interactable} (Any level progress found: {hasProgress})");
		}
		else {
			// Fallback to direct PlayerPrefs check if SaveManager is not available
			bool hasProgress = PlayerPrefs.HasKey(KEY_LEVEL_COMPLETED_PREFIX + LEVEL_1) ||
							   PlayerPrefs.HasKey(KEY_LEVEL_UNLOCKED_PREFIX + LEVEL_1);

			continueButton.interactable = hasProgress;
			Debug.Log($"Continue button status (direct check): {continueButton.interactable}");
		}
	}
}
