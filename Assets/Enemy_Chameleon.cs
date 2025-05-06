using UnityEngine;

public class Enemy_Chameleon : Enemy_Base {
	#region Variables
	[Header("Chameleon Properties")]
	[SerializeField] private float attackCooldown = 2f;
	[SerializeField] private float attackRaycastDistance = 3f;
	[SerializeField] private Vector2 knockbackPower = new Vector2(10f, 5f);
	[SerializeField] private float knockbackDuration = 1f;
	[SerializeField] private Transform attackOrigin;
	[SerializeField] private float transparentAlpha = 0.3f;
	[SerializeField] private float solidAlpha = 1f;
	[SerializeField] private float fadeSpeed = 5f;

	private SpriteRenderer spriteRenderer;
	private float attackTimer;
	private bool isAttacking;
	private bool playerDetected;
	#endregion

	#region Unity Methods
	protected override void Awake() {
		base.Awake();
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer == null) {
			Debug.LogError("SpriteRenderer not found on " + gameObject.name);
		}

		// Set initial transparency
		SetTransparency(transparentAlpha);

		if (attackOrigin == null) {
			attackOrigin = transform; // Use own transform if not specified
		}
	}

	protected override void Update() {
		base.Update();

		anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));

		HandleCollision();
		ManageTransparency();
		CheckForPlayer();

		if (isGrounded) {
			if (!isAttacking) {
				HandleMovement();

				if (isWallDetected || !isGroundinFrontDetected) {
					Flip();
					idleTimer = idleDuration;
				}
			}
		}

		// Handle attack cooldown
		if (attackTimer > 0) {
			attackTimer -= Time.deltaTime;
		}
	}

	#endregion

	#region Custom Methods
	private void CheckForPlayer() {
		// Use the inherited DetectedPlayer method
		playerDetected = DetectedPlayer();

		if (playerDetected && !isAttacking && attackTimer <= 0) {
			Attack();
		}
	}

	private void Attack() {
		// Start the attack sequence
		isAttacking = true;
		attackTimer = attackCooldown;

		// Play attack animation
		anim.SetTrigger("onAttack");

		// Set solid appearance when attacking
		SetTransparency(solidAlpha);

		// play attack sound
		AudioManager.Instance.PlaySFXOnce("SFX_ChameleonAttack");
	}

	// Called by Animation Event
	public void FireAttackRaycast() {
		RaycastHit2D hit = Physics2D.Raycast(
			attackOrigin.position,
			new Vector2(facingDir, 0),
			attackRaycastDistance,
			playerLayer
		);

		// Draw debug ray to visualize the attack
		Debug.DrawRay(attackOrigin.position, new Vector3(facingDir * attackRaycastDistance, 0, 0), Color.red, 1f);

		if (hit.collider != null && hit.collider.TryGetComponent<Player>(out var player)) {
			// Apply knockback to player
			player.Knockback(knockbackDuration, knockbackPower, attackOrigin.position);

			// Play hit effect or sound here if needed
		}
	}

	// Called by Animation Event
	public void EndAttack() {
		isAttacking = false;
	}

	private void ManageTransparency() {
		if (!isAttacking) {
			// Fade to transparent when not attacking
			if (spriteRenderer.color.a > transparentAlpha) {
				SetTransparency(Mathf.Lerp(spriteRenderer.color.a, transparentAlpha, fadeSpeed * Time.deltaTime));
			}
		}
		else {
			// Fade to solid when attacking
			if (spriteRenderer.color.a < solidAlpha) {
				SetTransparency(Mathf.Lerp(spriteRenderer.color.a, solidAlpha, fadeSpeed * Time.deltaTime));
			}
		}
	}

	private void SetTransparency(float alpha) {
		if (spriteRenderer != null) {
			Color color = spriteRenderer.color;
			color.a = alpha;
			spriteRenderer.color = color;
		}
	}

	protected override void OnDrawGizmos() {
		base.OnDrawGizmos();

		// Draw attack range
		if (attackOrigin != null) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(attackOrigin.position, new Vector3(facingDir, 0, 0) * attackRaycastDistance);
		}
	}
	#endregion
}
