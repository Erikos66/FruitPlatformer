using UnityEngine;
using System.Collections;

public class Enemy_Snail : Base_Enemy_Class {

	#region Variables

	[Header("Snail Specific")]
	[SerializeField] private float _shellSlideSpeed = 8f;      // Speed when sliding in shell form
	[SerializeField] private float _bounceForce = 5f;          // Force applied when bouncing off objects
	[SerializeField] private float _wallBounceDelay = 0.1f;    // Delay before bouncing off wall
	[SerializeField] private LayerMask _enemyLayer;            // Layer for detecting other enemies

	private enum SnailState { WithShell, NoShell }
	private SnailState _currentState = SnailState.WithShell;   // Current state of the snail
	private bool _isSliding = false;                          // Whether shell is sliding
	private bool _isBouncingOffWall = false;                  // Whether currently bouncing off wall
	private int _bounceCount = 0;                             // Number of bounces performed

	#endregion

	#region Unity Methods

	protected override void Awake() {
		base.Awake();
	}

	protected override void Update() {
		base.Update();

		if (_isDead) return;

		_anim.SetFloat("xVelocity", Mathf.Abs(_rb.linearVelocity.x));

		HandleCollision();

		switch (_currentState) {
			case SnailState.WithShell:
				if (_isGrounded) {
					HandleMovement();

					if (_isWallDetected || !_isGroundinFrontDetected) {
						Flip();
						_idleTimer = _idleDuration;
					}
				}
				break;

			case SnailState.NoShell:
				if (_isGrounded && _isSliding && !_isBouncingOffWall) {
					SlideShell();

					if (_isWallDetected) {
						StartCoroutine(DelayedBounceOffWall());
					}
				}
				break;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		// Only perform collision effects when in NoShell state and sliding
		if (_currentState == SnailState.NoShell && _isSliding) {
			// Check if shell hit player
			if (((1 << collision.gameObject.layer) & _playerLayer) != 0) {
				Player player = collision.gameObject.GetComponent<Player>();
				if (player != null) {
					// Apply knockback to player
					Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
					Vector2 knockbackPower = new Vector2(
						knockbackDirection.x * _bounceForce,
						_bounceForce  // Keep the vertical component consistent
					);

					player.Knockback(0.5f, knockbackPower);
				}
			}

			// Check if shell hit another enemy
			if (((1 << collision.gameObject.layer) & _enemyLayer) != 0) {
				// Try to get Enemy_Base component from the collided object or its parent
				Base_Enemy_Class enemy = collision.gameObject.GetComponent<Base_Enemy_Class>();
				if (enemy == null) {
					enemy = collision.gameObject.GetComponentInParent<Base_Enemy_Class>();
				}

				// Make sure we don't kill ourselves
				if (enemy != null && enemy != this) {
					enemy.Die();
				}
			}
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Controls the shell sliding behavior
	/// </summary>
	private void SlideShell() {
		_rb.linearVelocity = new Vector2(_shellSlideSpeed * _facingDir, _rb.linearVelocity.y);
	}

	/// <summary>
	/// Delays the bounce off wall to create better visual effect
	/// </summary>
	/// <returns>Coroutine IEnumerator</returns>
	private IEnumerator DelayedBounceOffWall() {
		// Set flag to prevent multiple bounces while waiting
		_isBouncingOffWall = true;

		_bounceCount++;
		if (_bounceCount > 3) {
			Die();
		}

		// Stop the shell
		_rb.linearVelocity = Vector2.zero;

		// Play wall hit animation
		_anim.SetTrigger("onWallHit");

		// Wait for the delay
		yield return new WaitForSeconds(_wallBounceDelay);

		// Bounce off wall
		Flip();

		// Re-enable sliding
		_isBouncingOffWall = false;

		// play the bounce sound
		AudioManager.Instance.PlaySFX("SFX_EnemyKicked");
	}

	/// <summary>
	/// Starts the shell sliding behavior
	/// </summary>
	private void LaunchShell() {
		_isSliding = true;

		// Find player position to determine launch direction
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		if (player != null) {
			// Face away from the player (shell gets kicked in direction player is facing)
			float dirToPlayer = player.transform.position.x > transform.position.x ? 1 : -1;

			// If player's direction is different than current facing direction, flip
			if (dirToPlayer != _facingDir) {
				Flip();
			}
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Overrides the base Die method to implement shell state transitions
	/// </summary>
	public override void Die() {
		if (_isDead) return;
		// play the die sound
		AudioManager.Instance.PlaySFX("SFX_EnemyKicked");

		switch (_currentState) {
			case SnailState.WithShell:
				// Change to NoShell state instead of dying
				_currentState = SnailState.NoShell;
				_anim.SetTrigger("onHit");
				_rb.linearVelocity = Vector2.zero;
				_isSliding = false;
				break;

			case SnailState.NoShell:
				if (!_isSliding) {
					// Start sliding if not already sliding
					LaunchShell();
				}
				else {
					// Actually die if already in NoShell state and sliding
					base.Die();
				}
				break;
		}
	}

	#endregion
}
