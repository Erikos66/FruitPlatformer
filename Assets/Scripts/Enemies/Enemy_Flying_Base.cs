// Base Class Setup When Needed, did some start work. Likely will need to be changed later.

using UnityEngine;

public class Enemy_Flying_Base : MonoBehaviour {

	[Header("Enemy Components")]
	[SerializeField] protected Rigidbody2D rb; // Rigidbody component for physics interactions
	[SerializeField] protected Animator anim; // Animator component for animations
	[SerializeField] protected SpriteRenderer spriteRenderer; // SpriteRenderer component for sprite rendering
	[SerializeField] protected Collider2D col; // Collider component for collision detection

	[Header("Death Properties")]
	[SerializeField] protected float despawnTime = 5f; // Time before the enemy is destroyed after death
	[SerializeField] protected float deathRotationSpeed = 100f; // Speed of rotation when the enemy dies
	[SerializeField] protected float deathImpactForce; // Force applied to the enemy when it dies

	[Header("Player Detection Settings")]
	[SerializeField] protected LayerMask playerLayer; // Layer for player detection
	[SerializeField] protected float detectionRadius = 5f; // Radius for player detection



	protected bool isDead = false;

	protected virtual void Awake() {
		if (rb == null) rb = GetComponent<Rigidbody2D>();
		if (anim == null) anim = GetComponent<Animator>();
		if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
		if (col == null) col = GetComponent<Collider2D>();
	}


	public virtual void Die() {
		if (isDead) return;
		isDead = true;
		rb.freezeRotation = false;
		anim.SetTrigger("onHit");

		rb.linearVelocity = new Vector2(rb.linearVelocity.x, deathImpactForce);
		Collider2D[] colliders = GetComponents<Collider2D>();
		foreach (Collider2D collider in colliders) {
			collider.enabled = false;
		}

		gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

		float rotationDirection = Random.value < 0.5f ? -1f : 1f;
		rb.angularVelocity = rotationDirection * deathRotationSpeed;
		rb.gravityScale = 1f;

		Destroy(gameObject, despawnTime);
	}

}
