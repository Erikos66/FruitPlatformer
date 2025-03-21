using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.InputSystem.XInput;

public class Enemy_Base : MonoBehaviour {

    [Header("Enemy Properties")]
    [SerializeField] protected int health = 1; // Health of the enemy
    [SerializeField] protected int damage = 1; // Damage dealt by the enemy
    [SerializeField] protected float moveSpeed = 1; // Movement speed of the enemy
    [SerializeField] protected float idleDuration = 1; // Duration to idle before moving
    protected float idleTimer = 1f; // Timer to track idle duration
    protected int facingDir = -1; // Direction the enemy is facing
    protected bool facingRight = false; // Indicates if the enemy is facing right
    [Space]
    [Header("Enemy Components")]
    protected Animator anim; // Animator component for the enemy
    protected Rigidbody2D rb; // Rigidbody2D component for the enemy
    [Space]
    [Header("Collision Properties")]
    [SerializeField] protected LayerMask groundLayer; // Layer mask for ground detection
    [SerializeField] protected LayerMask playerLayer; // Layer mask for player detection
    [SerializeField] protected Transform groundTransform; // Transform to check for ground
    [SerializeField] protected float groundCheckDistance = 1f; // Distance to check for ground
    [SerializeField] protected float playerCheckDistance = 1f; // Distance to check for player
    [SerializeField] protected float wallCheckDistance = 1f; // Distance to check for walls
    protected bool isWallDetected; // Indicates if a wall is detected
    protected bool isGroundinFrontDetected; // Indicates if ground is detected
    protected bool isGrounded; // Indicates if the enemy is grounded


    protected virtual void Awake() {
        anim = GetComponent<Animator>();
        if (anim == null) {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) {
            Debug.LogError("Rigidbody2D component not found on " + gameObject.name);
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

    private void OnDrawGizmos() {
        Gizmos.DrawLine(groundTransform.position, new Vector2(groundTransform.position.x, groundTransform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + wallCheckDistance * facingDir, transform.position.y));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
    }
}
