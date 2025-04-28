using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    // References to all specialized managers
    [HideInInspector] public FruitManager fruitManager;
    [HideInInspector] public PlayerManager playerManager;
    [HideInInspector] public LevelManager levelManager;
    [HideInInspector] public CameraManager cameraManager;
    [HideInInspector] public UIManager uiManager;
    [HideInInspector] public SaveManager saveManager;
    [HideInInspector] public TimerManager timerManager;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    private void InitializeManagers() {
        // Create and initialize all specialized managers
        fruitManager = gameObject.AddComponent<FruitManager>();
        playerManager = gameObject.AddComponent<PlayerManager>();
        levelManager = gameObject.AddComponent<LevelManager>();
        cameraManager = gameObject.AddComponent<CameraManager>();
        uiManager = gameObject.AddComponent<UIManager>();
        saveManager = gameObject.AddComponent<SaveManager>();
        timerManager = gameObject.AddComponent<TimerManager>();
    }
}
