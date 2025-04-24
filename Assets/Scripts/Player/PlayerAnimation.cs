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
        anim.runtimeAnimatorController = playerSkins[(int)GameManager.instance.selectedSkinIndex];
        if (GameManager.instance.selectedSkinIndex > playerSkins.Length) {
            Debug.LogWarning("Selected skin index is out of range! Using default skin.");
            anim.runtimeAnimatorController = playerSkins[0];
        }
    }
}
