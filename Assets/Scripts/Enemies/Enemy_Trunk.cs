using UnityEngine;

public class Enemy_Trunk : Base_Enemy_Class {

	#region Variables

	[Header("Trunk Enemy Specific Settings")]
	[SerializeField] private float _attackCooldown = 2.5f;       // Time between attacks
	[SerializeField] private GameObject _bulletPrefab;           // Prefab for bullet projectile
	[SerializeField] private Transform _bulletSpawnPoint;        // Location to spawn bullets
	[SerializeField] private float _bulletSpeed = 5f;            // Speed of fired bullets
	[SerializeField] private float _stopDuration = 1.5f;         // Duration to stop for attack

	private float _attackCooldownTimer = 0f;                     // Current attack cooldown timer
	private float _stopTimer = 0f;                               // Current stop timer
	private bool _isShooting = false;                            // Whether trunk is shooting

	#endregion

	#region Unity Methods

	protected override void Update() {
		base.Update();

		if (_isDead) return; // Skip if the enemy is dead

		// Handle attack cooldown
		if (_attackCooldownTimer > 0) {
			_attackCooldownTimer -= Time.deltaTime;
		}

		// Handle stop timer when shooting
		if (_stopTimer > 0) {
			_stopTimer -= Time.deltaTime;
			if (_stopTimer <= 0) {
				_isShooting = false;
				// Resume movement after stopping for attack
			}
		}

		// Handle collision detection
		HandleCollision();

		// If player is detected and cooldown is ready, stop and attack
		if (DetectedPlayer() && _attackCooldownTimer <= 0 && !_isShooting) {
			StopAndAttack();
		}
		// Otherwise, if not shooting, handle movement
		else if (!_isShooting && _isGrounded) {
			HandleMovement();

			// Flip logic when wall detected or ledge ahead
			if (_isWallDetected || !_isGroundinFrontDetected) {
				Flip();
				_idleTimer = _idleDuration;
			}
		}

		// Update animation
		_anim.SetFloat("xVelocity", Mathf.Abs(_rb.linearVelocity.x));
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Stops movement and initiates attack
	/// </summary>
	private void StopAndAttack() {
		// Stop moving
		_rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
		_isShooting = true;
		_stopTimer = _stopDuration;

		// Attack
		Attack();
	}

	/// <summary>
	/// Triggers attack animation and resets cooldown
	/// </summary>
	private void Attack() {
		_anim.SetTrigger("onAttack");
		_attackCooldownTimer = _attackCooldown;

		// Bullet will be spawned via animation event through SpawnBullet method
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Spawns a bullet projectile aimed at player
	/// Called by Animation Event
	/// </summary>
	public void SpawnBullet() {
		if (_bulletPrefab == null || _bulletSpawnPoint == null) {
			Debug.LogWarning("Bullet prefab or spawn point is not assigned!");
			return;
		}

		// Find the player to determine target location
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		if (player == null) {
			Debug.LogWarning("Player not found!");
			return;
		}

		// Play attack sound
		AudioManager.Instance.PlaySFX("SFX_Shoot");

		Vector2 targetPosition = player.transform.position;

		// Instantiate the bullet at the spawn point
		GameObject bulletObj = Instantiate(_bulletPrefab, _bulletSpawnPoint.position, Quaternion.identity);

		// Get the Enemy_Bullet component and initialize it
		Enemy_Bullet bullet = bulletObj.GetComponent<Enemy_Bullet>();
		if (bullet != null) {
			bullet.Initialize(targetPosition, _bulletSpeed);
		}
		else {
			Debug.LogError("Enemy_Bullet component not found on bullet prefab!");
		}
	}

	#endregion
}
