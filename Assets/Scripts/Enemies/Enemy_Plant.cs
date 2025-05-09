using UnityEngine;

public class Enemy_Plant : Enemy_Base {
	[Header("Plant Enemy Specific Settings")]
	[SerializeField] private float attackCooldown = 3f;
	private float attackCooldownTimer = 0f;
	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private Transform bulletSpawnPoint;
	[SerializeField] private float bulletSpeed = 5f;

	protected override void Update() {
		base.Update();

		if (attackCooldownTimer > 0) {
			attackCooldownTimer -= Time.deltaTime;
		}

		if (DetectedPlayer() && attackCooldownTimer <= 0) {
			Attack();
		}
	}

	private void Attack() {
		anim.SetTrigger("onAttack");
		attackCooldownTimer = attackCooldown;

		// We'll spawn the bullet at the animation event
		// This allows us to synchronize the bullet spawn with the attack animation
	}

	// This method will be called by an animation event during the attack animation
	public void SpawnBullet() {
		if (isDead) return;
		if (bulletPrefab == null || bulletSpawnPoint == null) {
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
		GameObject bulletObj = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);

		// Get the Enemy_Bullet component and initialize it
		Enemy_Bullet bullet = bulletObj.GetComponent<Enemy_Bullet>();
		if (bullet != null) {
			bullet.Initialize(targetPosition, bulletSpeed);
		}
		else {
			Debug.LogError("Enemy_Bullet component not found on bullet prefab!");
		}
	}
}
