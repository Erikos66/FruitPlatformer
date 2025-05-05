using UnityEngine;

public class Trap_Fire : MonoBehaviour {
	private Animator anim;
	public bool isOn = true;

	private void Awake() {
		anim = GetComponent<Animator>();
	}

	public virtual void Toggle() {
		isOn = !isOn;
		anim.SetBool("active", isOn);
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.TryGetComponent<Player>(out var player)) {
			if (!isOn) {
				player.Knockback(0.5f, new Vector2(15, 10), transform.position);
			}
		}
	}
}
