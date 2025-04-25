using UnityEngine;
using TMPro;

public class UI_InGame : MonoBehaviour {
    public static UI_InGame Instance { get; private set; }
    public UI_FadeEffect FadeEffect { get; private set; }

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI fruitCountText; // Added fruit counter text
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
    }

    void Start() {
        FadeEffect.ScreenFadeEffect(0f, 1.5f, GameManager.instance.RespawnPlayer);

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

        // Update fruit count display
        UpdateFruitCountText();
    }

    // Update the fruit count text to show current collection progress
    private void UpdateFruitCountText() {
        if (showFruitCount && fruitCountText != null && GameManager.instance != null) {
            // Get current scene name
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // Get collected and total fruits for the current level
            int collectedFruits = GameManager.instance.fruitsCollected;
            int totalFruits = GameManager.instance.totalFruits;

            // Display as "collected/total"
            fruitCountText.text = $"{collectedFruits}/{totalFruits}";
        }
    }

    // Method to be called by GameManager when a fruit is collected
    public void OnFruitCollected() {
        UpdateFruitCountText();
    }
}