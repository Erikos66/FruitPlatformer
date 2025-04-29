using System;
using UnityEngine;

public class FruitManager : MonoBehaviour {
    // Singleton instance
    public static FruitManager Instance { get; private set; }

    // Current level fruits collected/total
    private int fruitsCollected = 0;
    private int fruitsInLevel = 0;

    // Event that UI can subscribe to for updates
    public event Action<int, int> OnFruitsUpdated;

    private void Awake() {
        // Singleton setup
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
        }
        else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Reset fruit counter when entering a new level
    /// </summary>
    public void ResetFruitCounter() {
        // Get the current level name
        string currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // First check if we have a saved total count for this level
        int savedTotalFruits = SaveManager.Instance.GetTotalFruitsCount(currentLevel);

        // If we have a saved count, use it; otherwise count the fruits in the level
        if (savedTotalFruits > 0) {
            fruitsInLevel = savedTotalFruits;
        }
        else {
            // Only count fruits if we don't have a saved count
            CountFruitsInCurrentLevel();
        }

        // Check if we have saved fruit count data for this level
        int savedCollectedFruits = SaveManager.Instance.GetCollectedFruitsCount(currentLevel);

        // Use the saved count or reset to 0
        fruitsCollected = savedCollectedFruits;

        // Make sure we notify UI about the initial state
        NotifyFruitCountUpdated();
    }

    /// <summary>
    /// Count how many collectible fruits are in the current level
    /// </summary>
    public void CountFruitsInCurrentLevel() {
        string currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // First check if we already have a saved total count for this level
        int savedTotalFruits = SaveManager.Instance.GetTotalFruitsCount(currentLevel);

        // If we already have a saved count for this level, use it instead of recounting
        if (savedTotalFruits > 0) {
            fruitsInLevel = savedTotalFruits;
            return;
        }

        // Only count fruits if we don't have a saved count
        Fruit[] fruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);

        // We need to count both active fruits and those that have been collected previously
        int inactiveCount = 0;
        foreach (var fruit in fruits) {
            if (!fruit.gameObject.activeSelf) {
                inactiveCount++;
            }
        }

        // Get count of fruits that were previously collected in past plays of this level
        // but aren't included in the scene anymore (edge case)
        int previouslyCollected = SaveManager.Instance.GetCollectedFruitsCount(currentLevel);

        // Total is the sum of visible fruits plus any inactive fruits
        fruitsInLevel = fruits.Length + inactiveCount;

        // Make sure we never set total less than what's been collected
        if (fruitsInLevel < previouslyCollected) {
            fruitsInLevel = previouslyCollected;
        }

        // Save the total fruits count for this level
        SaveManager.Instance.SaveTotalFruitsCount(currentLevel, fruitsInLevel);
    }

    /// <summary>
    /// Add a collected fruit to the count
    /// </summary>
    public void CollectFruit() {
        fruitsCollected++;

        // Save the current fruit collection progress
        string currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        SaveManager.Instance.SaveCollectedFruitsCount(currentLevel, fruitsCollected);

        NotifyFruitCountUpdated();

        // Check if we've collected all fruits
        if (fruitsCollected >= fruitsInLevel) {
            AllFruitsCollected();
        }
    }

    /// <summary>
    /// Called when all fruits in a level have been collected
    /// </summary>
    private void AllFruitsCollected() {
        // Save the achievement of collecting all fruits in the level
        string currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        SaveManager.Instance.SetAllFruitsCollected(currentLevel);
    }

    /// <summary>
    /// Get the number of fruits collected in the current level
    /// </summary>
    public int GetFruitsCollected() {
        return fruitsCollected;
    }

    /// <summary>
    /// Get the total number of fruits in the current level
    /// </summary>
    public int GetFruitsInLevel() {
        return fruitsInLevel;
    }

    /// <summary>
    /// Notify listeners that fruit count has changed
    /// </summary>
    private void NotifyFruitCountUpdated() {
        OnFruitsUpdated?.Invoke(fruitsCollected, fruitsInLevel);
    }

    /// <summary>
    /// Check if all fruits have been collected in the current level
    /// </summary>
    public bool HasCollectedAllFruits() {
        return fruitsCollected >= fruitsInLevel && fruitsInLevel > 0;
    }
}