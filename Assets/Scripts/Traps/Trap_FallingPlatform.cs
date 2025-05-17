using UnityEngine;
using System.Collections;

public class Trap_FallingPlatform : MonoBehaviour {
	#region Variables
	[SerializeField] private float _fallDelay = 1f; // Delay before platform falls
	[SerializeField] private float _floatingSpeed = 1f; // Speed of floating animation
	[SerializeField] private float _hoverAmplitude = 0.2f; // Amplitude of hover
	[SerializeField] private float _rechargeDuration = 0.5f; // Time before recharge
	[SerializeField] private float _lerpDuration = 2f; // Duration to lerp back
	[SerializeField] private float _shakeIntensity = 0.1f; // Intensity of the shake
	[SerializeField] private float _shakeFrequency = 50f; // Frequency of the shake

	private bool _isFalling = false; // Is platform falling
	private bool _playerOnPlatform = false; // Is player on platform
	private bool _isLerping = false; // Is platform lerping
	private bool _isShaking = false; // Is platform shaking
	private Vector3 _startPos; // Start position
	private Animator _anim; // Animator reference
	private Rigidbody2D _rb; // Rigidbody reference
	private BoxCollider2D[] _col; // Colliders
	private Vector3 _shakeOffset; // Current shake offset
	#endregion

	#region Unity Methods
	private void Awake() {
		_anim = GetComponent<Animator>();
		_rb = GetComponent<Rigidbody2D>();
		_startPos = transform.position;
		_col = GetComponents<BoxCollider2D>();
	}
	private void Update() {
		if (!_isFalling && !_isLerping) {
			if (_isShaking) {
				// Calculate hover position
				Vector3 hoverPosition = _startPos + new Vector3(0, _hoverAmplitude * Mathf.Sin(Time.time * _floatingSpeed), 0);

				// Calculate shake offset
				_shakeOffset = new Vector3(
					Mathf.Sin(Time.time * _shakeFrequency) * _shakeIntensity,
					0,
					0
				);

				// Apply both hover and shake
				transform.position = hoverPosition + _shakeOffset;
			}
			else {
				// Normal hovering when not shaking
				transform.position = _startPos + new Vector3(0, _hoverAmplitude * Mathf.Sin(Time.time * _floatingSpeed), 0);
			}
		}
	}
	#endregion

	#region Unity Events
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.TryGetComponent<Player>(out var player)) {
			_playerOnPlatform = true;
			CancelInvoke(nameof(CheckPlayerPresence));
			_anim.SetBool("isFalling", true);

			// Start shaking immediately as a warning
			_isShaking = true;

			// Then fall after delay
			Invoke(nameof(SwitchOffPlatform), _fallDelay);
		}
	}
	private void OnTriggerExit2D(Collider2D other) {
		if (other.TryGetComponent<Player>(out var player)) {
			_playerOnPlatform = false;
			_anim.SetBool("isFalling", false);

			// Stop shaking if player leaves and platform is not yet falling
			if (!_isFalling) {
				_isShaking = false;
				CancelInvoke(nameof(SwitchOffPlatform));
			}

			if (_isFalling) {
				CancelInvoke(nameof(CheckPlayerPresence));
				Invoke(nameof(CheckPlayerPresence), _rechargeDuration);
			}
		}
	}
	#endregion

	#region Private Methods
	private void CheckPlayerPresence() {
		if (!_playerOnPlatform) {
			_anim.SetBool("isFalling", false);
			SwitchOnPlatform();
		}
	}
	private void SwitchOffPlatform() {
		_isFalling = true;
		_isShaking = false; // Stop shaking when actually falling
		_rb.bodyType = RigidbodyType2D.Dynamic;
		_rb.gravityScale = 2;
		if (!_playerOnPlatform) {
			CancelInvoke(nameof(CheckPlayerPresence));
			Invoke(nameof(CheckPlayerPresence), _rechargeDuration);
		}
	}
	private void SwitchOnPlatform() {
		_isFalling = false;
		_isShaking = false; // Ensure shaking is turned off when platform resets
		_rb.bodyType = RigidbodyType2D.Kinematic;
		_rb.linearVelocity = Vector2.zero;
		_isLerping = true;
		StartCoroutine(LerpBackToStart());
	}
	#endregion

	#region Coroutines
	private IEnumerator LerpBackToStart() {
		Vector3 currentPos = transform.position;
		float elapsed = 0f;
		while (elapsed < _lerpDuration) {
			transform.position = Vector3.Lerp(currentPos, _startPos, elapsed / _lerpDuration);
			elapsed += Time.deltaTime;
			yield return null;
		}
		transform.position = _startPos;
		_rb.bodyType = RigidbodyType2D.Kinematic;
		_isLerping = false;
	}
	#endregion
}
