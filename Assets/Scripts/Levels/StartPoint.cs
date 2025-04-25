using System.Collections;
using UnityEngine;

public class StartPoint : MonoBehaviour {
    private Animator anim;
    [SerializeField] public Transform respawnPoint;

    private void Awake() {
        anim = GetComponentInChildren<Animator>();
    }

    public void AnimateFlag() {
        anim.SetTrigger("waveflag");
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.TryGetComponent<Player>(out var player)) {
            AnimateFlag();
            // Timer is now started when player spawns, not here
        }
    }
}
