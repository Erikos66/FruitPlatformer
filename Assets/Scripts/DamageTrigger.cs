using UnityEngine;
using System.Collections;

public class DamageBehaviour : MonoBehaviour {
    [Header("Damage Settings")]
    [SerializeField] private float knockbackDuration = 1; // The duration of the knockback effect
    [SerializeField] private Vector2 knockbackPower = new(10, 5); // The power of the knockback effect
    [SerializeField] private float Damage; // The amount of damage the trap deals
    [SerializeField] private float hitInterval = 1; // The interval at which the trap hits the player

    private bool isPlayerInside = false; // Whether the player is inside the trap
    private Player currentPlayer = null; // The player currently inside the trap
    private Coroutine hitCoroutine = null; // The coroutine that repeatedly hits the player

    void OnTriggerEnter2D(Collider2D collision) {
        Player player = collision.GetComponent<Player>();
        if (player != null) {
            currentPlayer = player;
            isPlayerInside = true;
            player.Knockback(knockbackDuration, knockbackPower, transform.position);
            if (hitCoroutine == null) {
                hitCoroutine = StartCoroutine(HitPlayerCoroutine());
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        Player player = collision.GetComponent<Player>();
        if (player != null && player == currentPlayer) {
            isPlayerInside = false;
            currentPlayer = null;
        }
    }

    IEnumerator HitPlayerCoroutine() {
        while (isPlayerInside) {
            yield return new WaitForSeconds(hitInterval);
            if (isPlayerInside && currentPlayer != null) {
                currentPlayer.Knockback(knockbackDuration, knockbackPower, transform.position);
            }
        }
        hitCoroutine = null;
    }
}
