using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour {
    // Component references
    private Rigidbody2D rb; // Rigidbody2D component for physics simulation
    private Animator anim;  // Animator component for animations

    [Header("Movement")]
    [SerializeField] private float moveSpeed;       // Movement speed of the player
    [SerializeField] private float jumpForce;         // Force applied when jumping
    [SerializeField] private float doubleJumpForce;   // Force applied during a double jump
    private bool canDoubleJump;                       // Indicates if a double jump is available

    [Header("Buffer & Coyote jump")]
    [SerializeField] private float bufferJumpWindow = .25f; // Allowed time window to buffer a jump input
    private float bufferJumpActivated = -1;                 // Timestamp when buffer jump was activated
    [SerializeField] private float coyoteJumpWindow = .5f;    // Time window for coyote jump (grace period)
    private float coyoteJumpActivated = -1;                 // Timestamp for coyote jump availability

    [Header("Wall interactions")]
    [SerializeField]
    private float wallJumpDuration = .6f; // Duration of wall jump effect
    [SerializeField] private Vector2 wallJumpForce;        // Force vector applied during wall jump
    private bool isWallJumping;                           // Indicates if the player is currently wall jumping

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 1;  // Duration during which knockback is active
    [SerializeField] private Vector2 knockbackPower;       // Force vector used for knockback
    private bool isKnocked;                               // Indicates if the player is currently knocked back

    [Header("Collision")]
    [SerializeField] private float groundCheckDistnace;    // Distance for ground collision check
    [SerializeField] private float wallCheckDistance;      // Distance for wall collision check
    [SerializeField] private LayerMask whatIsGround;       // Layer mask used to determine ground objects
    private bool isGrounded;                              // Whether the player is on the ground
    private bool isAirborne;                              // Whether the player is in the air
    private bool isWallDetected;                          // Whether a wall is detected in front of the player

    private float xInput; // Horizontal input from player
    private float yInput; // Vertical input from player

    private bool facingRight = true; // Indicates if the player is initially facing right
    private int facingDir = 1;       // Numerical representation of facing direction (1 = right, -1 = left)

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update() {
        UpdateAirbornStatus();

        if (isKnocked)
            return;

        HandleInput();
        HandleWallSlide();
        HandleMovement();
        HandleFlip();
        HandleCollision();
        HandleAnimations();
    }

    /// <summary>
    /// Initiates knockback effect on the player.
    /// </summary>
    public void Knockback() {
        if (isKnocked)
            return;

        StartCoroutine(KnockbackRoutine());
        anim.SetTrigger("knockback");
        rb.linearVelocity = new Vector2(knockbackPower.x * -facingDir, knockbackPower.y);
    }

    /// <summary>
    /// Coroutine that handles the knockback duration.
    /// </summary>
    private IEnumerator KnockbackRoutine() {
        isKnocked = true;

        yield return new WaitForSeconds(knockbackDuration);

        isKnocked = false;
    }

    /// <summary>
    /// Updates airborne status and handles landing events.
    /// </summary>
    private void UpdateAirbornStatus() {
        if (isGrounded && isAirborne)
            HandleLanding();

        if (!isGrounded && !isAirborne)
            BecomeAirborne();
    }

    /// <summary>
    /// Marks the player as airborne and activates coyote jump if falling.
    /// </summary>
    private void BecomeAirborne() {
        isAirborne = true;

        if (rb.linearVelocity.y < 0)
            ActivateCoyoteJump();
    }

    /// <summary>
    /// Handles landing events: resets airborne state and enables double jump.
    /// </summary>
    private void HandleLanding() {
        isAirborne = false;
        canDoubleJump = true;

        AttemptBufferJump();
    }

    /// <summary>
    /// Processes player input for movement and jump actions.
    /// </summary>
    private void HandleInput() {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space)) {
            JumpButton();
            RequestBufferJump();
        }
    }

    #region Coyote & Buffer Jump

    /// <summary>
    /// Records the time when the jump button is pressed in the air (buffering jump input).
    /// </summary>
    private void RequestBufferJump() {
        if (isAirborne)
            bufferJumpActivated = Time.time;
    }

    /// <summary>
    /// Checks if a buffered jump input is valid and performs the jump if so.
    /// </summary>
    private void AttemptBufferJump() {
        if (Time.time < bufferJumpActivated + bufferJumpWindow) {
            bufferJumpActivated = Time.time - 1;
            Jump();
        }
    }

    /// <summary>
    /// Activates the coyote jump window by recording the current time.
    /// </summary>
    private void ActivateCoyoteJump() => coyoteJumpActivated = Time.time;

    /// <summary>
    /// Cancels coyote jump availability by modifying its timestamp.
    /// </summary>
    private void CancelCoyoteJump() => coyoteJumpActivated = Time.time - 1;

    #endregion

    /// <summary>
    /// Determines and performs the appropriate jump action (ground, wall, or double jump).
    /// </summary>
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

    /// <summary>
    /// Performs a jump by setting the vertical velocity to jumpForce.
    /// </summary>
    private void Jump() => rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

    /// <summary>
    /// Executes a double jump, resets wall jump state, and disables further double jumps.
    /// </summary>
    private void DoubleJump() {
        StopCoroutine(WallJumpRoutine());
        isWallJumping = false;
        canDoubleJump = false;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
    }

    /// <summary>
    /// Executes a wall jump by applying a force opposite to the current facing direction, flips the player, and begins the wall jump routine.
    /// </summary>
    private void WallJump() {
        canDoubleJump = true;
        rb.linearVelocity = new Vector2(wallJumpForce.x * -facingDir, wallJumpForce.y);

        Flip();
        StartCoroutine(WallJumpRoutine());
    }

    /// <summary>
    /// Coroutine that manages the duration of a wall jump.
    /// </summary>
    private IEnumerator WallJumpRoutine() {
        isWallJumping = true;

        yield return new WaitForSeconds(wallJumpDuration);

        isWallJumping = false;
    }

    /// <summary>
    /// Adjusts player's vertical velocity for a wall slide when a wall is detected.
    /// </summary>
    private void HandleWallSlide() {
        bool canWallSlide = isWallDetected && rb.linearVelocity.y < 0;
        float yModifer = yInput < 0 ? 1 : .05f;

        if (!canWallSlide)
            return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * yModifer);
    }

    /// <summary>
    /// Uses raycasts to update collision statuses (grounded and wall detection).
    /// </summary>
    private void HandleCollision() {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistnace, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    }

    /// <summary>
    /// Updates animation parameters based on the current physics state of the player.
    /// </summary>
    private void HandleAnimations() {
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallDetected", isWallDetected);
    }

    /// <summary>
    /// Applies horizontal movement if the player is not wall jumping or sliding.
    /// </summary>
    private void HandleMovement() {
        if (isWallDetected)
            return;

        if (isWallJumping)
            return;

        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Checks for input direction change and initiates a sprite flip if needed.
    /// </summary>
    private void HandleFlip() {
        if (xInput < 0 && facingRight || xInput > 0 && !facingRight)
            Flip();
    }

    /// <summary>
    /// Flips the player's direction by rotating and toggling the facing indicators.
    /// </summary>
    private void Flip() {
        facingDir *= -1;
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }

    /// <summary>
    /// Draws gizmos in the editor to visualize ground and wall detection distances.
    /// </summary>
    private void OnDrawGizmos() {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistnace));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDir), transform.position.y));
    }
}
