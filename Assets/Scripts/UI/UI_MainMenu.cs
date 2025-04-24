using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public string sceneName;
    public string creditsSceneName;
    private UI_FadeEffect fadeEffect;

    [SerializeField] private GameObject[] UIElements;

    private void Awake() {
        fadeEffect = GetComponentInChildren<UI_FadeEffect>();

    }

    private void Start() {
        fadeEffect.ScreenFadeEffect(0f, 1.5f);
    }

    public void NewGame() {
        fadeEffect.ScreenFadeEffect(1f, 1.5f, LoadLevelScene);
    }

    public void Credits() {
        fadeEffect.ScreenFadeEffect(1f, 1.5f, LoadCreditsScene);
    }

    private void LoadLevelScene() {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame() {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    private void LoadCreditsScene() {
        SceneManager.LoadScene(creditsSceneName);
    }

    public void SwitchUI(GameObject uiToEnable) {
        foreach (GameObject uiElement in UIElements) {
            uiElement.SetActive(false);
        }
        uiToEnable.SetActive(true);
    }
}
