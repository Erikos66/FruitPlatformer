using UnityEngine;
using TMPro;

public class UI_InGame : MonoBehaviour {
    public static UI_InGame Instance { get; private set; }
    public UI_FadeEffect FadeEffect { get; private set; }

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI fruitCountText; // Added fruit counter text
    [SerializeField] private GameObject pauseMenu; // Reference to the pause menu UI
    private bool showTimer = true;
    private bool showFruitCount = true;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            FadeEffect = GetComponent<UI_FadeEffect>();
        }
        else {
            Destroy(gameObject);
        }
        FadeEffect = GetComponentInChildren<UI_FadeEffect>();
        if (FadeEffect == null) {
            Debug.LogError("FadeEffect is not assigned in the inspector.");
        }

        if (pauseMenu == null) {
            Debug.LogError("Pause menu is not assigned in the inspector.");
        }

        // Make sure we're active - removing this line that was deactivating the UI
        // this.gameObject.SetActive(false);
    }

    void Start() {
        // Initialize pause menu state - make sure it's initially hidden
        if (pauseMenu != null) {
            pauseMenu.SetActive(false);
        }

        // Start the fade in effect, with player spawning after fade completes
        FadeEffect.ScreenFadeEffect(0f, 1.5f, OnFadeInComplete);

        // Hide timer initially if there is no text component
        if (timerText == null) {
            showTimer = false;
        }

        // Hide fruit counter initially if there is no text component
        if (fruitCountText == null) {
            showFruitCount = false;
        }

        // Initial update of the fruit count
        UpdateFruitCountText();
    }

    // Called when the initial fade-in effect is complete
    private void OnFadeInComplete() {
        // Now spawn the player after fade completes
        if (GameManager.instance != null) {
            GameManager.instance.RespawnPlayer();
        }
    }

    void Update() {
        // Update timer display if we have the text component and timer should be shown
        if (showTimer && timerText != null && GameManager.instance != null) {
            float currentTime = GameManager.instance.GetCurrentLevelTime();

            if (currentTime > 0) {
                // Format time as just seconds (rounded to 2 decimal places)
                float seconds = Mathf.Floor(currentTime * 100) / 100;
                timerText.text = string.Format("{0:0}s", seconds);
            }
            else {
                timerText.text = "00s";
            }
        }

        // Handle pause menu toggle with escape key
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Debug.Log("Escape key pressed"); // Debug log to verify key detection

            if (pauseMenu != null) {
                if (pauseMenu.activeSelf) {
                    ResumeGame(); // Resume the game if pause menu is open
                    Debug.Log("Resuming game - hiding pause menu");
                }
                else {
                    PauseGame(); // Pause the game if pause menu is closed
                    Debug.Log("Pausing game - showing pause menu");
                }
            }
            else {
                Debug.LogError("Pause menu reference is null!");
            }
        }

        // Update fruit count display
        UpdateFruitCountText();
    }

    // Update the fruit count text to show current collection progress
    private void UpdateFruitCountText() {
        if (showFruitCount && fruitCountText != null && GameManager.instance != null) {
            // Get current scene name
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // Get collected and total fruits for the current level
            int collectedFruits = GameManager.instance.GetCollectedFruitsInLevel(currentScene);
            int totalFruits = GameManager.instance.GetTotalFruitsInLevel(currentScene);

            // Display as "collected/total"
            fruitCountText.text = $"{collectedFruits}/{totalFruits}";
        }
    }

    // Method to be called by GameManager when a fruit is collected
    public void OnFruitCollected() {
        UpdateFruitCountText();
    }

    // method to pause the game
    public void PauseGame() {
        Time.timeScale = 0f; // Pause the game
        if (pauseMenu != null) {
            pauseMenu.SetActive(true); // Show pause menu UI
        }
    }

    // method to resume the game
    public void ResumeGame() {
        Time.timeScale = 1f; // Resume the game
        if (pauseMenu != null) {
            pauseMenu.SetActive(false); // Hide pause menu UI
        }
    }

    // method to return to main menu
    public void ReturnToMainMenu() {
        // Make sure to reset time scale before loading a new scene
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}