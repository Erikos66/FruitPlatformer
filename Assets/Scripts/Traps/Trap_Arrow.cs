using System.Collections;
using UnityEngine;

public class Trap_Arrow : MonoBehaviour {
	private Animator anim;
	private Collider2D col;
	[SerializeField] private float pushForce = 30f;
	[SerializeField] private float disableDelay = 0.3f;
	[SerializeField] private float rotationSpeed = 100f;
	[SerializeField] private bool rotateRight;
	[SerializeField] private float rechargeTime = 2f;

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

	private void Update() {
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

	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.TryGetComponent<Player>(out var player)) {
			anim.SetTrigger("spring");
			player.PushPlayer(transform.up * pushForce, disableDelay);
			AudioManager.Instance.PlaySFX("SFX_Boost");
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

