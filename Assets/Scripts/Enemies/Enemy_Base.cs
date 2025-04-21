using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.InputSystem.XInput;

public class Enemy_Base : MonoBehaviour {

    [Header("Enemy Properties")]
    [SerializeField] protected int health = 1;
    [SerializeField] protected int damage = 1;
    [SerializeField] protected float moveSpeed = 1;
    [SerializeField] protected float idleDuration = 1;
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
    [SerializeField] protected float playerCheckDistance = 1f;
    [SerializeField] protected float wallCheckDistance = 1f;
    protected bool isWallDetected;
    protected bool isGroundinFrontDetected;
    protected bool isGrounded;
    [Space]
    [Header("Death Properties")]
    [SerializeField] protected float deathTime = 1f;
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

    public virtual void Die() {
        if (isDead) return;
        isDead = true;

        anim.SetTrigger("onHit");

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, deathImpactForce);

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders) {
            collider.enabled = false;
        }

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        float rotationDirection = Random.value < 0.5f ? -1f : 1f;
        rb.angularVelocity = rotationDirection * deathRotationSpeed;

        Destroy(gameObject, 5f);
    }
}
