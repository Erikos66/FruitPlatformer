using UnityEngine;

public class UI_Credits : MonoBehaviour {
    [SerializeField] private RectTransform creditsPanel;
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float OffScreenY = -1000f;
    private UI_FadeEffect fadeEffect;


    void Start() {

        fadeEffect.ScreenFadeEffect(0f, 1.5f);

    }

    private void Awake() {

        fadeEffect = GetComponentInChildren<UI_FadeEffect>();
        if (fadeEffect == null) {
            Debug.LogError("FadeEffect is not assigned in the inspector.");
        }

    }

    private bool isSkipped = false;

    private void Update() {
        creditsPanel.anchoredPosition += scrollSpeed * Time.deltaTime * Vector2.up;

        if (creditsPanel.anchoredPosition.y > OffScreenY) {
            GoToMainMenu();
        }
    }

    public void SkipCredits() {
        if (!isSkipped) {
            scrollSpeed *= 10f;
            isSkipped = true;
        }
        else {
            GoToMainMenu();
        }
    }

    public void GoToMainMenu() {
        fadeEffect.ScreenFadeEffect(1f, 1.5f, () => {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        });
    }
}
