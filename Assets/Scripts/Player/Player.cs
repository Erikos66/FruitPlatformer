using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour {
	private Rigidbody2D rb;
	private Animator anim;
	private PlayerAnimation playerAnimationController;

	[Header("Visuals")]
	public GameObject playerDeath_VFX;
	private ParticleSystem dustfx;

	[Header("Movement")]
	[SerializeField] private float moveSpeed;
	[SerializeField] private float jumpForce;
	[SerializeField] private float doubleJumpForce;
	public PlayerInput ActionMapping { get; private set; }
	private bool canDoubleJump;
	private bool canBeControlled = false;

	[Header("Buffer & Coyote jump")]
	[SerializeField] private float bufferJumpWindow = .25f;
	private float bufferJumpActivated = -1;
	[SerializeField] private float coyoteJumpWindow = .7f;
	private float coyoteJumpActivated = -1;

	[Header("Wall interactions")]
	[SerializeField]
	private float wallJumpDuration = .6f;
	[SerializeField] private Vector2 wallJumpForce;
	private bool isWallJumping;
	private bool canDetectWall = true;
	private Vector2 wallJumpInitialInput; // Store input when wall jump starts
	private bool ignoreHorizontalInput = false;

	[Header("Knockback")]
	[SerializeField] private Vector2 knockbackPower;
	[SerializeField] private float invincibilityDuration = 1.5f; // Duration of invincibility after knockback
	private bool isKnocked;
	private bool isInvincible;

	[Header("Collision")]
	[SerializeField] private float groundCheckDistnace;
	[SerializeField] private float wallCheckDistance;
	[SerializeField] private LayerMask whatIsGround;
	[Space]
	[SerializeField] private LayerMask whatIsEnemy;
	[SerializeField] private float EnemyCheckRadius;
	[SerializeField] private Transform EnemyCheck;
	private bool isGrounded;
	public bool isAirborne;
	private bool isWallDetected;
	private bool isTouchingLeftWall;
	private bool isTouchingRightWall;

	private Vector2 moveInput;

	private bool facingRight = true;
	private int facingDir = 1;

	private void Awake() {
		rb = GetComponent<Rigidbody2D>();
		if (rb == null) {
			Debug.LogError("Rigidbody2D component not found on Player script.");
		}
		anim = GetComponentInChildren<Animator>();
		if (anim == null) {
			Debug.LogError("Animator component not found on Player script.");
		}
		playerAnimationController = GetComponentInChildren<PlayerAnimation>();
		if (rb == null) {
			Debug.LogError("Rigidbody2D component not found on Player script.");
		}
		dustfx = GetComponentInChildren<ParticleSystem>();
		if (dustfx == null) {
			Debug.LogError("Dust particle system not found on Player script.");
		}
		ActionMapping = new PlayerInput();
		if (ActionMapping == null) {
			Debug.LogError("PlayerInput component not found on Player script.");
		}
	}

	private void OnEnable() {
		ActionMapping.Enable();

		ActionMapping.Player.Jump.performed += ctx => JumpButton();
		ActionMapping.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
		ActionMapping.Player.Movement.canceled += ctx => moveInput = Vector2.zero;

	}

	private void OnDisable() {
		ActionMapping.Disable();

		ActionMapping.Player.Jump.performed -= ctx => JumpButton();
		ActionMapping.Player.Movement.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
		ActionMapping.Player.Movement.canceled -= ctx => moveInput = Vector2.zero;

	}

	private void Update() {
		UpdateAirbornStatus();
		if (!canBeControlled) {
			HandleWallSlide();
			HandleCollision();
			HandleAnimations();
			return;
		}
		if (isKnocked)
			return;

		HandleEnemyDetection();
		// HandleInput();
		HandleWallSlide();
		HandleMovement();
		HandleFlip();
		HandleCollision();
		HandleAnimations();
	}

	private void HandleEnemyDetection() {
		// Only allow enemy damage if the player is:
		// 1. In the air (not grounded)
		// 2. Moving downward (falling)
		if (!isAirborne || rb.linearVelocity.y >= 0) {
			return;
		}

		Collider2D[] colliders = Physics2D.OverlapCircleAll(EnemyCheck.position, EnemyCheckRadius, whatIsEnemy);

		foreach (var hits in colliders) {
			if (hits) {
				IDamageable damageable = hits.GetComponent<IDamageable>();
				if (damageable != null) {
					damageable.Die();
					// Play enemy kicked sound
					AudioManager.Instance.PlaySFX("SFX_EnemyKicked");
				}
				Jump();
			}
		}
	}

	public void EnableControl() => canBeControlled = true;

	public void PushPlayer(Vector2 pushPower, float duration = 0) {
		rb.linearVelocity = Vector2.zero;
		rb.AddForce(pushPower, ForceMode2D.Impulse);
		StartCoroutine(PushControl(duration));
	}

	private IEnumerator PushControl(float duration) {
		canBeControlled = false;
		yield return new WaitForSeconds(duration);
		EnableControl();
	}

	public void Knockback(float knockbackDuration, Vector2 knockbackPower, Vector2? hitPosition = null) {
		if (isKnocked || isInvincible)
			return;

		// In Normal mode, knockback results in player death
		if (GameManager.Instance != null && GameManager.Instance.IsNormalMode()) {
			Die();
			return;
		}

		StartCoroutine(KnockbackRoutine(knockbackDuration));
		StartCoroutine(InvincibilityRoutine());
		anim.SetTrigger("knockback");

		// Use GameManager's CameraManager instead of singleton
		if (CameraManager.Instance != null)
			CameraManager.Instance.ShakeCamera();

		// Play knocked sound
		AudioManager.Instance.PlaySFX("SFX_PlayerKnocked");
		Vector2 direction = hitPosition.HasValue ? (((Vector2)transform.position) - hitPosition.Value).normalized : new Vector2(-facingDir, 0);
		rb.linearVelocity = new Vector2(direction.x * knockbackPower.x, knockbackPower.y);
	}

	private IEnumerator KnockbackRoutine(float knockbackDuration) {
		isKnocked = true;
		yield return new WaitForSeconds(knockbackDuration);
		isKnocked = false;
	}

	private IEnumerator InvincibilityRoutine() {
		isInvincible = true;

		// Get the sprite renderer to apply visual feedback
		SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();

		// Flash the player sprite to indicate invincibility
		if (spriteRenderer != null) {
			float flashInterval = 0.10f;
			for (float i = 0; i < invincibilityDuration; i += flashInterval) {
				spriteRenderer.enabled = !spriteRenderer.enabled;
				yield return new WaitForSeconds(flashInterval);
			}
			spriteRenderer.enabled = true; // Ensure sprite is visible when done
		}
		else {
			// If no sprite renderer, just wait the full duration
			yield return new WaitForSeconds(invincibilityDuration);
		}

		isInvincible = false;
	}

	private void UpdateAirbornStatus() {
		if (isGrounded && isAirborne)
			HandleLanding();
		if (!isGrounded && !isAirborne)
			BecomeAirborne();
	}

	private void BecomeAirborne() {
		isAirborne = true;
		if (rb.linearVelocity.y < 0)
			ActivateCoyoteJump();
	}

	private void HandleLanding() {
		// play dust fx
		if (dustfx != null) {
			dustfx.Play();
		}
		isAirborne = false;
		canDoubleJump = true;
		AttemptBufferJump();
	}

	// private void HandleInput() {
	//     xInput = Input.GetAxisRaw("Horizontal");
	//     yInput = Input.GetAxisRaw("Vertical");
	//     if (Input.GetKeyDown(KeyCode.Space)) {
	//         JumpButton();
	//         RequestBufferJump();
	//     }
	// }

	private void RequestBufferJump() {
		if (isAirborne)
			bufferJumpActivated = Time.time;
	}

	private void AttemptBufferJump() {
		if (Time.time < bufferJumpActivated + bufferJumpWindow) {
			bufferJumpActivated = Time.time - 1;
			Jump();
		}
	}

	private void ActivateCoyoteJump() => coyoteJumpActivated = Time.time;

	private void CancelCoyoteJump() => coyoteJumpActivated = Time.time - 1;

	private void JumpButton() {
		bool coyoteJumpAvalible = Time.time < coyoteJumpActivated + coyoteJumpWindow;
		if (isGrounded || coyoteJumpAvalible) {
			Jump();
		}
		else if ((isTouchingLeftWall || isTouchingRightWall) && !isGrounded) {
			WallJump();
		}
		else if (canDoubleJump) {
			DoubleJump();
		}
		CancelCoyoteJump();
	}

	private void Jump() {
		rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
		// Play jump sound
		AudioManager.Instance.PlaySFX("SFX_Jump");
		// play dust fx
		if (dustfx != null) {
			dustfx.Play();
		}
	}

	private void DoubleJump() {
		StopCoroutine(WallJumpRoutine());
		isWallJumping = false;
		canDoubleJump = false;
		rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
		// Play jump sound for double jump too
		AudioManager.Instance.PlaySFX("SFX_Jump");
		// play dust fx
		if (dustfx != null) {
			dustfx.Play();
		}
	}

	private void WallJump() {
		canDoubleJump = true;

		// Determine which wall we're actually touching, not just facing
		int wallDirection = 1;
		if (isTouchingLeftWall) {
			wallDirection = -1;
		}
		else if (isTouchingRightWall) {
			wallDirection = 1;
		}

		// Apply force in the opposite direction of the wall
		rb.linearVelocity = new Vector2(wallJumpForce.x * -wallDirection, wallJumpForce.y);

		// Play jump sound for wall jump
		AudioManager.Instance.PlaySFX("SFX_Jump");

		// Make sure we're facing away from the wall after jumping
		if ((wallDirection > 0 && facingRight) || (wallDirection < 0 && !facingRight)) {
			Flip();
		}

		// Store initial input state when wall jumping starts
		wallJumpInitialInput = moveInput;
		ignoreHorizontalInput = true;

		// Start wall jump related coroutines
		StartCoroutine(WallJumpRoutine());
	}

	private IEnumerator WallJumpRoutine() {
		isWallJumping = true;
		yield return new WaitForSeconds(wallJumpDuration);
		isWallJumping = false;
		ignoreHorizontalInput = false;
	}

	private void HandleWallSlide() {
		bool canWallSlide = (isTouchingLeftWall || isTouchingRightWall) && !isGrounded && rb.linearVelocity.y < 0;
		float yModifer = moveInput.y < 0 ? 1 : .05f;
		if (!canWallSlide) return;
		rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * yModifer);
	}

	private void HandleCollision() {
		isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistnace, whatIsGround);

		// Check for walls on both sides if wall detection is enabled
		if (canDetectWall) {
			isTouchingRightWall = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, whatIsGround);
			isTouchingLeftWall = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, whatIsGround);
			isWallDetected = (isTouchingRightWall && facingRight) || (isTouchingLeftWall && !facingRight);
		}
		else {
			isTouchingRightWall = false;
			isTouchingLeftWall = false;
			isWallDetected = false;
		}
	}

	private void HandleAnimations() {
		anim.SetFloat("xVelocity", rb.linearVelocity.x);
		anim.SetFloat("yVelocity", rb.linearVelocity.y);
		anim.SetBool("isGrounded", isGrounded);
		anim.SetBool("isWallDetected", isWallDetected);
	}

	private void HandleMovement() {
		if (isWallDetected)
			return;
		if (isWallJumping) {
			// Check if the input has changed significantly from initial wall jump input
			if (ignoreHorizontalInput) {
				// If input direction changes, stop ignoring input
				if (Mathf.Sign(moveInput.x) != Mathf.Sign(wallJumpInitialInput.x) && Mathf.Abs(moveInput.x) > 0.1f) {
					ignoreHorizontalInput = false;
				}
				// If input is released (near zero), stop ignoring input
				else if (Mathf.Abs(moveInput.x) < 0.1f) {
					ignoreHorizontalInput = false;
				}
			}
			return;
		}

		// Reset ignoreHorizontalInput when not wall jumping
		if (!isWallJumping && ignoreHorizontalInput) {
			ignoreHorizontalInput = false;
		}

		// Normal movement when not ignoring input
		if (!ignoreHorizontalInput) {
			rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
		}
	}

	private void HandleFlip() {
		// Don't flip based on player input if we're ignoring horizontal input
		if (ignoreHorizontalInput)
			return;

		if ((moveInput.x < 0 && facingRight) || (moveInput.x > 0 && !facingRight))
			Flip();
	}

	private void Flip() {
		facingDir *= -1;
		transform.Rotate(0, 180, 0);
		facingRight = !facingRight;
	}

	private void OnDrawGizmos() {
		Gizmos.DrawWireSphere(EnemyCheck.position, EnemyCheckRadius);
		Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistnace));

		// Draw lines for wall detection on both sides
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + wallCheckDistance, transform.position.y));
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, new Vector2(transform.position.x - wallCheckDistance, transform.position.y));
	}

	public void Die() {
		// Play death sound
		AudioManager.Instance.PlaySFX("SFX_Death");
		GameObject newPlayerDeathVFX = Instantiate(playerDeath_VFX, transform.position, Quaternion.identity);
		Destroy(gameObject);
		PlayerManager.Instance.PlayerDied();
	}
}
