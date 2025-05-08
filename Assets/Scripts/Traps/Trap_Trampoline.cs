using UnityEngine;

public class Trap_Trampoline : MonoBehaviour {
	private Animator anim;
	[SerializeField] private float pushForce = 50f;
	[SerializeField] private float disableDelay = 0f;

	private void Awake() {
		anim = GetComponent<Animator>();
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.TryGetComponent<Player>(out var player)) {
			anim.SetTrigger("spring");
			player.PushPlayer(transform.up * pushForce, disableDelay);
			AudioManager.Instance.PlaySFX("SFX_Piston");
		}
	}
}
