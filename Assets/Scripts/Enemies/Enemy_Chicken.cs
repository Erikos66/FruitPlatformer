using UnityEngine;

public class Enemy_Chicken : Enemy_Base {
    [Header("Chicken Specific")]
    [SerializeField] private float chargeSpeed = 4f;
    [SerializeField] private float slidingDeceleration = 2f;

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

        playerDetected = DetectedPlayer();

        HandleCollision();
        if (isGrounded) {
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

    private void ChargeBehavior() {
        if (!isCharging) {
            isCharging = true;
            currentSpeed = chargeSpeed;
        }
        rb.linearVelocity = new Vector2(currentSpeed * facingDir, rb.linearVelocity.y);
        // play the charge sound
        AudioManager.Instance.PlaySFX("SFX_ChickenCharge");
    }

    private void SlideToStop() {
        currentSpeed = Mathf.Max(normalSpeed, currentSpeed - slidingDeceleration * Time.deltaTime);

        rb.linearVelocity = new Vector2(currentSpeed * facingDir, rb.linearVelocity.y);

        if (currentSpeed <= normalSpeed) {
            isCharging = false;
            currentSpeed = normalSpeed;
            idleTimer = idleDuration;
        }
    }

    private void StopCharge() {
        isCharging = false;
        playerDetected = false;
        currentSpeed = normalSpeed;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        idleTimer = idleDuration * 1.5f;
    }

    protected override void OnDrawGizmos() {
        base.OnDrawGizmos();
    }
}
