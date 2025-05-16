using UnityEngine;

public class Enemy_Plant : Base_Enemy_Class {

	#region Variables

	[Header("Plant Enemy Specific Settings")]
	[SerializeField] private float _attackCooldown = 3f;       // Time between attacks
	[SerializeField] private GameObject _bulletPrefab;         // Prefab for bullet projectile
	[SerializeField] private Transform _bulletSpawnPoint;      // Location to spawn bullets
	[SerializeField] private float _bulletSpeed = 5f;          // Speed of fired bullets

	private float _attackCooldownTimer = 0f;                   // Current attack cooldown timer

	#endregion

	#region Unity Methods

	protected override void Update() {
		base.Update();

		if (_attackCooldownTimer > 0) {
			_attackCooldownTimer -= Time.deltaTime;
		}

		if (DetectedPlayer() && _attackCooldownTimer <= 0) {
			Attack();
		}
	}

	#endregion

	#region Private Methods

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
		if (_isDead) return;
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
