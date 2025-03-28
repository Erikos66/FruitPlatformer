using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour {
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Visuals")]
    public GameObject playerDeath_VFX;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private bool canDoubleJump;
    private bool canBeControlled = false;

    [Header("Buffer & Coyote jump")]
    [SerializeField] private float bufferJumpWindow = .25f;
    private float bufferJumpActivated = -1;
    [SerializeField] private float coyoteJumpWindow = .5f;
    private float coyoteJumpActivated = -1;

    [Header("Wall interactions")]
    [SerializeField]
    private float wallJumpDuration = .6f;
    [SerializeField] private Vector2 wallJumpForce;
    private bool isWallJumping;

    [Header("Knockback")]
    [SerializeField] private Vector2 knockbackPower;
    private bool isKnocked;

    [Header("Collision")]
    [SerializeField] private float groundCheckDistnace;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;
    private bool isAirborne;
    private bool isWallDetected;

    private float xInput;
    private float yInput;

    private bool facingRight = true;
    private int facingDir = 1;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update() {
        UpdateAirbornStatus();
        if (!canBeControlled) {
            HandleWallSlide();
            HandleCollision();
            HandleAnimations();
            return;
        }
        if (isKnocked)
            return;
        HandleInput();
        HandleWallSlide();
        HandleMovement();
        HandleFlip();
        HandleCollision();
        HandleAnimations();
    }

    public void EnableControl() => canBeControlled = true;

    public void PushPlayer(Vector2 pushPower, float duration = 0) {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(pushPower, ForceMode2D.Impulse);
        StartCoroutine(PushControl(duration));
    }

    private IEnumerator PushControl(float duration) {
        canBeControlled = false;
        yield return new WaitForSeconds(duration);
        EnableControl();
    }

    public void Knockback(float knockbackDuration, Vector2 knockbackPower, Vector2? hitPosition = null) {
        if (isKnocked)
            return;
        StartCoroutine(KnockbackRoutine(knockbackDuration));
        anim.SetTrigger("knockback");
        Vector2 direction = hitPosition.HasValue ? (((Vector2)transform.position) - hitPosition.Value).normalized : new Vector2(-facingDir, 0);
        rb.linearVelocity = new Vector2(direction.x * knockbackPower.x, knockbackPower.y);
    }

    private IEnumerator KnockbackRoutine(float knockbackDuration) {
        isKnocked = true;
        yield return new WaitForSeconds(knockbackDuration);
        isKnocked = false;
    }

    private void UpdateAirbornStatus() {
        if (isGrounded && isAirborne)
            HandleLanding();
        if (!isGrounded && !isAirborne)
            BecomeAirborne();
    }

    private void BecomeAirborne() {
        isAirborne = true;
        if (rb.linearVelocity.y < 0)
            ActivateCoyoteJump();
    }

    private void HandleLanding() {
        isAirborne = false;
        canDoubleJump = true;
        AttemptBufferJump();
    }

    private void HandleInput() {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(KeyCode.Space)) {
            JumpButton();
            RequestBufferJump();
        }
    }

    private void RequestBufferJump() {
        if (isAirborne)
            bufferJumpActivated = Time.time;
    }

    private void AttemptBufferJump() {
        if (Time.time < bufferJumpActivated + bufferJumpWindow) {
            bufferJumpActivated = Time.time - 1;
            Jump();
        }
    }

    private void ActivateCoyoteJump() => coyoteJumpActivated = Time.time;

    private void CancelCoyoteJump() => coyoteJumpActivated = Time.time - 1;

    private void JumpButton() {
        bool coyoteJumpAvalible = Time.time < coyoteJumpActivated + coyoteJumpWindow;
        if (isGrounded || coyoteJumpAvalible) {
            Jump();
        }
        else if (isWallDetected && !isGrounded) {
            WallJump();
        }
        else if (canDoubleJump) {
            DoubleJump();
        }
        CancelCoyoteJump();
    }

    private void Jump() => rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

    private void DoubleJump() {
        StopCoroutine(WallJumpRoutine());
        isWallJumping = false;
        canDoubleJump = false;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
    }

    private void WallJump() {
        canDoubleJump = true;
        rb.linearVelocity = new Vector2(wallJumpForce.x * -facingDir, wallJumpForce.y);
        Flip();
        StartCoroutine(WallJumpRoutine());
    }

    private IEnumerator WallJumpRoutine() {
        isWallJumping = true;
        yield return new WaitForSeconds(wallJumpDuration);
        isWallJumping = false;
    }

    private void HandleWallSlide() {
        bool canWallSlide = isWallDetected && rb.linearVelocity.y < 0;
        float yModifer = yInput < 0 ? 1 : .05f;
        if (!canWallSlide)
            return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * yModifer);
    }

    private void HandleCollision() {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistnace, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    }

    private void HandleAnimations() {
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallDetected", isWallDetected);
    }

    private void HandleMovement() {
        if (isWallDetected)
            return;
        if (isWallJumping)
            return;
        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
    }

    private void HandleFlip() {
        if ((xInput < 0 && facingRight) || (xInput > 0 && !facingRight))
            Flip();
    }

    private void Flip() {
        facingDir *= -1;
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistnace));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDir), transform.position.y));
    }

    public void Die() {
        GameObject newPlayerDeathVFX = Instantiate(playerDeath_VFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}