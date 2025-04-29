using UnityEngine;

public class Enemy_Trunk : Enemy_Base {
    [Header("Trunk Enemy Specific Settings")]
    [SerializeField] private float attackCooldown = 2.5f;
    private float attackCooldownTimer = 0f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private float stopDuration = 1.5f;
    private float stopTimer = 0f;
    private bool isShooting = false;

    protected override void Update() {
        base.Update();

        if (isDead) return; // Skip if the enemy is dead

        // Handle attack cooldown
        if (attackCooldownTimer > 0) {
            attackCooldownTimer -= Time.deltaTime;
        }

        // Handle stop timer when shooting
        if (stopTimer > 0) {
            stopTimer -= Time.deltaTime;
            if (stopTimer <= 0) {
                isShooting = false;
                // Resume movement after stopping for attack
            }
        }

        // Mushroom-like collision handling
        HandleCollision();

        // If player is detected and cooldown is ready, stop and attack
        if (DetectedPlayer() && attackCooldownTimer <= 0 && !isShooting) {
            StopAndAttack();
        }
        // Otherwise, if not shooting, handle movement like mushroom
        else if (!isShooting && isGrounded) {
            HandleMovement();

            // Flip logic from mushroom
            if (isWallDetected || !isGroundinFrontDetected) {
                Flip();
                idleTimer = idleDuration;
            }
        }

        // Update animation
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
    }

    private void StopAndAttack() {
        // Stop moving
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isShooting = true;
        stopTimer = stopDuration;

        // Attack
        Attack();
    }

    private void Attack() {
        anim.SetTrigger("onAttack");
        attackCooldownTimer = attackCooldown;
        // Play attack sound
        AudioManager.Instance.PlaySFX("SFX_Shoot");

        // Bullet will be spawned via animation event through SpawnBullet method
    }

    // This method will be called by an animation event during the attack animation
    public void SpawnBullet() {
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