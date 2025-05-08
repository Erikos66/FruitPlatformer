using System.Collections;
using UnityEngine;

public class Trap_FireButton : MonoBehaviour {
	private Animator anim;
	[Header("Fire Traps")]
	[SerializeField] private Trap_Fire[] firetrap;

	private bool pressed = false;

	[Header("Button Properties")]
	[SerializeField] private bool toggleable = false;
	[SerializeField] private bool resetAftertime = false;
	[SerializeField] private float resetDelay = 0.5f;
	[SerializeField] private float resetTimer = 3f;

	private void Awake() {
		anim = GetComponent<Animator>();
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.TryGetComponent<Player>(out var player)) {
			StopAllCoroutines();
			if (resetAftertime) {
				if (!pressed) {
					foreach (var trap in firetrap)
						trap.Toggle();
					pressed = true;
					anim.SetBool("pressed", true);
					AudioManager.Instance.PlaySFX("SFX_Press");
					StartCoroutine(ResetAfterTime());
				}
			}
			else {
				if (!pressed) {
					if (toggleable) {
						foreach (var trap in firetrap)
							trap.Toggle();
						AudioManager.Instance.PlaySFX("SFX_Press");
						pressed = !pressed;
						anim.SetBool("pressed", pressed);
					}
					else {
						foreach (var trap in firetrap)
							trap.Toggle();
						AudioManager.Instance.PlaySFX("SFX_Press");
						pressed = true;
						anim.SetBool("pressed", true);
					}
				}
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.TryGetComponent<Player>(out var player)) {
			if (!resetAftertime && toggleable)
				StartCoroutine(ResetButton());
		}
	}

	private IEnumerator ResetButton() {
		yield return new WaitForSeconds(resetDelay);
		pressed = false;
		anim.SetBool("pressed", false);
	}

	private IEnumerator ResetAfterTime() {
		yield return new WaitForSeconds(resetTimer);
		foreach (var trap in firetrap)
			trap.Toggle();
		pressed = false;
		anim.SetBool("pressed", false);
	}
}
