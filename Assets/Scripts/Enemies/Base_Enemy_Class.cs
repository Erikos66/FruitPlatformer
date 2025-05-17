using UnityEngine;

public class Base_Enemy_Class : MonoBehaviour, IDamageable {

	#region Variables

	[Header("Enemy Properties")]
	[SerializeField] protected float _moveSpeed = 1;                 // Movement speed
	[SerializeField] protected float _idleDuration = 1;              // Time spent idle between movements
	[SerializeField] protected bool _startFacingRight = false;       // Toggle to control starting direction
	protected float _idleTimer = 1f;                                 // Timer for idle state
	protected int _facingDir = -1;                                   // Direction (-1 left, 1 right)
	protected bool _facingRight = false;                             // Whether enemy is facing right
	protected bool _isDead = false;                                  // Whether enemy is dead

	[Header("Enemy Components")]
	protected Animator _anim;                                        // Reference to animator component
	protected Rigidbody2D _rb;                                       // Reference to rigidbody component
	[SerializeField] protected Transform _flipPivot;                 // Optional pivot point for flipping the enemy

	[Header("Collision Properties")]
	[SerializeField] protected LayerMask _groundLayer;               // Layer for ground detection
	[SerializeField] protected LayerMask _playerLayer;               // Layer for player detection
	[SerializeField] protected Transform _ledgeDetectionTransform;   // Transform for ledge detection
	[SerializeField] protected Transform _floorDetectionTransform;   // Transform for floor detection
	[SerializeField] protected Transform _wallDetectionTransform;    // Transform for wall detection
	[SerializeField] protected float _floorCheckDistance = 1f;       // Distance to check for floor
	[SerializeField] protected float _wallCheckDistance = 1f;        // Distance to check for walls
	[SerializeField] protected float _ledgeCheckDistance = 1f;       // Distance to check for ledges
	protected bool _isWallDetected;                                  // Whether wall is detected
	protected bool _isGroundinFrontDetected;                         // Whether ground in front is detected
	protected bool _isGrounded;                                      // Whether enemy is grounded
	protected DamageTrigger _dt;                                     // Reference to damage trigger component

	[Header("Player Detection")]
	[SerializeField] protected DetectionShape _detectionShape = DetectionShape.Line;  // Shape for detecting player
	[SerializeField] protected float _playerDetectionDistance = 10f;  // Distance for player detection
	[SerializeField] protected float _heightOffset = 0f;              // Height offset for detection
	[SerializeField] protected float _sideOffset = 0f;                // Side offset for detection
	[SerializeField] protected float _coneAngle = 30f;                // Angle for cone detection
	[SerializeField] protected float _rectangleHeight = 1f;           // Height for rectangle detection
	[SerializeField] protected float _sphereRadius = 2f;              // Radius for sphere detection

	[Header("Death Properties")]
	[SerializeField] protected float _despawnTime = 1f;               // Time before destroying gameObject after death
	[SerializeField] protected float _deathRotationSpeed = 100f;      // Rotation speed after death
	[SerializeField] protected float _deathImpactForce;               // Upward force applied on death

	#endregion

	#region Unity Methods

	protected virtual void Awake() {
		_anim = GetComponent<Animator>();
		if (_anim == null) {
			Debug.LogError("Animator component not found on " + gameObject.name);
		}
		_rb = GetComponent<Rigidbody2D>();
		if (_rb == null) {
			Debug.LogError("Rigidbody2D component not found on " + gameObject.name);
		}
		_dt = GetComponentInChildren<DamageTrigger>();
		if (_dt == null) {
			Debug.LogError("DamageTrigger component not found in children of " + gameObject.name);
		}

		// Initialize facing direction based on _startFacingRight toggle
		_facingDir = _startFacingRight ? 1 : -1;
		_facingRight = _startFacingRight;

		// Apply initial rotation if needed
		if (_startFacingRight) {
			transform.Rotate(0f, 180f, 0f);
		}
	}

	protected virtual void Update() {
		if (_isDead) return;
		_idleTimer -= Time.deltaTime;
	}

	protected virtual void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		// Draw the gizmos for the ground check, wall check, and ledge check
		Gizmos.DrawRay(_floorDetectionTransform.position, Vector2.down * _floorCheckDistance);
		Gizmos.DrawRay(_wallDetectionTransform.position, Vector2.right * _facingDir * _wallCheckDistance);
		Gizmos.DrawRay(_ledgeDetectionTransform.position, Vector2.down * _floorCheckDistance);

		if (_detectionShape == DetectionShape.None) {
			return;
		}

		Vector2 startPosition = (Vector2)transform.position + new Vector2(_sideOffset * _facingDir, _heightOffset);

		Gizmos.color = Color.red;

		switch (_detectionShape) {
			case DetectionShape.Line:
				Vector3 rayDirection = new Vector3(_facingDir, 0, 0);
				Gizmos.DrawRay(startPosition, rayDirection * _playerDetectionDistance);
				break;

			case DetectionShape.Cone:
				float halfAngle = _coneAngle * 0.5f;
				float angleStep = _coneAngle / 5f;

				for (float angle = -halfAngle; angle <= halfAngle; angle += angleStep) {
					float radians = Mathf.Deg2Rad * angle;
					Vector2 direction = new Vector2(
						Mathf.Cos(radians) * _facingDir - Mathf.Sin(radians) * 0,
						Mathf.Sin(radians) * _facingDir + Mathf.Cos(radians) * 0
					).normalized;

					Gizmos.DrawRay(startPosition, direction * _playerDetectionDistance);
				}
				break;

			case DetectionShape.Rectangle:
				Vector2 boxSize = new Vector2(_playerDetectionDistance, _rectangleHeight);
				Vector2 boxCenter = startPosition + new Vector2(_playerDetectionDistance * 0.5f * _facingDir, 0);

				Vector2 halfSize = boxSize * 0.5f;
				Vector2 topLeft = boxCenter + new Vector2(-halfSize.x * _facingDir, halfSize.y);
				Vector2 topRight = boxCenter + new Vector2(halfSize.x * _facingDir, halfSize.y);
				Vector2 bottomLeft = boxCenter + new Vector2(-halfSize.x * _facingDir, -halfSize.y);
				Vector2 bottomRight = boxCenter + new Vector2(halfSize.x * _facingDir, -halfSize.y);

				Gizmos.DrawLine(topLeft, topRight);
				Gizmos.DrawLine(topRight, bottomRight);
				Gizmos.DrawLine(bottomRight, bottomLeft);
				Gizmos.DrawLine(bottomLeft, topLeft);
				break;

			case DetectionShape.Sphere:
				Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
				Gizmos.DrawWireSphere(startPosition, _sphereRadius);
				break;
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Handles enemy death, applies death effects and destroys the object
	/// </summary>
	public virtual void Die() {
		if (_isDead) return;
		_isDead = true;
		_rb.freezeRotation = false;
		if (_dt != null) {
			_dt.DisableDamageTrigger();
		}
		_anim.SetTrigger("onHit");

		_rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _deathImpactForce);
		Collider2D[] colliders = GetComponents<Collider2D>();
		foreach (Collider2D collider in colliders) {
			collider.enabled = false;
		}

		gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

		float rotationDirection = Random.value < 0.5f ? -1f : 1f;
		_rb.angularVelocity = rotationDirection * _deathRotationSpeed;

		Destroy(gameObject, _despawnTime);
	}

	#endregion

	#region Protected Methods

	/// <summary>
	/// Flips the enemy based on the movement direction
	/// </summary>
	/// <param name="xValue">Movement value on X-axis</param>
	protected void HandleFlip(float xValue) {
		if (xValue < 0 && _facingRight || xValue > 0 && !_facingRight) {
			Flip();
		}
	}

	/// <summary>
	/// Handles collision detection for walls, ledges, and ground
	/// </summary>
	protected virtual void HandleCollision() {
		_isWallDetected = Physics2D.Raycast(_wallDetectionTransform.position, Vector2.right * _facingDir, _wallCheckDistance, _groundLayer);
		_isGroundinFrontDetected = Physics2D.Raycast(_ledgeDetectionTransform.position, Vector2.down, _floorCheckDistance, _groundLayer);
		_isGrounded = Physics2D.Raycast(_floorDetectionTransform.position, Vector2.down, _floorCheckDistance, _groundLayer);
	}

	/// <summary>
	/// Handles enemy movement based on facing direction
	/// </summary>
	protected virtual void HandleMovement() {
		if (_idleTimer > 0) return;
		_rb.linearVelocity = new Vector2(_moveSpeed * _facingDir, _rb.linearVelocity.y);
	}

	/// <summary>
	/// Flips the enemy's direction and transforms
	/// </summary>
	protected virtual void Flip() {
		_facingDir *= -1;
		_facingRight = !_facingRight;
		_rb.linearVelocity = Vector2.zero;

		if (_flipPivot != null) {
			// Save the world position of the pivot
			Vector3 pivotWorldPos = _flipPivot.position;

			// Get the current parent position
			Vector3 parentPos = transform.position;

			// Calculate the position difference relative to pivot
			Vector3 relativePos = parentPos - pivotWorldPos;

			// Flip the X position relative to pivot
			relativePos.x *= -1;

			// Set the new position (pivot + flipped relative position)
			transform.position = pivotWorldPos + relativePos;

			// Rotate the object
			transform.Rotate(0f, 180f, 0f);
		}
		else {
			// Use the original rotation method if no pivot point is assigned
			transform.Rotate(0f, 180f, 0f);
		}
	}

	/// <summary>
	/// Checks if player is detected using the selected detection method
	/// </summary>
	/// <returns>True if player is detected, false otherwise</returns>
	protected virtual bool DetectedPlayer() {
		if (_detectionShape == DetectionShape.None) {
			return false;
		}

		Vector2 startPosition = (Vector2)transform.position + new Vector2(_sideOffset * _facingDir, _heightOffset);

		switch (_detectionShape) {
			case DetectionShape.Line:
				return DetectPlayerWithLine(startPosition);
			case DetectionShape.Cone:
				return DetectPlayerWithCone(startPosition);
			case DetectionShape.Rectangle:
				return DetectPlayerWithRectangle(startPosition);
			case DetectionShape.Sphere:
				return DetectPlayerWithSphere(startPosition);
			default:
				return false;
		}
	}

	/// <summary>
	/// Detects player using a line raycast
	/// </summary>
	/// <param name="startPosition">Starting position of the raycast</param>
	/// <returns>True if player is detected, false otherwise</returns>
	protected virtual bool DetectPlayerWithLine(Vector2 startPosition) {
		RaycastHit2D hit = Physics2D.Raycast(
			startPosition,
			new Vector2(_facingDir, 0),
			_playerDetectionDistance,
			_playerLayer | _groundLayer
		);

		return hit.collider != null && ((1 << hit.collider.gameObject.layer) & _playerLayer) != 0;
	}

	/// <summary>
	/// Detects player using multiple raycasts in a cone shape
	/// </summary>
	/// <param name="startPosition">Starting position of the raycasts</param>
	/// <returns>True if player is detected, false otherwise</returns>
	protected virtual bool DetectPlayerWithCone(Vector2 startPosition) {
		float halfAngle = _coneAngle * 0.5f;
		float angleStep = _coneAngle / 5f;

		for (float angle = -halfAngle; angle <= halfAngle; angle += angleStep) {
			float radians = Mathf.Deg2Rad * angle;
			Vector2 direction = new Vector2(
				Mathf.Cos(radians) * _facingDir - Mathf.Sin(radians) * 0,
				Mathf.Sin(radians) * _facingDir + Mathf.Cos(radians) * 0
			).normalized;

			RaycastHit2D hit = Physics2D.Raycast(
				startPosition,
				direction,
				_playerDetectionDistance,
				_playerLayer | _groundLayer
			);

			if (hit.collider != null && ((1 << hit.collider.gameObject.layer) & _playerLayer) != 0) {
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Detects player using a rectangular area
	/// </summary>
	/// <param name="startPosition">Starting position for the rectangle</param>
	/// <returns>True if player is detected, false otherwise</returns>
	protected virtual bool DetectPlayerWithRectangle(Vector2 startPosition) {
		Vector2 boxSize = new Vector2(_playerDetectionDistance, _rectangleHeight);
		Vector2 boxCenter = startPosition + new Vector2(_playerDetectionDistance * 0.5f * _facingDir, 0);

		Collider2D hit = Physics2D.OverlapBox(
			boxCenter,
			boxSize,
			0f,
			_playerLayer
		);

		return hit != null;
	}

	/// <summary>
	/// Detects player using a spherical area
	/// </summary>
	/// <param name="startPosition">Center of the sphere</param>
	/// <returns>True if player is detected, false otherwise</returns>
	protected virtual bool DetectPlayerWithSphere(Vector2 startPosition) {
		Collider2D hit = Physics2D.OverlapCircle(
			startPosition,
			_sphereRadius,
			_playerLayer
		);

		return hit != null;
	}

	#endregion
}
