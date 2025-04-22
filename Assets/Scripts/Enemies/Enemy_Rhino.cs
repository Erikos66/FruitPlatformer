using UnityEngine;

public class Enemy_Rhino : Enemy_Base {
    [Header("Rhino Specific")]
    [SerializeField] private float chargeSpeed = 16f;
    [SerializeField] private float returnSpeed = 3f;
    [SerializeField] private float bounceForce = 2f;
    [SerializeField] private float returnDelay = 2f;

    private Vector3 startingPosition;
    private bool isCharging;
    private bool isReturning;
    private bool isBouncing;
    private float normalSpeed;
    private float returnTimer;
    private int startingFacingDir;

    protected override void Awake() {
        base.Awake();
        normalSpeed = moveSpeed;
        startingPosition = transform.position;
        facingDir = startingFacingDir;
    }

    protected override void Update() {
        base.Update();

        if (isDead) {
            return;
        }

        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));

        HandleCollision();

        if (isGrounded) {
            if (isBouncing) {
                isBouncing = false;
                isCharging = false;
                returnTimer = returnDelay;
            }
            else if (returnTimer > 0) {
                returnTimer -= Time.deltaTime;
                if (returnTimer <= 0) {
                    isReturning = true;
                }
            }
            else if (isCharging) {
                ChargeBehavior();
                if (isWallDetected) {
                    StartBounce();
                }
            }
            else if (isReturning) {
                ReturnBehavior();
                if (DetectedPlayer()) {
                    isCharging = true;
                    isReturning = false;
                    moveSpeed = chargeSpeed;
                }
            }
            else {
                if (DetectedPlayer()) {
                    isCharging = true;
                    isReturning = false;
                    moveSpeed = chargeSpeed;
                }
            }
        }
    }

    private void ChargeBehavior() {
        rb.linearVelocity = new Vector2(moveSpeed * facingDir, rb.linearVelocity.y);
    }

    private void StartBounce() {
        isBouncing = true;
        anim.SetTrigger("onWallHit");
        rb.linearVelocity = new Vector2(-facingDir * moveSpeed * 0.5f, bounceForce);
    }

    private void ReturnBehavior() {
        int directionToStart = transform.position.x > startingPosition.x ? -1 : 1;

        if (directionToStart != facingDir) {
            Flip();
            return;
        }

        moveSpeed = returnSpeed;
        rb.linearVelocity = new Vector2(moveSpeed * facingDir, rb.linearVelocity.y);

        float distanceToStart = Vector2.Distance(
            new Vector2(transform.position.x, 0),
            new Vector2(startingPosition.x, 0)
        );

        if (distanceToStart < 0.5f) {
            transform.position = new Vector3(startingPosition.x, transform.position.y, transform.position.z);
            rb.linearVelocity = Vector2.zero;

            if (facingDir != startingFacingDir) {
                Flip();
            }

            isReturning = false;
            moveSpeed = normalSpeed;
        }
    }

    protected override void OnDrawGizmos() {
        base.OnDrawGizmos();

        if (Application.isPlaying) {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(startingPosition, 0.2f);
        }
    }
}