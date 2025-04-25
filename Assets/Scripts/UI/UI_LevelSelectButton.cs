using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectButton : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI fruitInfoText;
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