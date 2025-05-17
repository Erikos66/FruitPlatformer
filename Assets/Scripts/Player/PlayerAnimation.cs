using UnityEngine;

public class PlayerAnimation : MonoBehaviour {
    #region Variables
    private Player _player; // Reference to player
    public AnimatorOverrideController[] playerSkins; // Array of player skins
    public Animator anim; // Animator reference
    #endregion

    #region Unity Methods
    private void Awake() {
        _player = GetComponentInParent<Player>();
        if (_player == null)
            Debug.LogError("Player component not found on PlayerAnimation script.");

        anim = GetComponent<Animator>();
        if (anim == null)
            Debug.LogError("Animator component not found on PlayerAnimation script.");
    }

    private void Start() {
        // Get the selected skin index from the PlayerManager
        if (SaveManager.Instance != null) {
            int skinIndex = SaveManager.Instance.GetSelectedSkinIndex();
            // Apply the skin if it's within range
            if (skinIndex < playerSkins.Length) {
                anim.runtimeAnimatorController = playerSkins[skinIndex];
            }
            else {
                Debug.LogWarning("Selected skin index is out of range! Using default skin.");
                anim.runtimeAnimatorController = playerSkins[0];
            }
        }
        else {
            // Fallback to default skin if GameManager isn't initialized yet
            anim.runtimeAnimatorController = playerSkins[0];
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Called when respawn animation is finished.
    /// </summary>
    public void RespawnFinished() {
        _player.EnableControl();
    }
    #endregion
}
