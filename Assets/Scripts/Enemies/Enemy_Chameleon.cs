using UnityEngine;

public class Enemy_Chameleon : Base_Enemy_Class {

	#region Variables

	[Header("Chameleon Properties")]
	[SerializeField] private float _attackCooldown = 2f;              // Time between attacks
	[SerializeField] private float _attackRaycastDistance = 3f;       // Distance of attack
	[SerializeField] private Vector2 _knockbackPower = new Vector2(10f, 5f); // Force applied to player
	[SerializeField] private float _knockbackDuration = 1f;           // Duration of knockback
	[SerializeField] private Transform _attackOrigin;                 // Origin point for attack
	[SerializeField] private float _transparentAlpha = 0.3f;          // Alpha value when hidden
	[SerializeField] private float _solidAlpha = 1f;                  // Alpha value when visible
	[SerializeField] private float _fadeSpeed = 5f;                   // Speed of transparency change

	private SpriteRenderer _spriteRenderer;                           // Reference to sprite renderer
	private float _attackTimer;                                        // Timer for attack cooldown
	private bool _isAttacking;                                         // Whether attacking is in progress
	private bool _playerDetected;                                      // Whether player is detected

	#endregion

	#region Unity Methods

	protected override void Awake() {
		base.Awake();
		_spriteRenderer = GetComponent<SpriteRenderer>();
		if (_spriteRenderer == null) {
			Debug.LogError("SpriteRenderer not found on " + gameObject.name);
		}

		// Set initial transparency
		SetTransparency(_transparentAlpha);

		if (_attackOrigin == null) {
			_attackOrigin = transform; // Use own transform if not specified
		}
	}

	protected override void Update() {
		base.Update();

		_anim.SetFloat("xVelocity", Mathf.Abs(_rb.linearVelocity.x));

		HandleCollision();
		ManageTransparency();
		CheckForPlayer();

		if (_isGrounded) {
			if (!_isAttacking) {
				HandleMovement();

				if (_isWallDetected || !_isGroundinFrontDetected) {
					Flip();
					_idleTimer = _idleDuration;
				}
			}
		}

		// Handle attack cooldown
		if (_attackTimer > 0) {
			_attackTimer -= Time.deltaTime;
		}
	}

	protected override void OnDrawGizmos() {
		base.OnDrawGizmos();

		// Draw attack range
		if (_attackOrigin != null) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(_attackOrigin.position, new Vector3(_facingDir, 0, 0) * _attackRaycastDistance);
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Checks if the player is detected and initiates attack
	/// </summary>
	private void CheckForPlayer() {
		// Use the inherited DetectedPlayer method
		_playerDetected = DetectedPlayer();

		if (_playerDetected && !_isAttacking && _attackTimer <= 0) {
			Attack();
		}
	}

	/// <summary>
	/// Initiates attack sequence
	/// </summary>
	private void Attack() {
		// Start the attack sequence
		_isAttacking = true;
		_attackTimer = _attackCooldown;

		// Play attack animation
		_anim.SetTrigger("onAttack");

		// Set solid appearance when attacking
		SetTransparency(_solidAlpha);

		// Play attack sound
		AudioManager.Instance.PlaySFXOnce("SFX_ChameleonAttack");
	}

	/// <summary>
	/// Manages transparency based on attack state
	/// </summary>
	private void ManageTransparency() {
		if (!_isAttacking) {
			// Fade to transparent when not attacking
			if (_spriteRenderer.color.a > _transparentAlpha) {
				SetTransparency(Mathf.Lerp(_spriteRenderer.color.a, _transparentAlpha, _fadeSpeed * Time.deltaTime));
			}
		}
		else {
			// Fade to solid when attacking
			if (_spriteRenderer.color.a < _solidAlpha) {
				SetTransparency(Mathf.Lerp(_spriteRenderer.color.a, _solidAlpha, _fadeSpeed * Time.deltaTime));
			}
		}
	}

	/// <summary>
	/// Sets the transparency of the sprite
	/// </summary>
	/// <param name="alpha">Alpha value (0-1)</param>
	private void SetTransparency(float alpha) {
		if (_spriteRenderer != null) {
			Color color = _spriteRenderer.color;
			color.a = alpha;
			_spriteRenderer.color = color;
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Fires a raycast to detect player during attack
	/// Called by Animation Event
	/// </summary>
	public void FireAttackRaycast() {
		RaycastHit2D hit = Physics2D.Raycast(
			_attackOrigin.position,
			new Vector2(_facingDir, 0),
			_attackRaycastDistance,
			_playerLayer
		);

		// Draw debug ray to visualize the attack
		Debug.DrawRay(_attackOrigin.position, new Vector3(_facingDir * _attackRaycastDistance, 0, 0), Color.red, 1f);

		if (hit.collider != null && hit.collider.TryGetComponent<Player>(out var player)) {
			// Apply knockback to player
			player.Knockback(_knockbackDuration, _knockbackPower, _attackOrigin.position);
		}
	}

	/// <summary>
	/// Ends the attack sequence
	/// Called by Animation Event
	/// </summary>
	public void EndAttack() {
		_isAttacking = false;
	}

	#endregion
}
