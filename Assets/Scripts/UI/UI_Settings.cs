using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class UI_Settings : MonoBehaviour {

	[Header("Navigation")]
	[SerializeField] private GameObject firstSelectedButton; // First button to be selected when the menu opens

	[Header("Audio Settings")]
	[SerializeField] private Slider sfxVolumeSlider;
	[SerializeField] private Slider musicVolumeSlider;

	[Header("Display Settings")]
	[SerializeField] private TMP_Dropdown resolutionDropdown;
	[SerializeField] private TMP_Dropdown displayModeDropdown;

	[Header("Game Settings")]
	[SerializeField] private Button easyDifficultyButton;
	[SerializeField] private Button normalDifficultyButton;
	[SerializeField] private Toggle onScreenControlsToggle; // Added on-screen controls toggle
	[SerializeField] private Color selectedButtonColor = Color.green;
	[SerializeField] private Color unselectedButtonColor = Color.white;

	private Resolution[] resolutions;
	private AudioManager audioManager;

	void Awake() {
		audioManager = AudioManager.Instance;
		if (audioManager == null) {
			Debug.LogError("AudioManager not found!");
		}

		// Initialize display mode dropdown
		if (displayModeDropdown != null) {
			displayModeDropdown.ClearOptions();
			var displayModes = new List<string> { "Windowed", "Fullscreen", "Borderless Window" };
			displayModeDropdown.AddOptions(displayModes);
		}

		// Initialize resolution dropdown
		if (resolutionDropdown != null) {
			InitializeResolutionDropdown();
		}

		// Initialize difficulty buttons
		InitializeDifficultyButtons();
	}

	void Start() {
		// Initialize volume sliders with saved values
		if (audioManager != null) {
			if (sfxVolumeSlider != null)
				sfxVolumeSlider.value = audioManager.SfxVolume;

			if (musicVolumeSlider != null)
				musicVolumeSlider.value = audioManager.MusicVolume;
		}

		// Set initial dropdown values based on current settings
		if (displayModeDropdown != null) {
			if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
				displayModeDropdown.value = 1; // Fullscreen
			else if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
				displayModeDropdown.value = 2; // Borderless Window
			else
				displayModeDropdown.value = 0; // Windowed
		}

		// Set initial toggle state for on-screen controls
		if (onScreenControlsToggle != null && GameManager.Instance != null) {
			onScreenControlsToggle.isOn = GameManager.Instance.OnScreenControlsEnabled;
		}
	}

	void OnEnable() {
		EventSystem.current.SetSelectedGameObject(firstSelectedButton);

		// Register listeners for sliders and dropdowns
		if (sfxVolumeSlider != null)
			sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

		if (musicVolumeSlider != null)
			musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

		if (resolutionDropdown != null)
			resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

		if (displayModeDropdown != null)
			displayModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);

		// Register difficulty button listeners
		if (easyDifficultyButton != null)
			easyDifficultyButton.onClick.AddListener(OnEasyDifficultySelected);

		if (normalDifficultyButton != null)
			normalDifficultyButton.onClick.AddListener(OnNormalDifficultySelected);

		// Register on-screen controls toggle listener
		if (onScreenControlsToggle != null)
			onScreenControlsToggle.onValueChanged.AddListener(OnOnScreenControlsToggled);

		// Update button visuals based on current difficulty
		UpdateDifficultyButtonVisuals();
	}

	void OnDisable() {
		// Unregister listeners when disabled
		if (sfxVolumeSlider != null)
			sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);

		if (musicVolumeSlider != null)
			musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

		if (resolutionDropdown != null)
			resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);

		if (displayModeDropdown != null)
			displayModeDropdown.onValueChanged.RemoveListener(OnDisplayModeChanged);

		// Unregister difficulty button listeners
		if (easyDifficultyButton != null)
			easyDifficultyButton.onClick.RemoveListener(OnEasyDifficultySelected);

		if (normalDifficultyButton != null)
			normalDifficultyButton.onClick.RemoveListener(OnNormalDifficultySelected);

		// Unregister on-screen controls toggle listener
		if (onScreenControlsToggle != null)
			onScreenControlsToggle.onValueChanged.RemoveListener(OnOnScreenControlsToggled);
	}

	private void InitializeResolutionDropdown() {
		resolutionDropdown.ClearOptions();

		// Get available resolutions
		resolutions = Screen.resolutions;
		List<string> resolutionOptions = new();
		int currentResolutionIndex = 0;

		for (int i = 0; i < resolutions.Length; i++) {
			string option = $"{resolutions[i].width} x {resolutions[i].height}";
			resolutionOptions.Add(option);

			// Check if this is the current resolution
			if (resolutions[i].width == Screen.currentResolution.width &&
				resolutions[i].height == Screen.currentResolution.height) {
				currentResolutionIndex = i;
			}
		}

		resolutionDropdown.AddOptions(resolutionOptions);
		resolutionDropdown.value = currentResolutionIndex;
		resolutionDropdown.RefreshShownValue();
	}

	private void InitializeDifficultyButtons() {
		// Update button visuals based on current difficulty
		UpdateDifficultyButtonVisuals();
	}

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

	public void OnSfxVolumeChanged(float value) {
		if (audioManager != null) {
			// Pass the new SFX volume to Audio Manager
			audioManager.SetSfxVolume(value);
		}
	}

	public void OnMusicVolumeChanged(float value) {
		if (audioManager != null) {
			// Pass the new Music volume to Audio Manager
			audioManager.SetMusicVolume(value);
		}
	}

	public void OnResolutionChanged(int index) {
		if (index < resolutions.Length) {
			Resolution newResolution = resolutions[index];
			// Apply the resolution but maintain current fullscreen mode
			Screen.SetResolution(newResolution.width, newResolution.height, Screen.fullScreenMode);
			PlayerPrefs.SetInt("ResolutionIndex", index);
			PlayerPrefs.Save();
		}
	}

	public void OnDisplayModeChanged(int index) {
		FullScreenMode mode;

		switch (index) {
			case 1: // Fullscreen
				mode = FullScreenMode.ExclusiveFullScreen;
				break;
			case 2: // Borderless Window
				mode = FullScreenMode.FullScreenWindow;
				break;
			default: // Windowed
				mode = FullScreenMode.Windowed;
				break;
		}

		// Apply the display mode but maintain current resolution
		Screen.fullScreenMode = mode;
		PlayerPrefs.SetInt("DisplayMode", index);
		PlayerPrefs.Save();
	}

	public void OnOnScreenControlsToggled(bool isOn) {
		if (GameManager.Instance != null) {
			// Update GameManager setting
			GameManager.Instance.OnScreenControlsEnabled = isOn;

			// Play UI confirmation sound
			AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");

			// If we're in a gameplay scene, update the UI immediately
			if (UI_InGame.Instance != null) {
				UI_InGame.Instance.RefreshOnScreenControls();
			}
		}
	}
}
