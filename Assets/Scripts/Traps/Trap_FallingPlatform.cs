using UnityEngine;
using System.Collections;

public class Trap_FallingPlatform : MonoBehaviour {
    [SerializeField] private float fallDelay = 1f;
    [SerializeField] private float floatingSpeed = 1f;
    [SerializeField] private float hoverAmplitude = 0.2f;
    [SerializeField] private float rechargeDuration = 0.5f;
    [SerializeField] private float lerpDuration = 2f;
    private bool isFalling = false;
    private bool playerOnPlatform = false;
    private bool isLerping = false;
    private Vector3 startPos;
    private Animator anim;
    private Rigidbody2D rb;
    private BoxCollider2D[] col;

    private void Awake() {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        col = GetComponents<BoxCollider2D>();
    }

    private void Update() {
        if (!isFalling && !isLerping)
            transform.position = startPos + new Vector3(0, hoverAmplitude * Mathf.Sin(Time.time * floatingSpeed), 0);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.TryGetComponent<Player>(out var player)) {
            playerOnPlatform = true;
            CancelInvoke(nameof(CheckPlayerPresence));
            anim.SetTrigger("deactivate");
            Invoke(nameof(SwitchOffPlatform), fallDelay);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.TryGetComponent<Player>(out var player)) {
            playerOnPlatform = false;
            if (isFalling) {
                CancelInvoke(nameof(CheckPlayerPresence));
                Invoke(nameof(CheckPlayerPresence), rechargeDuration);
            }
        }
    }

    private void CheckPlayerPresence() {
        if (!playerOnPlatform) {
            anim.SetTrigger("reactivate");
            SwitchOnPlatform();
        }
    }

    private void SwitchOffPlatform() {
        isFalling = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2;
        if (!playerOnPlatform) {
            CancelInvoke(nameof(CheckPlayerPresence));
            Invoke(nameof(CheckPlayerPresence), rechargeDuration);
        }
    }

    private void SwitchOnPlatform() {
        isFalling = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        isLerping = true;
        StartCoroutine(LerpBackToStart());
    }

    private IEnumerator LerpBackToStart() {
        Vector3 currentPos = transform.position;
        float elapsed = 0f;
        while (elapsed < lerpDuration) {
            transform.position = Vector3.Lerp(currentPos, startPos, elapsed / lerpDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = startPos;
        rb.bodyType = RigidbodyType2D.Kinematic;
        isLerping = false;
    }
}
