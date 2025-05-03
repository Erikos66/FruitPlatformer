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
}
