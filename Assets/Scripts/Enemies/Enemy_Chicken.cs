using UnityEngine;

public class Enemy_Chicken : Enemy_Base {
    [Header("Chicken Specific")]
    [SerializeField] private float chargeSpeed = 4f;
    [SerializeField] private float slidingDeceleration = 2f;
    // sightDistance, sightAngle, and playerDetectionLayer are now removed as we're using the base class detection

    private bool playerDetected;
    private bool isCharging;
    private float normalSpeed;
    private float currentSpeed;

    protected override void Awake() {
        base.Awake();
        normalSpeed = moveSpeed;
        currentSpeed = normalSpeed;
    }

    protected override void Update() {
        base.Update();

        if (isDead) {
            return;
        }

        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));

        // Check for player using the base class method
        playerDetected = DetectedPlayer();

        HandleCollision();
        if (isGrounded) {
            // Stop charging if we hit a wall
            if (isWallDetected && isCharging) {
                StopCharge();
            }
            else if (playerDetected) {
                ChargeBehavior();
            }
            else if (isCharging) {
                SlideToStop();
            }
            else {
                HandleMovement();

                if (isWallDetected || !isGroundinFrontDetected) {
                    Flip();
                    idleTimer = idleDuration;
                }
            }
        }
    }

    // Removed CheckVisionCone method as we're using the base class detection method

    private void ChargeBehavior() {
        if (!isCharging) {
            isCharging = true;
            currentSpeed = chargeSpeed;
        }
        rb.linearVelocity = new Vector2(currentSpeed * facingDir, rb.linearVelocity.y);
    }

    private void SlideToStop() {
        // Gradually slow down
        currentSpeed = Mathf.Max(normalSpeed, currentSpeed - slidingDeceleration * Time.deltaTime);

        rb.linearVelocity = new Vector2(currentSpeed * facingDir, rb.linearVelocity.y);

        // When we've slowed down enough, return to normal behavior
        if (currentSpeed <= normalSpeed) {
            isCharging = false;
            currentSpeed = normalSpeed;
            idleTimer = idleDuration; // Add a pause after charging
        }
    }

    private void StopCharge() {
        // Immediately stop charging when hitting a wall
        isCharging = false;
        playerDetected = false;
        currentSpeed = normalSpeed;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Stop horizontal movement
        idleTimer = idleDuration * 1.5f; // A bit longer idle time after hitting a wall
    }

    // The RotateVector method and cone visualization in OnDrawGizmos is no longer needed
    protected override void OnDrawGizmos() {
        base.OnDrawGizmos(); // This will include the player detection ray from the base class
    }
}
