using UnityEngine;
using System.Collections;

public class Enemy_Snail : Enemy_Base {
    [Header("Snail Specific")]
    [SerializeField] private float shellSlideSpeed = 8f;
    [SerializeField] private float bounceForce = 5f;
    [SerializeField] private float wallBounceDelay = 0.1f;
    [SerializeField] private LayerMask enemyLayer;

    private enum SnailState { WithShell, NoShell }
    private SnailState currentState = SnailState.WithShell;
    private bool isSliding = false;
    private bool isBouncingOffWall = false;

    protected override void Awake() {
        base.Awake();
    }

    protected override void Update() {
        base.Update();

        if (isDead) return;

        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));

        HandleCollision();

        switch (currentState) {
            case SnailState.WithShell:
                if (isGrounded) {
                    HandleMovement();

                    if (isWallDetected || !isGroundinFrontDetected) {
                        Flip();
                        idleTimer = idleDuration;
                    }
                }
                break;

            case SnailState.NoShell:
                if (isGrounded && isSliding && !isBouncingOffWall) {
                    SlideShell();

                    if (isWallDetected) {
                        StartCoroutine(DelayedBounceOffWall());
                    }
                }
                break;
        }
    }

    private void SlideShell() {
        rb.linearVelocity = new Vector2(shellSlideSpeed * facingDir, rb.linearVelocity.y);
    }

    private IEnumerator DelayedBounceOffWall() {
        // Set flag to prevent multiple bounces while waiting
        isBouncingOffWall = true;

        // Stop the shell
        rb.linearVelocity = Vector2.zero;

        // Play wall hit animation
        anim.SetTrigger("onWallHit");

        // Wait for the delay
        yield return new WaitForSeconds(wallBounceDelay);

        // Bounce off wall
        Flip();

        // Re-enable sliding
        isBouncingOffWall = false;
    }

    // Called when player jumps on the snail
    public override void Die() {
        if (isDead) return;

        switch (currentState) {
            case SnailState.WithShell:
                // Change to NoShell state instead of dying
                currentState = SnailState.NoShell;
                anim.SetTrigger("onHit");
                rb.linearVelocity = Vector2.zero;
                isSliding = false;
                break;

            case SnailState.NoShell:
                if (!isSliding) {
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

    private void LaunchShell() {
        isSliding = true;

        // Find player position to determine launch direction
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            // Face away from the player (shell gets kicked in direction player is facing)
            float dirToPlayer = player.transform.position.x > transform.position.x ? 1 : -1;

            // If player's direction is different than current facing direction, flip
            if (dirToPlayer != facingDir) {
                Flip();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        // Only perform collision effects when in NoShell state and sliding
        if (currentState == SnailState.NoShell && isSliding) {
            // Check if shell hit player
            if (((1 << collision.gameObject.layer) & playerLayer) != 0) {
                Player player = collision.gameObject.GetComponent<Player>();
                if (player != null) {
                    // Apply knockback to player
                    Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                    Vector2 knockbackPower = new Vector2(
                        knockbackDirection.x * bounceForce,
                        bounceForce  // Keep the vertical component consistent
                    );

                    player.Knockback(0.5f, knockbackPower);
                }
            }

            // Check if shell hit another enemy
            if (((1 << collision.gameObject.layer) & enemyLayer) != 0) {
                // Try to get Enemy_Base component from the collided object or its parent
                Enemy_Base enemy = collision.gameObject.GetComponent<Enemy_Base>();
                if (enemy == null) {
                    enemy = collision.gameObject.GetComponentInParent<Enemy_Base>();
                }

                // Make sure we don't kill ourselves
                if (enemy != null && enemy != this) {
                    enemy.Die();
                }
            }
        }
    }
}
