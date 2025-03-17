using UnityEngine;

public class DamageBehaviour : MonoBehaviour {
    [Header("Damage Settings")]
    [SerializeField] private float knockbackDuration = 1;
    [SerializeField] private Vector2 knockbackPower = new(5, 5);
    [SerializeField] private float Damage;

    void OnTriggerEnter2D(Collider2D collision) {
        Player player = collision.GetComponent<Player>();
        if (player != null) {
            player.Knockback(knockbackDuration, knockbackPower);
        }
    }
}
