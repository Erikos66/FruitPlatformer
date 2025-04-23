using UnityEngine;


public enum DetectionShape {
    None,
    Line,
    Cone,
    Rectangle,
    Sphere
}

public class Enemy_Base : MonoBehaviour {

    [Header("Enemy Properties")]
    [SerializeField] protected int health = 1;
    [SerializeField] protected int damage = 1;
    [SerializeField] protected float moveSpeed = 1;
    [SerializeField] protected float idleDuration = 1;
    [SerializeField] protected bool startFacingRight = false;  // Toggle to control starting direction
    protected float idleTimer = 1f;
    protected int facingDir = -1;
    protected bool facingRight = false;
    protected bool isDead = false;
    [Space]
    [Header("Enemy Components")]
    protected Animator anim;
    protected Rigidbody2D rb;
    [Space]
    [Header("Collision Properties")]
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected Transform groundTransform;
    [SerializeField] protected float groundCheckDistance = 1f;
    [SerializeField] protected float wallCheckDistance = 1f;
    protected bool isWallDetected;
    protected bool isGroundinFrontDetected;
    protected bool isGrounded;
    protected DamageTrigger dt;
    [Space]
    [Header("Player Detection")]
    [SerializeField] protected DetectionShape detectionShape = DetectionShape.Line;
    [SerializeField] protected float playerDetectionDistance = 10f;
    [SerializeField] protected float heightOffset = 0f;
    [SerializeField] protected float sideOffset = 0f;
    [SerializeField] protected float coneAngle = 30f;
    [SerializeField] protected float rectangleHeight = 1f;
    [SerializeField] protected float sphereRadius = 2f;
    [Space]
    [Header("Death Properties")]
    [SerializeField] protected float despawnTime = 1f;
    [SerializeField] protected float deathRotationSpeed = 100f;
    [SerializeField] protected float deathImpactForce;

    protected virtual void Awake() {
        anim = GetComponent<Animator>();
        if (anim == null) {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) {
            Debug.LogError("Rigidbody2D component not found on " + gameObject.name);
        }
        dt = GetComponentInChildren<DamageTrigger>();
        if (dt == null) {
            Debug.LogError("DamageTrigger component not found in children of " + gameObject.name);
        }

        // Initialize facing direction based on startFacingRight toggle
        facingDir = startFacingRight ? 1 : -1;
        facingRight = startFacingRight;

        // Apply initial rotation if needed
        if (startFacingRight) {
            transform.Rotate(0f, 180f, 0f);
        }
    }

    protected virtual void Update() {
        idleTimer -= Time.deltaTime;
    }

    protected void HandleFlip(float xValue) {
        if (xValue < 0 && facingRight || xValue > 0 && !facingRight) {
            Flip();
        }
    }

    protected virtual void HandleCollision() {
        isWallDetected = Physics2D.Raycast(groundTransform.position, Vector2.right * facingDir, wallCheckDistance, groundLayer);
        isGroundinFrontDetected = Physics2D.Raycast(groundTransform.position, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
    }

    protected virtual void HandleMovement() {
        if (idleTimer > 0) return;
        rb.linearVelocity = new Vector2(moveSpeed * facingDir, rb.linearVelocity.y);
    }

    protected virtual void Flip() {
        facingDir *= -1;
        transform.Rotate(0f, 180f, 0f);
        facingRight = !facingRight;
        rb.linearVelocity = Vector2.zero;
    }

    protected virtual bool DetectedPlayer() {
        if (detectionShape == DetectionShape.None) {
            return false;
        }

        Vector2 startPosition = (Vector2)transform.position + new Vector2(sideOffset * facingDir, heightOffset);

        switch (detectionShape) {
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

    protected virtual bool DetectPlayerWithLine(Vector2 startPosition) {
        RaycastHit2D hit = Physics2D.Raycast(
            startPosition,
            new Vector2(facingDir, 0),
            playerDetectionDistance,
            playerLayer | groundLayer
        );

        return hit.collider != null && ((1 << hit.collider.gameObject.layer) & playerLayer) != 0;
    }

    protected virtual bool DetectPlayerWithCone(Vector2 startPosition) {
        float halfAngle = coneAngle * 0.5f;
        float angleStep = coneAngle / 5f;

        for (float angle = -halfAngle; angle <= halfAngle; angle += angleStep) {
            float radians = Mathf.Deg2Rad * angle;
            Vector2 direction = new Vector2(
                Mathf.Cos(radians) * facingDir - Mathf.Sin(radians) * 0,
                Mathf.Sin(radians) * facingDir + Mathf.Cos(radians) * 0
            ).normalized;

            RaycastHit2D hit = Physics2D.Raycast(
                startPosition,
                direction,
                playerDetectionDistance,
                playerLayer | groundLayer
            );

            if (hit.collider != null && ((1 << hit.collider.gameObject.layer) & playerLayer) != 0) {
                return true;
            }
        }

        return false;
    }

    protected virtual bool DetectPlayerWithRectangle(Vector2 startPosition) {
        Vector2 boxSize = new Vector2(playerDetectionDistance, rectangleHeight);
        Vector2 boxCenter = startPosition + new Vector2(playerDetectionDistance * 0.5f * facingDir, 0);

        Collider2D hit = Physics2D.OverlapBox(
            boxCenter,
            boxSize,
            0f,
            playerLayer
        );

        return hit != null;
    }

    protected virtual bool DetectPlayerWithSphere(Vector2 startPosition) {
        Collider2D hit = Physics2D.OverlapCircle(
            startPosition,
            sphereRadius,
            playerLayer
        );

        return hit != null;
    }

    protected virtual void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        if (groundTransform != null) {
            Gizmos.DrawLine(groundTransform.position, new Vector2(groundTransform.position.x, groundTransform.position.y - groundCheckDistance));
            Gizmos.DrawLine(groundTransform.position, new Vector2(groundTransform.position.x + wallCheckDistance * facingDir, groundTransform.position.y));
        }
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));

        if (detectionShape == DetectionShape.None) {
            return;
        }

        Vector2 startPosition = (Vector2)transform.position + new Vector2(sideOffset * facingDir, heightOffset);

        Gizmos.color = Color.red;

        switch (detectionShape) {
            case DetectionShape.Line:
                Vector3 rayDirection = new Vector3(facingDir, 0, 0);
                Gizmos.DrawRay(startPosition, rayDirection * playerDetectionDistance);
                break;

            case DetectionShape.Cone:
                float halfAngle = coneAngle * 0.5f;
                float angleStep = coneAngle / 5f;

                for (float angle = -halfAngle; angle <= halfAngle; angle += angleStep) {
                    float radians = Mathf.Deg2Rad * angle;
                    Vector2 direction = new Vector2(
                        Mathf.Cos(radians) * facingDir - Mathf.Sin(radians) * 0,
                        Mathf.Sin(radians) * facingDir + Mathf.Cos(radians) * 0
                    ).normalized;

                    Gizmos.DrawRay(startPosition, direction * playerDetectionDistance);
                }
                break;

            case DetectionShape.Rectangle:
                Vector2 boxSize = new Vector2(playerDetectionDistance, rectangleHeight);
                Vector2 boxCenter = startPosition + new Vector2(playerDetectionDistance * 0.5f * facingDir, 0);

                Vector2 halfSize = boxSize * 0.5f;
                Vector2 topLeft = boxCenter + new Vector2(-halfSize.x * facingDir, halfSize.y);
                Vector2 topRight = boxCenter + new Vector2(halfSize.x * facingDir, halfSize.y);
                Vector2 bottomLeft = boxCenter + new Vector2(-halfSize.x * facingDir, -halfSize.y);
                Vector2 bottomRight = boxCenter + new Vector2(halfSize.x * facingDir, -halfSize.y);

                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
                Gizmos.DrawLine(bottomLeft, topLeft);
                break;

            case DetectionShape.Sphere:
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Gizmos.DrawWireSphere(startPosition, sphereRadius);
                break;
        }
    }

    public virtual void Die() {
        if (isDead) return;
        isDead = true;
        rb.freezeRotation = false;
        dt.DisableDamageTrigger();
        anim.SetTrigger("onHit");

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, deathImpactForce);
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders) {
            collider.enabled = false;
        }

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        float rotationDirection = Random.value < 0.5f ? -1f : 1f;
        rb.angularVelocity = rotationDirection * deathRotationSpeed;

        Destroy(gameObject, despawnTime);
    }
}
