using UnityEngine;

public class Trap_Trampoline : MonoBehaviour {
    private Animator anim;
    [SerializeField] private float pushForce = 50; // The force applied to the player when they land on the trampoline
    [SerializeField] private float disableDelay = 0; // The force applied to the player when they land on the trampoline

    private void Awake() {
        anim = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D collision) {
        Player player = collision.GetComponent<Player>();
        if (player != null) {
            anim.SetTrigger("spring");
            player.PushPlayer(transform.up * pushForce, disableDelay);
        }
    }
}
