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

        Debug.Log("Fruit collected! Total: " + fruitsCollected);
    }

    public bool AllowedRandomFuits() => randomFruitsAllowed;

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

    public void RespawnPlayer() {
        if (!currentSpawnPoint) {
            Debug.LogWarning("Current spawn point not set! Using default StartPoint.");
            currentSpawnPoint = startPoint;
        }
        StartCoroutine(RespawnPlayerCoroutine());
    }

    private void LoadCredits() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("The_End");
    }

    public void LevelFinished() {
        Debug.Log("Level Finished!");
        UI_InGame.Instance.FadeEffect.ScreenFadeEffect(1f, 1f, () => { LoadCredits(); });
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
}
