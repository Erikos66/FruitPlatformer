using UnityEngine;

public class Enemy_Rhino : Base_Enemy_Class {

	#region Variables

	[Header("Rhino Specific")]
	[SerializeField] private float _chargeSpeed = 16f;          // Speed when charging
	[SerializeField] private float _returnSpeed = 3f;           // Speed when returning to start
	[SerializeField] private float _bounceForce = 2f;           // Vertical force when bouncing
	[SerializeField] private float _returnDelay = 2f;           // Delay before returning to start

	private Vector3 _startingPosition;                          // Initial position
	private bool _isCharging;                                   // Whether rhino is charging
	private bool _isReturning;                                  // Whether returning to start
	private bool _isBouncing;                                   // Whether bouncing off wall
	private float _normalSpeed;                                 // Normal movement speed
	private float _returnTimer;                                 // Timer for return delay
	private int _startingFacingDir;                             // Initial facing direction

	#endregion

	#region Unity Methods

	protected override void Awake() {
		base.Awake();
		_normalSpeed = _moveSpeed;
		_startingPosition = transform.position;
		_startingFacingDir = _facingDir;
	}

	protected override void Update() {
		base.Update();

		if (_isDead) {
			return;
		}

		_anim.SetFloat("xVelocity", Mathf.Abs(_rb.linearVelocity.x));

		HandleCollision();

		if (_isGrounded) {
			if (_isBouncing) {
				_isBouncing = false;
				_isCharging = false;
				_returnTimer = _returnDelay;
			}
			else if (_returnTimer > 0) {
				_returnTimer -= Time.deltaTime;
				if (_returnTimer <= 0) {
					_isReturning = true;
				}
			}
			else if (_isCharging) {
				ChargeBehavior();
				if (_isWallDetected) {
					StartBounce();
				}
			}
			else if (_isReturning) {
				ReturnBehavior();
				if (DetectedPlayer()) {
					_isCharging = true;
					_isReturning = false;
					_moveSpeed = _chargeSpeed;
				}
			}
			else {
				if (DetectedPlayer()) {
					_isCharging = true;
					_isReturning = false;
					_moveSpeed = _chargeSpeed;
				}
			}
		}
	}

	protected override void OnDrawGizmos() {
		base.OnDrawGizmos();

		if (Application.isPlaying) {
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(_startingPosition, 0.2f);
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Handles the charging behavior
	/// </summary>
	private void ChargeBehavior() {
		// Play the charge sound
		AudioManager.Instance.PlaySFX("SFX_RinoCharge");
		_rb.linearVelocity = new Vector2(_moveSpeed * _facingDir, _rb.linearVelocity.y);
	}

	/// <summary>
	/// Initiates bounce behavior when hitting a wall
	/// </summary>
	private void StartBounce() {
		_isBouncing = true;
		_anim.SetTrigger("onWallHit");
		_rb.linearVelocity = new Vector2(-_facingDir * _moveSpeed * 0.5f, _bounceForce);
	}

	/// <summary>
	/// Handles returning to starting position
	/// </summary>
	private void ReturnBehavior() {
		int directionToStart = transform.position.x > _startingPosition.x ? -1 : 1;

		if (directionToStart != _facingDir) {
			Flip();
			return;
		}

		_moveSpeed = _returnSpeed;
		_rb.linearVelocity = new Vector2(_moveSpeed * _facingDir, _rb.linearVelocity.y);

		float distanceToStart = Vector2.Distance(
			new Vector2(transform.position.x, 0),
			new Vector2(_startingPosition.x, 0)
		);

		if (distanceToStart < 0.5f) {
			transform.position = new Vector3(_startingPosition.x, transform.position.y, transform.position.z);
			_rb.linearVelocity = Vector2.zero;

			if (_facingDir != _startingFacingDir) {
				Flip();
			}

			_isReturning = false;
			_moveSpeed = _normalSpeed;
		}
	}

	#endregion
}
