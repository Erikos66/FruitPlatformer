using UnityEngine;

public class UI_Credits : MonoBehaviour {
    [SerializeField] private RectTransform creditsPanel;
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float OffScreenY = -1000f;
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
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }
}
