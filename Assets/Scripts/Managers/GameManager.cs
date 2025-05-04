using UnityEngine;

public class GameManager : MonoBehaviour {
	// Singleton instance
	public static GameManager Instance { get; private set; }

	// Difficulty enum
	public enum GameDifficulty { Easy, Normal }

	// Current difficulty setting
	private GameDifficulty _currentDifficulty = GameDifficulty.Easy;
	public GameDifficulty CurrentDifficulty {
		get { return _currentDifficulty; }
		set {
			_currentDifficulty = value;
			// Save difficulty whenever it's changed
			if (SaveManager.Instance != null)
				SaveManager.Instance.SaveGameDifficulty(_currentDifficulty);
		}
	}

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

		// Load saved difficulty
		if (SaveManager.Instance != null) {
			_currentDifficulty = SaveManager.Instance.GetGameDifficulty();
		}
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

	// Helper methods to check difficulty
	public bool IsEasyMode() => _currentDifficulty == GameDifficulty.Easy;
	public bool IsNormalMode() => _currentDifficulty == GameDifficulty.Normal;
}
