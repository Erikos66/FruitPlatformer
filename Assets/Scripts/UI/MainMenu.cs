using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public string sceneName;


    public void NewGame() {
        SceneManager.LoadScene(sceneName);
    }

}
