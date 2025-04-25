using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectButton : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI fruitInfoText;
    [SerializeField] private TextMeshProUGUI timeInfoText;
    [SerializeField] private Button button;

    private string sceneName;
    private UI_FadeEffect fadeEffect;

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

    public void Setup(string levelName, string scene) {
        levelNameText.text = levelName;
        sceneName = scene;

        // Update fruit information if we have the fruitInfoText component
        if (fruitInfoText != null) {
            int collectedFruits = GameManager.instance.GetCollectedFruitsInLevel(scene);
            int totalFruits = GameManager.instance.GetTotalFruitsInLevel(scene);

            if (totalFruits > 0) {
                fruitInfoText.text = $"Fruits: {collectedFruits}/{totalFruits}";
                fruitInfoText.gameObject.SetActive(true);
            }
            else {
                fruitInfoText.gameObject.SetActive(false);
            }
        }

        // Update time information if we have the timeInfoText component
        if (timeInfoText != null) {
            float bestTime = GameManager.instance.GetBestLevelTime(scene);

            if (bestTime > 0f) {
                // Format time as minutes:seconds.milliseconds
                int minutes = Mathf.FloorToInt(bestTime / 60f);
                int seconds = Mathf.FloorToInt(bestTime % 60f);
                int milliseconds = Mathf.FloorToInt((bestTime * 100f) % 100f);

                timeInfoText.text = $"Best Time: {minutes}:{seconds:00}.{milliseconds:00}";
                timeInfoText.gameObject.SetActive(true);
            }
            else {
                timeInfoText.gameObject.SetActive(false);
            }
        }
    }

    private void OnButtonClick() {
        if (fadeEffect != null) {
            fadeEffect.ScreenFadeEffect(1f, 1f, LoadLevel);
        }
        else {
            LoadLevel();
        }
    }

    private void LoadLevel() {
        SceneManager.LoadScene(sceneName);
    }
}