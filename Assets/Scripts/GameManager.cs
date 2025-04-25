using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    [Header("Fruit Manager")]
    public int fruitsCollected;
    public bool randomFruitsAllowed;
    public int totalFruits;
    public Fruit[] allFruits;

    [SerializeField] private LevelDataSO levelData;  // Reference to the LevelDataSO

    // Dictionary to store total fruits per level (not serialized, calculated at runtime)
    private Dictionary<string, int> totalFruitsPerLevel = new Dictionary<string, int>();

    // Dictionary to store collected fruits per level (not serialized, loaded from PlayerPrefs)
    private Dictionary<string, int> collectedFruitsPerLevel = new Dictionary<string, int>();

    [Header("Player")]
    public Player player;
    public GameObject playerPrefab;
    public GameObject currentSpawnPoint;
    public GameObject startPoint;
    public float RespawnDelay = 1f;

    [Header("PlayerSkins")]
    public int selectedSkinIndex = 0;

    [Header("Level Timer")]
    private float levelStartTime;
    private bool isTimerRunning = false;
    private Dictionary<string, float> bestLevelTimes = new Dictionary<string, float>();

    // Add this field to track if we should show level select when returning to main menu
    [HideInInspector] public bool showLevelSelectOnMainMenu = false;

    // Name of the main menu scene
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void OnEnable() {
        // Register for scene loaded events to update fruit info
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        // Load saved fruit collection data
        LoadFruitCollectionData();

        // Load saved level completion times
        LoadLevelCompletionTimes();

        // Pre-populate total fruits per level from LevelDataSO if available
        if (levelData != null) {
            PreloadFruitCounts();
        }
        else {
            Debug.LogWarning("LevelDataSO not assigned in GameManager. Total fruit counts won't be available until levels are loaded.");
        }
    }

    // Preload all fruit counts from the LevelDataSO
    private void PreloadFruitCounts() {
        // Find all scenes in build settings
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            // Only process level scenes
            if (sceneName.StartsWith("Level_")) {
                int fruitCount = levelData.GetTotalFruitsInLevel(sceneName);
                if (fruitCount > 0) {
                    totalFruitsPerLevel[sceneName] = fruitCount;
                }
            }
        }
    }

    private void OnDisable() {
        // Unregister from scene loaded events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Score() {
        Debug.Log("Score!");
    }

    public void AddFruit() {
        fruitsCollected++;

        // Store the collected fruit for the current level
        string currentScene = SceneManager.GetActiveScene().name;
        if (collectedFruitsPerLevel.ContainsKey(currentScene)) {
            collectedFruitsPerLevel[currentScene]++;
        }
        else {
            collectedFruitsPerLevel[currentScene] = 1;
        }

        // Save to PlayerPrefs
        SaveFruitCollectionData();

        // Notify UI to update fruit count display
        if (UI_InGame.Instance != null) {
            UI_InGame.Instance.OnFruitCollected();
        }

        Debug.Log("Fruit collected! Total: " + fruitsCollected);
    }

    public bool AllowedRandomFuits() => randomFruitsAllowed;

    public void RespawnPlayer() {
        if (!currentSpawnPoint) {
            Debug.LogWarning("Current spawn point not set! Using default StartPoint.");
            currentSpawnPoint = startPoint;
        }
        StartCoroutine(RespawnPlayerCoroutine());
    }

    private IEnumerator RespawnPlayerCoroutine() {
        Transform playerCurrentSpawnPoint = currentSpawnPoint.transform;

        yield return new WaitForSeconds(RespawnDelay);

        if (player) {
            player.Die();
        }

        if (currentSpawnPoint.TryGetComponent<StartPoint>(out var startPoint)) {
            startPoint.AnimateFlag();
            playerCurrentSpawnPoint = startPoint.respawnPoint;
        }

        GameObject newPlayer = Instantiate(playerPrefab, playerCurrentSpawnPoint.position, playerCurrentSpawnPoint.rotation);
        player = newPlayer.GetComponent<Player>();

        // Start the level timer when the player is spawned
        StartLevelTimer();
    }

    private void Start() {
        CollectFruitInfo();
        Debug.Log("Total fruits: " + totalFruits);
    }

    // Called when a scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Only collect fruit info for levels (not menu scenes)
        if (scene.name.StartsWith("Level_")) {
            CollectFruitInfo();
            // Reset timer state when a new level is loaded
            isTimerRunning = false;
            levelStartTime = 0f;
        }
    }

    private void CollectFruitInfo() {
        allFruits = new Fruit[0];
        allFruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);
        totalFruits = allFruits.Length;

        // Store the total fruits for this level
        string currentScene = SceneManager.GetActiveScene().name;
        totalFruitsPerLevel[currentScene] = totalFruits;
    }

    private void LoadMainMenu() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LevelFinished() {
        Debug.Log("Level Finished!");

        // Stop the timer and save the time if it was running
        if (isTimerRunning) {
            StopLevelTimer();
        }

        // Set flag to show level select UI when returning to main menu
        showLevelSelectOnMainMenu = true;

        // Fade out and load main menu instead of credits
        UI_InGame.Instance.FadeEffect.ScreenFadeEffect(1f, 1f, () => { LoadMainMenu(); });
    }

    // Get the number of fruits collected in a specific level
    public int GetCollectedFruitsInLevel(string levelName) {
        if (collectedFruitsPerLevel.ContainsKey(levelName)) {
            return collectedFruitsPerLevel[levelName];
        }
        return 0;
    }

    // Get the total number of fruits in a specific level
    public int GetTotalFruitsInLevel(string levelName) {
        // Try to get from runtime dictionary first
        if (totalFruitsPerLevel.ContainsKey(levelName)) {
            return totalFruitsPerLevel[levelName];
        }

        // If not found and we have LevelDataSO, try from there
        if (levelData != null) {
            int fruitCount = levelData.GetTotalFruitsInLevel(levelName);
            if (fruitCount > 0) {
                return fruitCount;
            }
        }

        return 0;
    }

    // Save fruit collection data to PlayerPrefs
    private void SaveFruitCollectionData() {
        foreach (var entry in collectedFruitsPerLevel) {
            PlayerPrefs.SetInt("CollectedFruits_" + entry.Key, entry.Value);
        }
        PlayerPrefs.Save();
    }

    // Load fruit collection data from PlayerPrefs
    private void LoadFruitCollectionData() {
        collectedFruitsPerLevel.Clear();

        // This will load data for all levels that have been played
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            // Only load data for level scenes
            if (sceneName.StartsWith("Level_")) {
                int collected = PlayerPrefs.GetInt("CollectedFruits_" + sceneName, 0);
                if (collected > 0) {
                    collectedFruitsPerLevel[sceneName] = collected;
                }

                int total = PlayerPrefs.GetInt("TotalFruits_" + sceneName, 0);
                if (total > 0) {
                    totalFruitsPerLevel[sceneName] = total;
                }
            }
        }
    }

    // Get the current level time (while the level is being played)
    public float GetCurrentLevelTime() {
        if (isTimerRunning) {
            return Time.time - levelStartTime;
        }
        return 0f;
    }

    // Start the level timer
    public void StartLevelTimer() {
        if (!isTimerRunning) {
            levelStartTime = Time.time;
            isTimerRunning = true;
            Debug.Log("Level timer started");
        }
    }

    // Stop the level timer and save the completion time
    public void StopLevelTimer() {
        if (isTimerRunning) {
            float completionTime = Time.time - levelStartTime;
            string currentScene = SceneManager.GetActiveScene().name;

            // Only save the time if it's better than the previous best (or if there's no previous time)
            if (!bestLevelTimes.ContainsKey(currentScene) || completionTime < bestLevelTimes[currentScene]) {
                bestLevelTimes[currentScene] = completionTime;
                PlayerPrefs.SetFloat("LevelTime_" + currentScene, completionTime);
                PlayerPrefs.Save();
                Debug.Log($"New best time for {currentScene}: {completionTime:F2} seconds");
            }

            isTimerRunning = false;
        }
    }

    // Get the best completion time for a specific level
    public float GetBestLevelTime(string levelName) {
        if (bestLevelTimes.ContainsKey(levelName)) {
            return bestLevelTimes[levelName];
        }
        return 0f; // Return 0 if no time has been recorded
    }

    // Has the level been completed?
    public bool HasLevelBeenCompleted(string levelName) {
        return bestLevelTimes.ContainsKey(levelName) && bestLevelTimes[levelName] > 0f;
    }

    // Load saved level completion times
    private void LoadLevelCompletionTimes() {
        bestLevelTimes.Clear();

        // This will load time data for all levels that have been played
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            // Only load data for level scenes
            if (sceneName.StartsWith("Level_")) {
                float time = PlayerPrefs.GetFloat("LevelTime_" + sceneName, 0f);
                if (time > 0f) {
                    bestLevelTimes[sceneName] = time;
                }
            }
        }
    }
}
