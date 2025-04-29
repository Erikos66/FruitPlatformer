using UnityEngine;

public class GameManager : MonoBehaviour {
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Public properties for easy access to common managers
    public GameObject[] managerObjects;

    private void Awake() {
        // Singleton setup
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
        }
        else if (Instance != this) {
            Destroy(gameObject);
        }
        // Initialize managers
        InitializeManagers();
    }

    private void InitializeManagers() {
        // instantiate manager objects if they are not already in the scene
        foreach (var managerObject in managerObjects) {
            if (managerObject != null) {
                var managerType = managerObject.GetComponent<MonoBehaviour>().GetType();
                if (FindFirstObjectByType(managerType) == null) {
                    Instantiate(managerObject);
                }
            }
        }
    }
}
