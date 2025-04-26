using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles navigation and interaction with different settings panels
/// </summary>
public class UI_SettingsMenu : MonoBehaviour {
    [SerializeField] private GameObject audioSettingsPanel;
    [SerializeField] private GameObject videoSettingsPanel;
    [SerializeField] private GameObject controlsSettingsPanel;

    [SerializeField] private Button audioTabButton;
    [SerializeField] private Button videoTabButton;
    [SerializeField] private Button controlsTabButton;

    [SerializeField] private Color selectedTabColor = new Color(0.75f, 0.75f, 1f);
    [SerializeField] private Color normalTabColor = Color.white;

    private UI_Settings settingsManager;

    private void Awake() {
        settingsManager = GetComponent<UI_Settings>();

        // Set up button listeners
        if (audioTabButton != null)
            audioTabButton.onClick.AddListener(() => ShowPanel(PanelType.Audio));

        if (videoTabButton != null)
            videoTabButton.onClick.AddListener(() => ShowPanel(PanelType.Video));

        if (controlsTabButton != null)
            controlsTabButton.onClick.AddListener(() => ShowPanel(PanelType.Controls));
    }

    private void Start() {
        // Default to audio settings panel
        ShowPanel(PanelType.Audio);
    }

    public enum PanelType {
        Audio,
        Video,
        Controls
    }

    public void ShowPanel(PanelType panel) {
        // Deactivate all panels first
        if (audioSettingsPanel != null) audioSettingsPanel.SetActive(false);
        if (videoSettingsPanel != null) videoSettingsPanel.SetActive(false);
        if (controlsSettingsPanel != null) controlsSettingsPanel.SetActive(false);

        // Reset button colors
        if (audioTabButton != null) audioTabButton.image.color = normalTabColor;
        if (videoTabButton != null) videoTabButton.image.color = normalTabColor;
        if (controlsTabButton != null) controlsTabButton.image.color = normalTabColor;

        // Activate the requested panel and highlight the button
        switch (panel) {
            case PanelType.Audio:
                if (audioSettingsPanel != null) audioSettingsPanel.SetActive(true);
                if (audioTabButton != null) audioTabButton.image.color = selectedTabColor;
                break;

            case PanelType.Video:
                if (videoSettingsPanel != null) videoSettingsPanel.SetActive(true);
                if (videoTabButton != null) videoTabButton.image.color = selectedTabColor;
                break;

            case PanelType.Controls:
                if (controlsSettingsPanel != null) controlsSettingsPanel.SetActive(true);
                if (controlsTabButton != null) controlsTabButton.image.color = selectedTabColor;
                break;
        }
    }

    public void OnBackButtonClicked() {
        // Hide the entire settings menu
        gameObject.SetActive(false);
    }

    public void OnResetButtonClicked() {
        if (settingsManager != null) {
            settingsManager.ResetToDefaults();
        }
    }
}