using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public string sceneName;
    public string creditsSceneName;
    private UI_FadeEffect fadeEffect;

    [SerializeField] private GameObject[] UIElements;
    [SerializeField] private GameObject levelSelectUI; // Reference to the level select UI panel

    private void Awake() {
        fadeEffect = GetComponentInChildren<UI_FadeEffect>();
    }

    private void Start() {
        fadeEffect.ScreenFadeEffect(0f, 1.5f);

        // Check if we should show the level select UI
        if (LevelManager.Instance != null && LevelManager.Instance.showLevelSelectOnMainMenu) {
            // Reset the flag
            LevelManager.Instance.showLevelSelectOnMainMenu = false;

            // Wait a brief moment to ensure UI is fully loaded, then show level select
            Invoke(nameof(ShowLevelSelect), 0.1f);
        }
    }

    private void ShowLevelSelect() {
        // Show the level select UI
        if (levelSelectUI != null) {
            SwitchUI(levelSelectUI);
        }
    }

    public void NewGame() {
        // Play UI button click sound
        AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
        fadeEffect.ScreenFadeEffect(1f, 1.5f, LoadLevelScene);
    }

    public void Credits() {
        // Play UI button click sound
        AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
        fadeEffect.ScreenFadeEffect(1f, 1.5f, LoadCreditsScene);
    }
    private void LoadLevelScene() {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame() {
        // Play UI button click sound
        AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
        Application.Quit();
        Debug.Log("Quit Game");
    }

    private void LoadCreditsScene() {
        SceneManager.LoadScene(creditsSceneName);
    }

    public void SwitchUI(GameObject uiToEnable) {
        // Play UI button click sound
        AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
        foreach (GameObject uiElement in UIElements) {
            uiElement.SetActive(false);
        }
        uiToEnable.SetActive(true);
    }

    /// <summary>
    /// Unlocks all levels in the game and refreshes the level select UI.
    /// Assign this to a button in the Unity Editor.
    /// </summary>
    public void UnlockAllLevels() {
        // Play UI button click sound
        AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
        LevelManager.Instance.UnlockAllLevels();
        Debug.Log("All levels have been unlocked!");
    }
}
