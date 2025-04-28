using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
    [Header("Player Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Player Skin Settings")]
    [SerializeField] private GameObject[] availableSkins;
    private int selectedSkinIndex = 0;

    private GameObject currentPlayer;
    private bool firstSpawn = true;

    private void Start() {
        // Initialize default skin settings
        LoadSelectedSkin();
    }

    /// <summary>
    /// Set the current spawn point for the player
    /// </summary>
    public void SetSpawnPoint(Transform newSpawnPoint) {
        if (newSpawnPoint != null) {
            spawnPoint = newSpawnPoint;
            Debug.Log("Spawn point updated to: " + newSpawnPoint.position);
        }
    }

    /// <summary>
    /// Spawn or respawn the player at the current spawn point
    /// </summary>
    public void RespawnPlayer() {
        if (currentPlayer != null) {
            Destroy(currentPlayer);
        }

        if (spawnPoint == null) {
            // Try to find a spawn point in the scene
            spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint")?.transform;
            if (spawnPoint == null) {
                Debug.LogError("No spawn point found in the scene!");
                return;
            }
        }

        // Instantiate player prefab at spawn point
        currentPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);

        // Apply the selected skin
        ApplySelectedSkin();

        // Set up camera to follow player
        GameManager.instance.cameraManager.SetTargetToFollow(currentPlayer.transform);

        //Play the respawn sound
        AudioManager.Instance.PlayRandomSFX("SFX_Respawn");

        // Start the timer on first spawn only
        if (firstSpawn) {
            GameManager.instance.timerManager.StartLevelTimer();
            firstSpawn = false;
        }

        // Add a small delay before enabling player control
        StartCoroutine(EnablePlayerControlAfterDelay(0.5f));
    }

    private IEnumerator EnablePlayerControlAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        Player playerComponent = currentPlayer.GetComponent<Player>();
        if (playerComponent != null) {
            playerComponent.EnableControl();
        }
    }

    /// <summary>
    /// Handle player death and respawn
    /// </summary>
    public void PlayerDied() {
        if (currentPlayer != null) {
            // Create death VFX
            Player playerComponent = currentPlayer.GetComponent<Player>();
            if (playerComponent != null && playerComponent.playerDeath_VFX != null) {
                Instantiate(playerComponent.playerDeath_VFX, currentPlayer.transform.position, Quaternion.identity);
            }

            // Play death sound
            AudioManager.Instance.PlaySFX("SFX_Die");

            // Destroy player object
            Destroy(currentPlayer);
            currentPlayer = null;

            // Respawn player after a delay
            StartCoroutine(DelayedRespawn(1f));
        }
    }

    private IEnumerator DelayedRespawn(float delay) {
        yield return new WaitForSeconds(delay);
        RespawnPlayer();
    }

    #region Skin Management

    /// <summary>
    /// Load the saved selected skin index
    /// </summary>
    private void LoadSelectedSkin() {
        if (GameManager.instance != null && GameManager.instance.saveManager != null) {
            selectedSkinIndex = GameManager.instance.saveManager.GetSelectedSkinIndex();
        }
        else {
            Debug.LogWarning("SaveManager not available. Using default skin index.");
            selectedSkinIndex = 0; // Use a default value if save manager isn't available
        }
    }

    /// <summary>
    /// Apply the currently selected skin to the player
    /// </summary>
    public void ApplySelectedSkin() {
        if (currentPlayer == null || availableSkins == null || availableSkins.Length == 0)
            return;

        // Find the current player renderer and apply skin
        SpriteRenderer playerRenderer = currentPlayer.GetComponentInChildren<SpriteRenderer>();
        if (playerRenderer != null && selectedSkinIndex < availableSkins.Length) {
            // Apply skin based on the selected index
            // This implementation depends on how your skins are structured
        }
    }

    /// <summary>
    /// Change to a specified skin index
    /// </summary>
    public void SetSkin(int skinIndex) {
        // Check if availableSkins is initialized
        if (availableSkins == null || availableSkins.Length == 0) {
            Debug.LogError("No skins available in the PlayerManager. Cannot set skin.");
            return;
        }

        if (skinIndex >= 0 && skinIndex < availableSkins.Length) {
            selectedSkinIndex = skinIndex;
            // Add null check for GameManager.instance and saveManager
            if (GameManager.instance != null && GameManager.instance.saveManager != null) {
                GameManager.instance.saveManager.SaveSelectedSkinIndex(selectedSkinIndex);
            }
            else {
                Debug.LogWarning("SaveManager not available. Skin selection will not be saved.");
            }
            ApplySelectedSkin();
        }
    }

    /// <summary>
    /// Get the currently selected skin
    /// </summary>
    public GameObject GetSelectedSkin() {
        if (availableSkins != null && availableSkins.Length > 0 && selectedSkinIndex < availableSkins.Length)
            return availableSkins[selectedSkinIndex];
        return null;
    }

    #endregion

    /// <summary>
    /// Reset the first spawn flag (called when loading a new level)
    /// </summary>
    public void ResetFirstSpawnFlag() {
        firstSpawn = true;
    }
}