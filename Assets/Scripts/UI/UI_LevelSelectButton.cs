using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectButton : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI fruitInfoText;
    [SerializeField] private TextMeshProUGUI timeInfoText;
    [SerializeField] private Button button;
    [SerializeField] private GameObject lockedOverlay;

    private string sceneName;
    private UI_FadeEffect fadeEffect;
    private bool isUnlocked = true;

    private void Awake() {
        if (button == null)
            button = GetComponent<Button>();

        if (levelNameText == null)
            levelNameText = GetComponentInChildren<TextMeshProUGUI>();

        fadeEffect = Object.FindAnyObjectByType<UI_FadeEffect>();
    }

    private void Start() {
        button.onClick.AddListener(OnButtonClick);
    }

    public void Setup(string levelName, string scene, bool unlocked = true) {
        levelNameText.text = levelName;
        sceneName = scene;
        isUnlocked = unlocked;

        // Enable or disable the button based on whether the level is unlocked
        button.interactable = isUnlocked;

        // If we have a locked overlay, show/hide it
        if (lockedOverlay != null) {
            lockedOverlay.SetActive(!isUnlocked);
        }

        // Update fruit information if we have the fruitInfoText component
        if (fruitInfoText != null) {
            if (isUnlocked) {
                // Check if the level has been played before
                bool levelPlayed = GameManager.instance.saveManager.HasLevelBeenPlayed(scene);

                if (levelPlayed) {
                    // Get the collected and total fruit counts
                    int collectedFruits = GameManager.instance.saveManager.GetCollectedFruitsCount(scene);
                    int totalFruits = GameManager.instance.saveManager.GetTotalFruitsCount(scene);

                    // Display the collected/total fruits
                    fruitInfoText.text = $"Fruits: {collectedFruits}/{totalFruits}";
                }
                else {
                    // Level hasn't been played yet, show unknown values
                    fruitInfoText.text = "Fruits: ???/???";
                }

                fruitInfoText.gameObject.SetActive(true);
            }
            else {
                fruitInfoText.gameObject.SetActive(false);
            }
        }

        // Update time information if we have the timeInfoText component
        if (timeInfoText != null) {
            float bestTime = GameManager.instance.saveManager.GetLevelBestTime(scene);

            if (bestTime > 0f && isUnlocked) {
                // Format time using the TimerManager's format method
                string formattedTime = GameManager.instance.timerManager.FormatTime(bestTime);
                timeInfoText.text = $"Best Time: {formattedTime}";
                timeInfoText.gameObject.SetActive(true);
            }
            else {
                timeInfoText.gameObject.SetActive(false);
            }
        }
    }

    private void OnButtonClick() {
        // Don't allow clicking if the level is locked
        if (!isUnlocked)
            return;

        // Play UI button click sound
        AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");

        if (fadeEffect != null) {
            fadeEffect.ScreenFadeEffect(1f, 1f, LoadLevel);
        }
        else {
            LoadLevel();
        }
    }

    private void LoadLevel() {
        // Use LevelManager to load the scene
        GameManager.instance.levelManager.LoadLevel(sceneName);
    }
}