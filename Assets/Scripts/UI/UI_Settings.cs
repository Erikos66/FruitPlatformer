using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Handles game settings such as audio volumes and screen mode options
/// </summary>
public class UI_Settings : MonoBehaviour {
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle muteToggle;

    [Header("Video Settings")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    // Resolution options
    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;
    private float currentRefreshRate;
    private int currentResolutionIndex = 0;

    // Default mixer parameter names - match these with your actual mixer parameters
    private const string MIXER_MUSIC = "MusicVolume";
    private const string MIXER_SFX = "SFXVolume";

    // PlayerPrefs keys
    private const string PREF_MUSIC_VOLUME = "MusicVolume";
    private const string PREF_SFX_VOLUME = "SFXVolume";
    private const string PREF_MUTE = "Muted";
    private const string PREF_SCREEN_MODE = "ScreenMode";
    private const string PREF_RESOLUTION = "Resolution";

    private void Awake() {
        // Initialize the UI components if they're not set in inspector
        if (musicVolumeSlider == null) musicVolumeSlider = transform.Find("MusicVolumeSlider")?.GetComponent<Slider>();
        if (sfxVolumeSlider == null) sfxVolumeSlider = transform.Find("SFXVolumeSlider")?.GetComponent<Slider>();
        if (muteToggle == null) muteToggle = transform.Find("MuteToggle")?.GetComponent<Toggle>();
        if (resolutionDropdown == null) resolutionDropdown = transform.Find("ResolutionDropdown")?.GetComponent<TMP_Dropdown>();
        if (screenModeDropdown == null) screenModeDropdown = transform.Find("ScreenModeDropdown")?.GetComponent<TMP_Dropdown>();
        if (musicVolumeText == null) musicVolumeText = musicVolumeSlider?.transform.Find("VolumeText")?.GetComponent<TextMeshProUGUI>();
        if (sfxVolumeText == null) sfxVolumeText = sfxVolumeSlider?.transform.Find("VolumeText")?.GetComponent<TextMeshProUGUI>();
    }

    private void Start() {
        // Initialize resolution options
        SetupResolutions();

        // Initialize screen mode dropdown
        SetupScreenModeDropdown();

        // Set up audio control listeners
        SetupAudioControls();

        // Load saved settings
        LoadSettings();
    }

    #region Resolution and Screen Mode Setup

    private void SetupResolutions() {
        if (resolutionDropdown == null) return;

        // Get all available resolutions
        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        // Get current refresh rate
        currentRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;

        // Clear the dropdown options
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        // Filter resolutions to keep only those with current refresh rate
        for (int i = 0; i < resolutions.Length; i++) {
            if (Mathf.Approximately((float)resolutions[i].refreshRateRatio.value, currentRefreshRate)) {
                filteredResolutions.Add(resolutions[i]);
            }
        }

        // If no resolutions were filtered, use all resolutions
        if (filteredResolutions.Count == 0) {
            filteredResolutions.AddRange(resolutions);
        }

        // Add each resolution to the dropdown
        for (int i = 0; i < filteredResolutions.Count; i++) {
            string option = filteredResolutions[i].width + " x " + filteredResolutions[i].height;
            options.Add(option);

            // Check if this is the current resolution
            if (filteredResolutions[i].width == Screen.currentResolution.width &&
                filteredResolutions[i].height == Screen.currentResolution.height) {
                currentResolutionIndex = i;
            }
        }

        // Add options to dropdown
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Add listener
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void SetupScreenModeDropdown() {
        if (screenModeDropdown == null) return;

        // Clear the dropdown options
        screenModeDropdown.ClearOptions();

        // Add screen mode options
        List<string> options = new List<string>
        {
            "Fullscreen",
            "Borderless Windowed",
            "Windowed"
        };

        screenModeDropdown.AddOptions(options);

        // Set current value based on screen mode
        if (Screen.fullScreen) {
            screenModeDropdown.value = 0; // Fullscreen
        }
        else {
            screenModeDropdown.value = 2; // Windowed
        }

        screenModeDropdown.RefreshShownValue();

        // Add listener
        screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
    }

    #endregion

    #region Audio Setup

    private void SetupAudioControls() {
        // Set up music volume slider
        if (musicVolumeSlider != null) {
            musicVolumeSlider.minValue = 0.0001f; // Avoid log(0) which is -infinity
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        // Set up SFX volume slider
        if (sfxVolumeSlider != null) {
            sfxVolumeSlider.minValue = 0.0001f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        // Set up mute toggle
        if (muteToggle != null) {
            muteToggle.onValueChanged.AddListener(SetMute);
        }
    }

    #endregion

    #region Settings Methods

    public void SetMusicVolume(float volume) {
        if (audioMixer != null) {
            // Convert slider value (linear) to decibels (logarithmic)
            float dbValue = Mathf.Log10(volume) * 20;
            audioMixer.SetFloat(MIXER_MUSIC, dbValue);
        }

        // Update UI text if available
        if (musicVolumeText != null) {
            musicVolumeText.text = Mathf.Round(volume * 100) + "%";
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat(PREF_MUSIC_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume) {
        if (audioMixer != null) {
            // Convert slider value (linear) to decibels (logarithmic)
            float dbValue = Mathf.Log10(volume) * 20;
            audioMixer.SetFloat(MIXER_SFX, dbValue);
        }

        // Update UI text if available
        if (sfxVolumeText != null) {
            sfxVolumeText.text = Mathf.Round(volume * 100) + "%";
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat(PREF_SFX_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public void SetMute(bool isMuted) {
        if (audioMixer != null) {
            // Mute/unmute both channels
            float muteValue = isMuted ? -80f : 0f;
            audioMixer.SetFloat(MIXER_MUSIC, isMuted ? -80f : Mathf.Log10(musicVolumeSlider.value) * 20);
            audioMixer.SetFloat(MIXER_SFX, isMuted ? -80f : Mathf.Log10(sfxVolumeSlider.value) * 20);
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetInt(PREF_MUTE, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetScreenMode(int screenModeIndex) {
        // 0 = Fullscreen, 1 = Borderless Windowed, 2 = Windowed
        switch (screenModeIndex) {
            case 0: // Fullscreen
                Screen.fullScreen = true;
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;

            case 1: // Borderless Windowed
                Screen.fullScreen = true;
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;

            case 2: // Windowed
                Screen.fullScreen = false;
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetInt(PREF_SCREEN_MODE, screenModeIndex);
        PlayerPrefs.Save();
    }

    public void SetResolution(int resolutionIndex) {
        if (resolutionIndex < 0 || resolutionIndex >= filteredResolutions.Count)
            return;

        Resolution resolution = filteredResolutions[resolutionIndex];

        // Set screen resolution while maintaining fullscreen state
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        // Save to PlayerPrefs
        PlayerPrefs.SetInt(PREF_RESOLUTION, resolutionIndex);
        PlayerPrefs.Save();
    }

    #endregion

    #region Save/Load Settings

    private void LoadSettings() {
        // Load audio settings
        float musicVolume = PlayerPrefs.GetFloat(PREF_MUSIC_VOLUME, 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, 0.75f);
        bool isMuted = PlayerPrefs.GetInt(PREF_MUTE, 0) == 1;

        // Load screen settings
        int screenMode = PlayerPrefs.GetInt(PREF_SCREEN_MODE, 0); // Default to fullscreen
        int resolutionIndex = PlayerPrefs.GetInt(PREF_RESOLUTION, currentResolutionIndex);

        // Apply audio settings
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
        if (muteToggle != null) muteToggle.isOn = isMuted;

        // Apply screen settings
        if (screenModeDropdown != null) screenModeDropdown.value = screenMode;
        if (resolutionDropdown != null && resolutionIndex < filteredResolutions.Count)
            resolutionDropdown.value = resolutionIndex;

        // Invoke the methods directly to ensure settings are applied
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
        SetMute(isMuted);
        SetScreenMode(screenMode);
        if (resolutionIndex < filteredResolutions.Count)
            SetResolution(resolutionIndex);
    }

    // Call this to reset all settings to default values
    public void ResetToDefaults() {
        // Reset audio
        if (musicVolumeSlider != null) musicVolumeSlider.value = 0.75f;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = 0.75f;
        if (muteToggle != null) muteToggle.isOn = false;

        // Reset screen settings
        if (screenModeDropdown != null) screenModeDropdown.value = 0; // Fullscreen
        if (resolutionDropdown != null) resolutionDropdown.value = currentResolutionIndex;

        // Apply and save all settings
        SetMusicVolume(0.75f);
        SetSFXVolume(0.75f);
        SetMute(false);
        SetScreenMode(0);
        SetResolution(currentResolutionIndex);
    }

    #endregion

    #region UI Helper Methods

    // Call this to show the settings panel
    public void Show() {
        gameObject.SetActive(true);
    }

    // Call this to hide the settings panel
    public void Hide() {
        gameObject.SetActive(false);
    }

    // Toggle visibility of the settings panel
    public void Toggle() {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    #endregion
}