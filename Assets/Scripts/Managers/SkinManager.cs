using UnityEngine;

public class SkinManager : MonoBehaviour {
    // Singleton instance
    public static SkinManager Instance { get; private set; }

    [Header("Player Skin Settings")]
    [SerializeField] private GameObject[] availableSkins;
    private int selectedSkinIndex = 0;

    private void Awake() {
        // Singleton setup
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this) {
            Destroy(gameObject);
        }
        // Initialize default skin settings
        LoadSelectedSkin();
    }

    /// <summary>
    /// Load the saved selected skin index
    /// </summary>
    private void LoadSelectedSkin() {
        if (SaveManager.Instance != null) {
            selectedSkinIndex = SaveManager.Instance.GetSelectedSkinIndex();
        }
        else {
            Debug.LogWarning("SaveManager not available. Using default skin index.");
            selectedSkinIndex = 0; // Use a default value if save manager isn't available
        }
    }

    /// <summary>
    /// Apply the currently selected skin to the player
    /// </summary>
    public void ApplySelectedSkin(GameObject player) {
        if (player == null || availableSkins == null || availableSkins.Length == 0)
            return;

        // Find the player renderer and apply skin
        SpriteRenderer playerRenderer = player.GetComponentInChildren<SpriteRenderer>();
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
            Debug.LogError("No skins available in the SkinManager. Cannot set skin.");
            return;
        }

        if (skinIndex >= 0 && skinIndex < availableSkins.Length) {
            selectedSkinIndex = skinIndex;
            // Add null check for GameManager.Instance and saveManager
            if (SaveManager.Instance != null) {
                SaveManager.Instance.SaveSelectedSkinIndex(selectedSkinIndex);
            }
            else {
                Debug.LogWarning("SaveManager not available. Skin selection will not be saved.");
            }

            // If player exists in the scene, apply the skin immediately
            if (GameManager.Instance != null && PlayerManager.Instance != null) {
                ApplySelectedSkin(PlayerManager.Instance.GetCurrentPlayer());
            }
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
}