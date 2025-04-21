using UnityEngine;

public class Enemy_Chicken : Enemy_Base {
    [Header("Chicken Specific")]
    [SerializeField] private float sightDistance = 5f;
    [SerializeField] private float sightAngle = 45f; // Cone angle (half of the full cone)
    [SerializeField] private float chargeSpeed = 4f;
    [SerializeField] private float slidingDeceleration = 2f;
    [SerializeField] private LayerMask playerDetectionLayer;

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

        // Check for player in sight
        CheckVisionCone();

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

    private void CheckVisionCone() {
        // Get all colliders within the sight distance
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, sightDistance, playerDetectionLayer);

        playerDetected = false;

        foreach (Collider2D collider in colliders) {
            Vector2 directionToTarget = (collider.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(new Vector2(facingDir, 0), directionToTarget);

            // Check if target is within the vision cone angle
            if (angle <= sightAngle) {
                // Check if there are no obstacles between chicken and player
                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    directionToTarget,
                    Vector2.Distance(transform.position, collider.transform.position),
                    groundLayer
                );

                if (hit.collider == null) {
                    playerDetected = true;
                    break;
                }
            }
        }

        if (playerDetected && !isCharging) {
            isCharging = true;
            currentSpeed = chargeSpeed;
        }
    }

    private void ChargeBehavior() {
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

    protected override void OnDrawGizmos() {
        base.OnDrawGizmos();

        // Draw vision cone
        Gizmos.color = Color.red;

        // Define the cone's direction based on facing direction
        Vector2 coneDirection = new Vector2(facingDir, 0);

        // Calculate the cone's vertices
        float angleInRadians = sightAngle * Mathf.Deg2Rad;
        Vector2 leftRayDir = RotateVector(coneDirection, angleInRadians);
        Vector2 rightRayDir = RotateVector(coneDirection, -angleInRadians);

        // Draw the cone's outline
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + leftRayDir * sightDistance);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + rightRayDir * sightDistance);

        // Draw an arc to visualize the cone
        Vector2 previousPoint = (Vector2)transform.position + rightRayDir * sightDistance;

        int segments = 20;
        for (int i = 1; i <= segments; i++) {
            float t = i / (float)segments;
            float currentAngle = Mathf.Lerp(-sightAngle, sightAngle, t) * Mathf.Deg2Rad;
            Vector2 currentDir = RotateVector(coneDirection, currentAngle);
            Vector2 currentPoint = (Vector2)transform.position + currentDir * sightDistance;

            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }

    private Vector2 RotateVector(Vector2 vector, float angle) {
        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);

        return new Vector2(
            vector.x * cosAngle - vector.y * sinAngle,
            vector.x * sinAngle + vector.y * cosAngle
        );
    }
}
