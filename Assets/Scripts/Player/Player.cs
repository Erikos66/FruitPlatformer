
using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour {
	#region Variables
	private Rigidbody2D _rb; // Rigidbody2D component
	private Animator _anim; // Animator component
	private PlayerAnimation _playerAnimationController; // Animation controller

	[Header("Visuals")]
	public GameObject playerDeath_VFX; // Prefab for player death VFX
	private ParticleSystem _dustfx; // Dust particle system

	[Header("Movement")]
	[SerializeField] private float _moveSpeed; // Player move speed
	[SerializeField] private float _jumpForce; // Jump force
	[SerializeField] private float _doubleJumpForce; // Double jump force
	public PlayerInput ActionMapping { get; private set; } // Input mapping
	private bool _canDoubleJump; // Can double jump
	private bool _canBeControlled = false; // Can player be controlled

	[Header("Buffer & Coyote jump")]
	[SerializeField] private float _bufferJumpWindow = .25f; // Buffer jump window
	private float _bufferJumpActivated = -1; // Buffer jump activation time
	[SerializeField] private float _coyoteJumpWindow = .7f; // Coyote jump window
	private float _coyoteJumpActivated = -1; // Coyote jump activation time

	[Header("Wall interactions")]
	[SerializeField] private float _wallJumpDuration = .6f; // Wall jump duration
	[SerializeField] private Vector2 _wallJumpForce; // Wall jump force
	private bool _isWallJumping; // Is wall jumping
	private bool _canDetectWall = true; // Can detect wall
	private Vector2 _wallJumpInitialInput; // Store input when wall jump starts
	private bool _ignoreHorizontalInput = false; // Ignore horizontal input

	[Header("Knockback")]
	[SerializeField] private Vector2 _knockbackPower; // Knockback power
	[SerializeField] private float _invincibilityDuration = 1.5f; // Invincibility duration
	private bool _isKnocked; // Is knocked
	private bool _isInvincible; // Is invincible

	[Header("Collision")]
	[SerializeField] private float _groundCheckDistnace; // Ground check distance
	[SerializeField] private float _wallCheckDistance; // Wall check distance
	[SerializeField] private LayerMask _whatIsGround; // Ground layer
	[Space]
	[SerializeField] private LayerMask _whatIsEnemy; // Enemy layer
	[SerializeField] private float _enemyCheckRadius; // Enemy check radius
	[SerializeField] private Transform _enemyCheck; // Enemy check transform
	private bool _isGrounded; // Is grounded
	public bool isAirborne; // Is airborne (public for animation)
	private bool _isWallDetected; // Is wall detected
	private bool _isTouchingLeftWall; // Is touching left wall
	private bool _isTouchingRightWall; // Is touching right wall

	private Vector2 _moveInput; // Movement input

	private bool _facingRight = true; // Facing right
	private int _facingDir = 1; // Facing direction
	#endregion

	#region Unity Methods
	private void Awake() {
		_rb = GetComponent<Rigidbody2D>();
		if (_rb == null)
			Debug.LogError("Rigidbody2D component not found on Player script.");
		_anim = GetComponentInChildren<Animator>();
		if (_anim == null)
			Debug.LogError("Animator component not found on Player script.");
		_playerAnimationController = GetComponentInChildren<PlayerAnimation>();
		if (_playerAnimationController == null)
			Debug.LogError("PlayerAnimation component not found on Player script.");
		_dustfx = GetComponentInChildren<ParticleSystem>();
		if (_dustfx == null)
			Debug.LogError("Dust particle system not found on Player script.");
		ActionMapping = new PlayerInput();
		if (ActionMapping == null)
			Debug.LogError("PlayerInput component not found on Player script.");
	}

	private void OnEnable() {
		ActionMapping.Enable();
		ActionMapping.Player.Jump.performed += ctx => JumpButton();
		ActionMapping.Player.Movement.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
		ActionMapping.Player.Movement.canceled += ctx => _moveInput = Vector2.zero;
	}

	private void OnDisable() {
		ActionMapping.Disable();
		ActionMapping.Player.Jump.performed -= ctx => JumpButton();
		ActionMapping.Player.Movement.performed -= ctx => _moveInput = ctx.ReadValue<Vector2>();
		ActionMapping.Player.Movement.canceled -= ctx => _moveInput = Vector2.zero;
	}

	private void Update() {
		UpdateAirbornStatus();
		if (!_canBeControlled) {
			HandleWallSlide();
			HandleCollision();
			HandleAnimations();
			return;
		}
		if (_isKnocked)
			return;

		HandleEnemyDetection();
		// HandleInput();
		HandleWallSlide();
		HandleMovement();
		HandleFlip();
		HandleCollision();
		HandleAnimations();
	}
	#endregion

	#region Private Methods
	private void HandleEnemyDetection() {
		// Only allow enemy damage if the player is:
		// 1. In the air (not grounded)
		// 2. Moving downward (falling)
		if (!isAirborne || _rb.linearVelocity.y >= 0)
			return;

		Collider2D[] colliders = Physics2D.OverlapCircleAll(_enemyCheck.position, _enemyCheckRadius, _whatIsEnemy);
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

	#endregion

	#region Public Methods
	/// <summary>
	/// Enables player control.
	/// </summary>
	public void EnableControl() => _canBeControlled = true;

	/// <summary>
	/// Pushes the player with a force and disables control for a duration.
	/// </summary>
	public void PushPlayer(Vector2 pushPower, float duration = 0) {
		_rb.linearVelocity = Vector2.zero;
		_rb.AddForce(pushPower, ForceMode2D.Impulse);
		StartCoroutine(PushControl(duration));
	}

	private IEnumerator PushControl(float duration) {
		_canBeControlled = false;
		yield return new WaitForSeconds(duration);
		EnableControl();
	}

	/// <summary>
	/// Applies knockback to the player.
	/// </summary>
	public void Knockback(float knockbackDuration, Vector2 knockbackPower, Vector2? hitPosition = null) {
		if (_isKnocked || _isInvincible)
			return;

		// In Normal mode, knockback results in player death
		if (GameManager.Instance != null && GameManager.Instance.IsNormalMode()) {
			Die();
			return;
		}

		StartCoroutine(KnockbackRoutine(knockbackDuration));
		StartCoroutine(InvincibilityRoutine());
		_anim.SetTrigger("knockback");

		// Use GameManager's CameraManager instead of singleton
		if (CameraManager.Instance != null)
			CameraManager.Instance.ShakeCamera();

		// Play knocked sound
		AudioManager.Instance.PlaySFX("SFX_PlayerKnocked");
		Vector2 direction = hitPosition.HasValue ? (((Vector2)transform.position) - hitPosition.Value).normalized : new Vector2(-_facingDir, 0);
		_rb.linearVelocity = new Vector2(direction.x * knockbackPower.x, knockbackPower.y);
	}

	private IEnumerator KnockbackRoutine(float knockbackDuration) {
		_isKnocked = true;
		yield return new WaitForSeconds(knockbackDuration);
		_isKnocked = false;
	}

	private IEnumerator InvincibilityRoutine() {
		_isInvincible = true;
		// Get the sprite renderer to apply visual feedback
		SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		// Flash the player sprite to indicate invincibility
		if (spriteRenderer != null) {
			float flashInterval = 0.10f;
			for (float i = 0; i < _invincibilityDuration; i += flashInterval) {
				spriteRenderer.enabled = !spriteRenderer.enabled;
				yield return new WaitForSeconds(flashInterval);
			}
			spriteRenderer.enabled = true; // Ensure sprite is visible when done
		}
		else {
			// If no sprite renderer, just wait the full duration
			yield return new WaitForSeconds(_invincibilityDuration);
		}
		_isInvincible = false;
	}

	private void UpdateAirbornStatus() {
		if (_isGrounded && isAirborne)
			HandleLanding();
		if (!_isGrounded && !isAirborne)
			BecomeAirborne();
	}

	private void BecomeAirborne() {
		isAirborne = true;
		if (_rb.linearVelocity.y < 0)
			ActivateCoyoteJump();
	}

	private void HandleLanding() {
		// play dust fx
		if (_dustfx != null)
			_dustfx.Play();
		isAirborne = false;
		_canDoubleJump = true;
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
			_bufferJumpActivated = Time.time;
	}

	private void AttemptBufferJump() {
		if (Time.time < _bufferJumpActivated + _bufferJumpWindow) {
			_bufferJumpActivated = Time.time - 1;
			Jump();
		}
	}

	private void ActivateCoyoteJump() => _coyoteJumpActivated = Time.time;

	private void CancelCoyoteJump() => _coyoteJumpActivated = Time.time - 1;

	private void JumpButton() {
		bool coyoteJumpAvalible = Time.time < _coyoteJumpActivated + _coyoteJumpWindow;
		if (_isGrounded || coyoteJumpAvalible) {
			Jump();
		}
		else if ((_isTouchingLeftWall || _isTouchingRightWall) && !_isGrounded) {
			WallJump();
		}
		else if (_canDoubleJump) {
			DoubleJump();
		}
		CancelCoyoteJump();
	}

	private void Jump() {
		_rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
		// Play jump sound
		AudioManager.Instance.PlaySFX("SFX_Jump");
		// play dust fx
		if (_dustfx != null)
			_dustfx.Play();
	}

	private void DoubleJump() {
		StopCoroutine(WallJumpRoutine());
		_isWallJumping = false;
		_canDoubleJump = false;
		_rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _doubleJumpForce);
		// Play jump sound for double jump too
		AudioManager.Instance.PlaySFX("SFX_Jump");
		// play dust fx
		if (_dustfx != null)
			_dustfx.Play();
	}

	private void WallJump() {
		_canDoubleJump = true;
		// Determine which wall we're actually touching, not just facing
		int wallDirection = 1;
		if (_isTouchingLeftWall)
			wallDirection = -1;
		else if (_isTouchingRightWall)
			wallDirection = 1;
		// Apply force in the opposite direction of the wall
		_rb.linearVelocity = new Vector2(_wallJumpForce.x * -wallDirection, _wallJumpForce.y);
		// Play jump sound for wall jump
		AudioManager.Instance.PlaySFX("SFX_Jump");
		// Make sure we're facing away from the wall after jumping
		if ((wallDirection > 0 && _facingRight) || (wallDirection < 0 && !_facingRight))
			Flip();
		// Store initial input state when wall jumping starts
		_wallJumpInitialInput = _moveInput;
		_ignoreHorizontalInput = true;
		// Start wall jump related coroutines
		StartCoroutine(WallJumpRoutine());
	}

	private IEnumerator WallJumpRoutine() {
		_isWallJumping = true;
		yield return new WaitForSeconds(_wallJumpDuration);
		_isWallJumping = false;
		_ignoreHorizontalInput = false;
	}

	private void HandleWallSlide() {
		bool canWallSlide = (_isTouchingLeftWall || _isTouchingRightWall) && !_isGrounded && _rb.linearVelocity.y < 0;
		float yModifer = _moveInput.y < 0 ? 1 : .05f;
		if (!canWallSlide) return;
		_rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * yModifer);
	}

	private void HandleCollision() {
		_isGrounded = Physics2D.Raycast(transform.position, Vector2.down, _groundCheckDistnace, _whatIsGround);
		// Check for walls on both sides if wall detection is enabled
		if (_canDetectWall) {
			_isTouchingRightWall = Physics2D.Raycast(transform.position, Vector2.right, _wallCheckDistance, _whatIsGround);
			_isTouchingLeftWall = Physics2D.Raycast(transform.position, Vector2.left, _wallCheckDistance, _whatIsGround);
			_isWallDetected = (_isTouchingRightWall && _facingRight) || (_isTouchingLeftWall && !_facingRight);
		}
		else {
			_isTouchingRightWall = false;
			_isTouchingLeftWall = false;
			_isWallDetected = false;
		}
	}

	private void HandleAnimations() {
		_anim.SetFloat("xVelocity", _rb.linearVelocity.x);
		_anim.SetFloat("yVelocity", _rb.linearVelocity.y);
		_anim.SetBool("isGrounded", _isGrounded);
		_anim.SetBool("isWallDetected", _isWallDetected);
	}

	private void HandleMovement() {
		if (_isWallDetected)
			return;
		if (_isWallJumping) {
			// Check if the input has changed significantly from initial wall jump input
			if (_ignoreHorizontalInput) {
				// If input direction changes, stop ignoring input
				if (Mathf.Sign(_moveInput.x) != Mathf.Sign(_wallJumpInitialInput.x) && Mathf.Abs(_moveInput.x) > 0.1f)
					_ignoreHorizontalInput = false;
				// If input is released (near zero), stop ignoring input
				else if (Mathf.Abs(_moveInput.x) < 0.1f)
					_ignoreHorizontalInput = false;
			}
			return;
		}
		// Reset ignoreHorizontalInput when not wall jumping
		if (!_isWallJumping && _ignoreHorizontalInput)
			_ignoreHorizontalInput = false;
		// Normal movement when not ignoring input
		if (!_ignoreHorizontalInput)
			_rb.linearVelocity = new Vector2(_moveInput.x * _moveSpeed, _rb.linearVelocity.y);
	}

	private void HandleFlip() {
		// Don't flip based on player input if we're ignoring horizontal input
		if (_ignoreHorizontalInput)
			return;
		if ((_moveInput.x < 0 && _facingRight) || (_moveInput.x > 0 && !_facingRight))
			Flip();
	}

	private void Flip() {
		_facingDir *= -1;
		transform.Rotate(0, 180, 0);
		_facingRight = !_facingRight;
	}

	private void OnDrawGizmos() {
		Gizmos.DrawWireSphere(_enemyCheck.position, _enemyCheckRadius);
		Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - _groundCheckDistnace));
		// Draw lines for wall detection on both sides
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + _wallCheckDistance, transform.position.y));
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, new Vector2(transform.position.x - _wallCheckDistance, transform.position.y));
	}

	/// <summary>
	/// Handles player death.
	/// </summary>
	public void Die() {
		// Play death sound
		AudioManager.Instance.PlaySFX("SFX_Death");
		GameObject newPlayerDeathVFX = Instantiate(playerDeath_VFX, transform.position, Quaternion.identity);
		Destroy(gameObject);
		PlayerManager.Instance.PlayerDied();
	}
	#endregion
}
