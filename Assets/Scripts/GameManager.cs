using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{

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

    [Header("Camera")]
    [SerializeField] private CinemachineCamera virtualCamera; // Reference to Cinemachine camera

    [Header("PlayerSkins")]
    public int selectedSkinIndex = 0;

    [Header("Level Timer")]
    private float levelStartTime;
    private bool isTimerRunning = false;
    private Dictionary<string, float> bestLevelTimes = new Dictionary<string, float>();

    // Level progression tracking
    private HashSet<string> unlockedLevels = new HashSet<string>();
    private List<string> orderedLevelNames = new List<string>();

    // Add this field to track if we should show level select when returning to main menu
    [HideInInspector] public bool showLevelSelectOnMainMenu = false;

    // Name of the main menu scene
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Register for scene loaded events to update fruit info
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Load saved fruit collection data
        LoadFruitCollectionData();

        // Load saved level completion times
        LoadLevelCompletionTimes();

        // Load unlocked levels
        LoadUnlockedLevels();

        // Pre-populate total fruits per level from LevelDataSO if available
        if (levelData != null)
        {
            PreloadFruitCounts();
        }
        else
        {
            Debug.LogWarning("LevelDataSO not assigned in GameManager. Total fruit counts won't be available until levels are loaded.");
        }

        // Build ordered list of level names
        BuildOrderedLevelList();
    }

    // Build an ordered list of all level names in the build settings
    private void BuildOrderedLevelList()
    {
        orderedLevelNames.Clear();

        // Find all scenes in build settings
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            // Only process level scenes
            if (sceneName.StartsWith("Level_"))
            {
                orderedLevelNames.Add(sceneName);
            }
        }

        // Sort levels by their number
        orderedLevelNames.Sort((a, b) =>
        {
            string aNum = a.Substring("Level_".Length);
            string bNum = b.Substring("Level_".Length);

            if (int.TryParse(aNum, out int aVal) && int.TryParse(bNum, out int bVal))
            {
                return aVal.CompareTo(bVal);
            }
            return string.Compare(a, b);
        });

        // Always ensure Level_1 is unlocked
        if (orderedLevelNames.Count > 0)
        {
            UnlockLevel(orderedLevelNames[0]);
        }
    }

    // Preload all fruit counts from the LevelDataSO
    private void PreloadFruitCounts()
    {
        // Find all scenes in build settings
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            // Only process level scenes
            if (sceneName.StartsWith("Level_"))
            {
                int fruitCount = levelData.GetTotalFruitsInLevel(sceneName);
                if (fruitCount > 0)
                {
                    totalFruitsPerLevel[sceneName] = fruitCount;
                }
            }
        }
    }

    private void OnDisable()
    {
        // Unregister from scene loaded events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Score()
    {
        Debug.Log("Score!");
    }

    public void AddFruit()
    {
        fruitsCollected++;

        // Store the collected fruit for the current level
        string currentScene = SceneManager.GetActiveScene().name;
        if (collectedFruitsPerLevel.ContainsKey(currentScene))
        {
            collectedFruitsPerLevel[currentScene]++;
        }
        else
        {
            collectedFruitsPerLevel[currentScene] = 1;
        }

        // Save to PlayerPrefs
        SaveFruitCollectionData();

        // Notify UI to update fruit count display
        if (UI_InGame.Instance != null)
        {
            UI_InGame.Instance.OnFruitCollected();
        }

        Debug.Log("Fruit collected! Total: " + fruitsCollected);
    }

    public bool AllowedRandomFuits() => randomFruitsAllowed;

    public void RespawnPlayer()
    {
        // Validate we have a player prefab assigned
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned in GameManager! Cannot respawn player.");
            return;
        }

        // Validate spawn point
        if (currentSpawnPoint == null)
        {
            Debug.LogWarning("Current spawn point not set! Looking for startPoint...");

            if (startPoint == null)
            {
                // Try to find any StartPoint in the scene
                StartPoint[] points = FindObjectsByType<StartPoint>(FindObjectsSortMode.None);
                if (points.Length > 0)
                {
                    currentSpawnPoint = points[0].gameObject;
                    Debug.Log($"Found StartPoint '{currentSpawnPoint.name}' to use as spawn point.");
                }
                else
                {
                    Debug.LogError("No spawn points found in the level! Cannot respawn player.");
                    return;
                }
            }
            else
            {
                currentSpawnPoint = startPoint;
                Debug.Log("Using default startPoint for respawning.");
            }
        }

        StartCoroutine(RespawnPlayerCoroutine());
    }

    private IEnumerator RespawnPlayerCoroutine()
    {
        Transform playerCurrentSpawnPoint = currentSpawnPoint.transform;

        yield return new WaitForSeconds(RespawnDelay);

        if (player)
        {
            player.Die();
        }

        if (currentSpawnPoint.TryGetComponent<StartPoint>(out var startPoint))
        {
            startPoint.AnimateFlag();
            playerCurrentSpawnPoint = startPoint.respawnPoint;
        }

        GameObject newPlayer = Instantiate(playerPrefab, playerCurrentSpawnPoint.position, playerCurrentSpawnPoint.rotation);
        player = newPlayer.GetComponent<Player>();

        // Set the Cinemachine virtual camera's follow target to the new player
        if (virtualCamera != null)
        {
            virtualCamera.Follow = player.transform;
            Debug.Log("Cinemachine camera now following the new player");
        }
        else
        {
            Debug.LogWarning("Virtual camera reference is missing on GameManager. Camera won't follow the player.");
        }

        // Start the level timer when the player is spawned
        StartLevelTimer();
    }

    private void Start()
    {
        CollectFruitInfo();
        Debug.Log("Total fruits: " + totalFruits);
    }

    // Called when a scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only collect fruit info and handle player spawning for levels (not menu scenes)
        if (scene.name.StartsWith("Level_"))
        {
            CollectFruitInfo();
            // Reset timer state when a new level is loaded
            isTimerRunning = false;
            levelStartTime = 0f;

            // Find and set up spawn points for the new level
            SetupSpawnPointsForLevel();
        }
    }

    private void CollectFruitInfo()
    {
        allFruits = new Fruit[0];
        allFruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);
        totalFruits = allFruits.Length;

        // Store the total fruits for this level
        string currentScene = SceneManager.GetActiveScene().name;
        totalFruitsPerLevel[currentScene] = totalFruits;
    }

    private void LoadMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    // Find the next level in sequence
    private string GetNextLevelName(string currentLevelName)
    {
        int currentIndex = orderedLevelNames.IndexOf(currentLevelName);

        // If the current level is found and it's not the last one
        if (currentIndex >= 0 && currentIndex < orderedLevelNames.Count - 1)
        {
            return orderedLevelNames[currentIndex + 1];
        }

        // If it's the last level or not found, return empty string
        return string.Empty;
    }

    public void LoadNextLevel(string currentLevelName)
    {
        string nextLevelName = GetNextLevelName(currentLevelName);

        if (!string.IsNullOrEmpty(nextLevelName))
        {
            // Unlock the next level
            UnlockLevel(nextLevelName);

            // Load the next level
            SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            // If there's no next level, go back to the menu
            LoadMainMenu();
        }
    }

    public void LevelFinished()
    {
        Debug.Log("Level Finished!");

        // Stop the timer and save the time if it was running
        if (isTimerRunning)
        {
            StopLevelTimer();
        }

        string currentScene = SceneManager.GetActiveScene().name;

        // Mark level as completed in PlayerPrefs
        UnlockLevel(currentScene);

        // Get the next level name
        string nextLevelName = GetNextLevelName(currentScene);

        // If there is a next level
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            // Unlock the next level
            UnlockLevel(nextLevelName);

            // Fade out and load the next level
            UI_InGame.Instance.FadeEffect.ScreenFadeEffect(1f, 1f, () => { SceneManager.LoadScene(nextLevelName); });
        }
        else
        {
            // If this was the last level, set flag to show level select UI when returning to main menu
            showLevelSelectOnMainMenu = true;

            // Fade out and load main menu
            UI_InGame.Instance.FadeEffect.ScreenFadeEffect(1f, 1f, () => { LoadMainMenu(); });
        }
    }

    // Get the number of fruits collected in a specific level
    public int GetCollectedFruitsInLevel(string levelName)
    {
        if (collectedFruitsPerLevel.ContainsKey(levelName))
        {
            return collectedFruitsPerLevel[levelName];
        }
        return 0;
    }

    // Get the total number of fruits in a specific level
    public int GetTotalFruitsInLevel(string levelName)
    {
        // Try to get from runtime dictionary first
        if (totalFruitsPerLevel.ContainsKey(levelName))
        {
            return totalFruitsPerLevel[levelName];
        }

        // If not found and we have LevelDataSO, try from there
        if (levelData != null)
        {
            int fruitCount = levelData.GetTotalFruitsInLevel(levelName);
            if (fruitCount > 0)
            {
                return fruitCount;
            }
        }

        return 0;
    }

    // Save fruit collection data to PlayerPrefs
    private void SaveFruitCollectionData()
    {
        foreach (var entry in collectedFruitsPerLevel)
        {
            PlayerPrefs.SetInt("CollectedFruits_" + entry.Key, entry.Value);
        }
        PlayerPrefs.Save();
    }

    // Load fruit collection data from PlayerPrefs
    private void LoadFruitCollectionData()
    {
        collectedFruitsPerLevel.Clear();

        // This will load data for all levels that have been played
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            // Only load data for level scenes
            if (sceneName.StartsWith("Level_"))
            {
                int collected = PlayerPrefs.GetInt("CollectedFruits_" + sceneName, 0);
                if (collected > 0)
                {
                    collectedFruitsPerLevel[sceneName] = collected;
                }

                int total = PlayerPrefs.GetInt("TotalFruits_" + sceneName, 0);
                if (total > 0)
                {
                    totalFruitsPerLevel[sceneName] = total;
                }
            }
        }
    }

    // Get the current level time (while the level is being played)
    public float GetCurrentLevelTime()
    {
        if (isTimerRunning)
        {
            return Time.time - levelStartTime;
        }
        return 0f;
    }

    // Start the level timer
    public void StartLevelTimer()
    {
        if (!isTimerRunning)
        {
            levelStartTime = Time.time;
            isTimerRunning = true;
            Debug.Log("Level timer started");
        }
    }

    // Stop the level timer and save the completion time
    public void StopLevelTimer()
    {
        if (isTimerRunning)
        {
            float completionTime = Time.time - levelStartTime;
            string currentScene = SceneManager.GetActiveScene().name;

            // Only save the time if it's better than the previous best (or if there's no previous time)
            if (!bestLevelTimes.ContainsKey(currentScene) || completionTime < bestLevelTimes[currentScene])
            {
                bestLevelTimes[currentScene] = completionTime;
                PlayerPrefs.SetFloat("LevelTime_" + currentScene, completionTime);
                PlayerPrefs.Save();
                Debug.Log($"New best time for {currentScene}: {completionTime:F2} seconds");
            }

            isTimerRunning = false;
        }
    }

    // Get the best completion time for a specific level
    public float GetBestLevelTime(string levelName)
    {
        if (bestLevelTimes.ContainsKey(levelName))
        {
            return bestLevelTimes[levelName];
        }
        return 0f; // Return 0 if no time has been recorded
    }

    // Has the level been completed?
    public bool HasLevelBeenCompleted(string levelName)
    {
        return bestLevelTimes.ContainsKey(levelName) && bestLevelTimes[levelName] > 0f;
    }

    // Load saved level completion times
    private void LoadLevelCompletionTimes()
    {
        bestLevelTimes.Clear();

        // This will load time data for all levels that have been played
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            // Only load data for level scenes
            if (sceneName.StartsWith("Level_"))
            {
                float time = PlayerPrefs.GetFloat("LevelTime_" + sceneName, 0f);
                if (time > 0f)
                {
                    bestLevelTimes[sceneName] = time;
                }
            }
        }
    }

    // LEVEL UNLOCKING METHODS

    // Check if a level is unlocked
    public bool IsLevelUnlocked(string levelName)
    {
        // First level is always unlocked
        if (orderedLevelNames.Count > 0 && orderedLevelNames[0] == levelName)
        {
            return true;
        }

        return unlockedLevels.Contains(levelName);
    }

    // Unlock a specific level
    public void UnlockLevel(string levelName)
    {
        if (!unlockedLevels.Contains(levelName))
        {
            unlockedLevels.Add(levelName);
            PlayerPrefs.SetInt("UnlockedLevel_" + levelName, 1);
            PlayerPrefs.Save();
        }
    }

    // Unlock all levels
    public void UnlockAllLevels()
    {
        foreach (string levelName in orderedLevelNames)
        {
            UnlockLevel(levelName);
        }
        Debug.Log("All levels unlocked!");
    }

    // Load unlocked levels from PlayerPrefs
    private void LoadUnlockedLevels()
    {
        unlockedLevels.Clear();

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneName.StartsWith("Level_"))
            {
                if (PlayerPrefs.GetInt("UnlockedLevel_" + sceneName, 0) == 1)
                {
                    unlockedLevels.Add(sceneName);
                }
            }
        }
    }

    // Find and setup spawn points when entering a new level
    private void SetupSpawnPointsForLevel()
    {
        // Find all StartPoint objects in the scene
        StartPoint[] startPoints = FindObjectsByType<StartPoint>(FindObjectsSortMode.None);

        if (startPoints.Length == 0)
        {
            Debug.LogError("No StartPoint found in the scene! Player cannot spawn. Please add a StartPoint component to a GameObject in the level.");
            return;
        }

        // Set the first one as the default startPoint
        startPoint = startPoints[0].gameObject;

        // Validate all StartPoints to ensure they have valid respawn points
        foreach (StartPoint sp in startPoints)
        {
            if (sp.respawnPoint == null)
            {
                Debug.LogError($"StartPoint '{sp.gameObject.name}' has no respawnPoint assigned! Please assign a Transform as respawnPoint in the inspector.");
            }
        }

        // Set the currentSpawnPoint to the startPoint by default
        currentSpawnPoint = startPoint;

        // Find and set the Cinemachine camera if not already assigned
        if (virtualCamera == null)
        {
            // Try to find a game object specifically named "CineCamera"
            GameObject cineCameraObject = GameObject.Find("CineCamera");
            if (cineCameraObject != null && cineCameraObject.TryGetComponent<CinemachineCamera>(out var cineCamera))
            {
                virtualCamera = cineCamera;
                Debug.Log($"Found Cinemachine camera named 'CineCamera' and set it as the virtual camera reference.");
            }
            else
            {
                // If we can't find one named "CineCamera", look for any CinemachineCamera component in the scene
                CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
                if (cameras.Length > 0)
                {
                    virtualCamera = cameras[0];
                    Debug.Log($"Found Cinemachine camera '{virtualCamera.gameObject.name}' and set it as the virtual camera reference.");
                }
                else
                {
                    Debug.LogWarning("No Cinemachine camera found in the scene. Player won't be followed by camera.");
                }
            }
        }

        Debug.Log($"Found {startPoints.Length} spawn point(s). Using '{startPoint.name}' as default.");
    }

    // Spawn the player at the appropriate start point
    private void SpawnPlayerAtStartPoint()
    {
        // Validate playerPrefab exists
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned in GameManager! Cannot spawn player.");
            return;
        }

        // Validate that we have a start point
        if (startPoint == null)
        {
            Debug.LogError("No start point found in the level! Cannot spawn player.");
            return;
        }

        // Validate the StartPoint component and respawn point
        if (!startPoint.TryGetComponent<StartPoint>(out var startPointComponent))
        {
            Debug.LogError($"GameObject '{startPoint.name}' doesn't have a StartPoint component! Cannot spawn player.");
            return;
        }

        if (startPointComponent.respawnPoint == null)
        {
            Debug.LogError($"StartPoint '{startPoint.name}' has no respawnPoint assigned! Cannot spawn player.");
            return;
        }

        // Destroy any existing player
        if (player != null)
        {
            Destroy(player.gameObject);
            player = null;
        }

        // Instantiate the player and get the component
        Vector3 spawnPosition = startPointComponent.respawnPoint.position;
        Quaternion spawnRotation = startPointComponent.respawnPoint.rotation;

        Debug.Log($"Spawning player at position: {spawnPosition}");
        GameObject playerObject = Instantiate(playerPrefab, spawnPosition, spawnRotation);

        if (playerObject.TryGetComponent<Player>(out var playerComponent))
        {
            player = playerComponent;
            startPointComponent.AnimateFlag();

            // Set the Cinemachine virtual camera's follow target to the new player
            if (virtualCamera != null)
            {
                virtualCamera.Follow = player.transform;
                Debug.Log("Cinemachine camera now following the initial player");
            }
            else
            {
                Debug.LogWarning("Virtual camera reference is missing on GameManager. Camera won't follow the player.");
            }

            StartLevelTimer();
            Debug.Log("Player spawned successfully!");
        }
        else
        {
            Debug.LogError("Player prefab doesn't have a Player component! Cannot control the character.");
        }
    }
}
