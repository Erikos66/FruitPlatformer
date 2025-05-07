using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_NewGameSelection : MonoBehaviour {

	[SerializeField] private Animator chrSkinDisplayAnimator;
	[SerializeField] private GameObject charSkinDisplayObject;
	[SerializeField] private GameObject firstSelectedButton;
	[SerializeField] private int selectedSkinIndex = 0;

	[Header("Difficulty Selection")]
	[SerializeField] private Button easyDifficultyButton;
	[SerializeField] private Button normalDifficultyButton;
	[SerializeField] private Color selectedButtonColor = Color.green;
	[SerializeField] private Color unselectedButtonColor = Color.white;

	void OnEnable() {
		EventSystem.current.SetSelectedGameObject(firstSelectedButton);
		charSkinDisplayObject.SetActive(true);
		for (int i = 0; i < chrSkinDisplayAnimator.layerCount; i++) {
			chrSkinDisplayAnimator.SetLayerWeight(i, 0f);
		}

		// Register difficulty button listeners
		if (easyDifficultyButton != null)
			easyDifficultyButton.onClick.AddListener(OnEasyDifficultySelected);

		if (normalDifficultyButton != null)
			normalDifficultyButton.onClick.AddListener(OnNormalDifficultySelected);

		// Update button visuals based on current difficulty
		UpdateDifficultyButtonVisuals();
	}

	void OnDisable() {
		if (chrSkinDisplayAnimator != null) {
			charSkinDisplayObject.SetActive(false);
		}


		// Unregister difficulty button listeners
		if (easyDifficultyButton != null)
			easyDifficultyButton.onClick.RemoveListener(OnEasyDifficultySelected);

		if (normalDifficultyButton != null)
			normalDifficultyButton.onClick.RemoveListener(OnNormalDifficultySelected);
	}

	public void NextSkin() {
		selectedSkinIndex++;
		if (selectedSkinIndex > 3) {
			selectedSkinIndex = 0;
		}
		AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
		UpdateSkin();
	}

	public void PreviousSkin() {
		selectedSkinIndex--;
		if (selectedSkinIndex < 0) {
			selectedSkinIndex = 3;
		}
		AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
		UpdateSkin();
	}


	private void UpdateSkin() {
		for (int i = 0; i < chrSkinDisplayAnimator.layerCount; i++) {
			if (i == selectedSkinIndex) {
				chrSkinDisplayAnimator.SetLayerWeight(i, 1f);
			}
			else {
				chrSkinDisplayAnimator.SetLayerWeight(i, 0f);
			}
		}
	}

	public void SelectSkin() {
		if (GameManager.Instance != null && PlayerManager.Instance != null) {
			SkinManager.Instance.SetSkin(selectedSkinIndex);
			Debug.Log("Selected skin index: " + selectedSkinIndex);
		}
		else {
			Debug.LogError("Cannot select skin: GameManager or playerManager is not initialized.");
		}
	}

	#region Difficulty Selection
	private void UpdateDifficultyButtonVisuals() {
		if (GameManager.Instance == null || easyDifficultyButton == null || normalDifficultyButton == null)
			return;

		// Get current difficulty
		GameManager.GameDifficulty currentDifficulty = GameManager.Instance.CurrentDifficulty;

		// Get button colors
		ColorBlock easyColorBlock = easyDifficultyButton.colors;
		ColorBlock normalColorBlock = normalDifficultyButton.colors;

		// Set button colors based on current difficulty
		if (currentDifficulty == GameManager.GameDifficulty.Easy) {
			easyColorBlock.normalColor = selectedButtonColor;
			normalColorBlock.normalColor = unselectedButtonColor;
		}
		else {
			easyColorBlock.normalColor = unselectedButtonColor;
			normalColorBlock.normalColor = selectedButtonColor;
		}

		// Apply updated color blocks
		easyDifficultyButton.colors = easyColorBlock;
		normalDifficultyButton.colors = normalColorBlock;
	}

	public void OnEasyDifficultySelected() {
		if (GameManager.Instance != null) {
			GameManager.Instance.CurrentDifficulty = GameManager.GameDifficulty.Easy;
			// Play UI confirmation sound
			AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
			// Update button visuals
			UpdateDifficultyButtonVisuals();
		}
	}

	public void OnNormalDifficultySelected() {
		if (GameManager.Instance != null) {
			GameManager.Instance.CurrentDifficulty = GameManager.GameDifficulty.Normal;
			// Play UI confirmation sound
			AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
			// Update button visuals
			UpdateDifficultyButtonVisuals();
		}
	}
	#endregion

	#region New Methods
	/// <summary>
	/// Clears all player progress data from the game
	/// </summary>
	public void ClearPlayerProgress() {
		// Store the current skin selection before clearing data
		int currentSkinIndex = 0;
		if (SaveManager.Instance != null) {
			currentSkinIndex = SaveManager.Instance.GetSelectedSkinIndex();
		}
		else if (SkinManager.Instance != null) {
			// Try to get from SkinManager if SaveManager is not available
			currentSkinIndex = selectedSkinIndex;
		}

		// Clear PlayerPrefs data
		PlayerPrefs.DeleteAll();

		// Restore the skin selection
		if (SaveManager.Instance != null) {
			SaveManager.Instance.SaveSelectedSkinIndex(currentSkinIndex);
		}

		// Reset any in-memory progress if game is running
		if (GameManager.Instance != null) {
			// Reset game manager state - since we don't have a ResetProgress method,
			// we can just let the game restart with the cleared PlayerPrefs
			Debug.Log("GameManager progress cleared through PlayerPrefs");
		}

		if (PlayerManager.Instance != null) {
			// Reset player-specific data
			// Since there's no ResetPlayerData method, we'll rely on PlayerPrefs being cleared
			Debug.Log("PlayerManager data cleared through PlayerPrefs");
		}

		Debug.Log($"All player progress has been cleared, skin selection ({currentSkinIndex}) preserved");
		AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
	}

	/// <summary>
	/// Loads a scene by name
	/// </summary>
	/// <param name="sceneName">Name of the scene to load</param>
	public void LoadScene(string sceneName) {

		// Save the selected skin before loading the scene
		if (SkinManager.Instance != null) {
			SkinManager.Instance.SetSkin(selectedSkinIndex);
			Debug.Log($"Saved skin selection ({selectedSkinIndex}) before loading scene");
		}

		Debug.Log($"Loading scene: {sceneName}");

		// Load the level
		LevelManager.Instance.LoadLevel(sceneName);
	}
	#endregion
}
