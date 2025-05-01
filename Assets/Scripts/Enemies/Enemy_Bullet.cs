using UnityEngine;

public class Enemy_Bullet : MonoBehaviour {
	private Vector2 targetPosition;
	private float speed;
	private Rigidbody2D rb;
	private Vector2 movementDirection;
	[SerializeField] private float destroyTime = 5f; // Destroy after this time if no collision
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private LayerMask playerLayer;
	[SerializeField] private Vector2 knockbackForce = new Vector2(5f, 5f);
	[SerializeField] private float knockbackDuration = 0.5f;

	private void Awake() {
		rb = GetComponent<Rigidbody2D>();
		if (rb == null) {
			Debug.LogError("Rigidbody2D component not found on Enemy_Bullet!");
			Destroy(gameObject);
		}

		// Destroy the bullet after a set time to prevent it from existing indefinitely
		Destroy(gameObject, destroyTime);
	}

	public void Initialize(Vector2 targetPos, float bulletSpeed) {
		this.targetPosition = targetPos;
		this.speed = bulletSpeed;

		// Calculate direction to target
		movementDirection = (targetPos - (Vector2)transform.position).normalized;

		// Apply velocity in that direction
		rb.linearVelocity = movementDirection * speed;

		// Rotate bullet to face the direction it's moving
		float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0, 0, angle);
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		// Check if bullet hit ground
		if (((1 << collision.gameObject.layer) & groundLayer) != 0) {
			Destroy(gameObject);
			return;
		}

		// Check if bullet hit player
		if (((1 << collision.gameObject.layer) & playerLayer) != 0) {
			// Try to get Player component
			Player player = collision.gameObject.GetComponent<Player>();
			if (player != null) {
				// Use the opposite of the bullet's movement direction for knockback
				Vector2 knockbackDirection = -movementDirection;
				Vector2 knockbackPower = new Vector2(
					knockbackDirection.x * knockbackForce.x,
					knockbackForce.y  // Keep the vertical component consistent
				);

				// Apply knockback to player
				player.Knockback(knockbackDuration, knockbackPower);
			}

			// Destroy the bullet on player hit
			Destroy(gameObject);
		}
	}
}
