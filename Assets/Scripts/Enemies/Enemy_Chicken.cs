using UnityEngine;
using System.Collections;

public class Enemy_Chicken : Base_Enemy_Class {

	#region Variables

	[Header("Chicken Specific")]
	[SerializeField] private float _chargeSpeed = 4f;         // Speed when charging toward player
	[SerializeField] private float _slidingDeceleration = 2f; // Rate at which chicken slows down after charge

	private bool _playerDetected;                             // Whether player has been detected
	private bool _isCharging;                                 // Whether chicken is currently charging
	private float _normalSpeed;                               // Standard movement speed
	private float _currentSpeed;                              // Current movement speed
	private bool _hasPlayedChargeSound;                       // Whether charge sound has been played

	#endregion

	#region Unity Methods

	protected override void Awake() {
		base.Awake();
		_normalSpeed = _moveSpeed;
		_currentSpeed = _normalSpeed;
	}

	protected override void Update() {
		base.Update();

		if (_isDead) {
			return;
		}

		_anim.SetFloat("xVelocity", Mathf.Abs(_rb.linearVelocity.x));

		_playerDetected = DetectedPlayer();

		HandleCollision();
		if (_isGrounded) {
			if (_isWallDetected && _isCharging) {
				StopCharge();
			}
			else if (_playerDetected) {
				ChargeBehavior();
			}
			else if (_isCharging) {
				SlideToStop();
			}
			else {
				HandleMovement();

				if (_isWallDetected || !_isGroundinFrontDetected) {
					Flip();
					_idleTimer = _idleDuration;
				}
			}
		}
	}

	protected override void OnDrawGizmos() {
		base.OnDrawGizmos();
	}

	#endregion

	#region Private Methods
	/// <summary>
	/// Handles the charging behavior when player is detected
	/// </summary>
	private void ChargeBehavior() {
		if (!_isCharging) {
			_isCharging = true;
			_currentSpeed = _chargeSpeed;
			_hasPlayedChargeSound = false;
		}
		_rb.linearVelocity = new Vector2(_currentSpeed * _facingDir, _rb.linearVelocity.y);

		// Play the charge sound once with cooldown
		if (!_hasPlayedChargeSound) {
			StartCoroutine(PlayChargeSound());
		}
	}

	/// <summary>
	/// Gradually slows down the chicken after charging
	/// </summary>
	private void SlideToStop() {
		_currentSpeed = Mathf.Max(_normalSpeed, _currentSpeed - _slidingDeceleration * Time.deltaTime);

		_rb.linearVelocity = new Vector2(_currentSpeed * _facingDir, _rb.linearVelocity.y);

		if (_currentSpeed <= _normalSpeed) {
			_isCharging = false;
			_currentSpeed = _normalSpeed;
			_idleTimer = _idleDuration;
		}
	}

	/// <summary>
	/// Stops the charging behavior when hitting a wall
	/// </summary>
	private void StopCharge() {
		_isCharging = false;
		_playerDetected = false;
		_currentSpeed = _normalSpeed;
		_rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
		_idleTimer = _idleDuration * 1.5f;
	}

	#endregion

	#region Coroutines

	private IEnumerator PlayChargeSound() {
		AudioManager.Instance.PlaySFXOnce("SFX_ChickenCharge");
		_hasPlayedChargeSound = true;
		yield return new WaitForSeconds(1f);
		_hasPlayedChargeSound = false;
	}

	#endregion
}
