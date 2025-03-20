using System.Collections;
using UnityEngine;

public class Trap_Arrow : MonoBehaviour {

    private Animator anim;
    private Collider2D col;
    [SerializeField] private float pushForce = 30; // The force applied to the player when they land on the trampoline
    [SerializeField] private float disableDelay = 0.3f; // The force applied to the player when they land on the trampoline
    [SerializeField] private float rotationSpeed = 100f; // rotation speed of the arrow
    [SerializeField] private bool rotateRight; // rotate right or left
    [SerializeField] private float rechargeTime = 2f; // time to recharge the arrow
    private int direction = 1;
    private Vector3 activeScale = new(1f, 1f, 1f);
    private Vector3 inactiveScale = new(0.5f, 0.5f, 0.5f);
    private Vector3 targetScale;

    private void Awake() {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    private void Start() {
        targetScale = activeScale;
        EnableArrow();
    }

    void Update() {
        HandleRotation();
        HandleScale();
    }

    private void HandleScale() {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 0.01f);
    }

    private void HandleRotation() {
        direction = rotateRight ? -1 : 1;
        transform.Rotate(0, 0, rotationSpeed * direction * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.TryGetComponent<Player>(out var player)) {
            anim.SetTrigger("spring");
            player.PushPlayer(transform.up * pushForce, disableDelay);
        }
    }

    private IEnumerator ArrowRecharge(float time) {
        yield return new WaitForSeconds(time);
        anim.SetTrigger("reload");
    }

    public void EnableArrow() {
        targetScale = activeScale;
        col.enabled = true;
    }

    public void DisableArrow() {
        targetScale = inactiveScale;
        col.enabled = false;
        StartCoroutine(ArrowRecharge(rechargeTime));
    }
}

