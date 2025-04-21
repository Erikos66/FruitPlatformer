using System.Collections;
using UnityEngine;

public class HeadJumpable : MonoBehaviour {
    private Animator anim;
    private Collider2D col;
    private Rigidbody2D rb;
    [SerializeField] private GameObject damageTrigger;


    private void Awake() {
        anim = GetComponentInParent<Animator>();
        col = GetComponent<Collider2D>();
        rb = GetComponentInParent<Rigidbody2D>();

        if (!anim || !col || !rb) {
            Debug.LogWarning("Missing component on HeadJumpable object");
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.TryGetComponent<Player>(out var player)) {
            // Check if player is in the air AND moving downward (falling)
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (player.isAirborne == true && playerRb != null && playerRb.linearVelocity.y < 0) {
                StartCoroutine(OnHit());
                anim.SetTrigger("onHit");
                rb.linearVelocity = new Vector2(0, 0);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10);
            }
        }
    }

    private IEnumerator OnHit() {
        damageTrigger.layer = LayerMask.NameToLayer("Disabled");

        yield return new WaitForSeconds(0.5f);

        damageTrigger.layer = LayerMask.NameToLayer("EnemyHitbox");
    }
}
