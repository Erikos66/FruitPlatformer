using UnityEditor.Animations;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour {

    private Player player;
    public AnimatorOverrideController[] playerSkins;
    public Animator anim;

    private void Awake() {
        player = GetComponentInParent<Player>();
        if (player == null) {
            Debug.LogError("Player component not found on PlayerAnimation script.");
        }

        anim = GetComponent<Animator>();
        if (anim == null) {
            Debug.LogError("Animator component not found on PlayerAnimation script.");
        }

    }

    public void RespawnFinished() {
        player.EnableControl();
    }

    void Start() {
        // Get the selected skin index from the PlayerManager
        if (GameManager.instance != null && GameManager.instance.saveManager != null) {
            int skinIndex = GameManager.instance.saveManager.GetSelectedSkinIndex();

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
}
