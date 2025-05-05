using System.Collections;
using UnityEngine;

public class Trap_FireButton : MonoBehaviour {
	private Animator anim; // Animator component for the button
	[Header("Fire Traps")]
	[SerializeField] private Trap_Fire[] firetrap; // Array of associated fire trap objects

	private bool pressed = false; // Indicates if the button is currently pressed

	[Space]
	[Header("Button Properties")]
	[SerializeField] private bool toggleable = false;         // Determines if button toggles on repeated presses
	[SerializeField] private bool resetAftertime = false;      // If enabled, resets fire traps after a timer
	[SerializeField] private float resetDelay = 0.5f;          // Delay before reset when not time-based
	[SerializeField] private float resetTimer = 3f;            // Duration to wait before auto-resetting (time-based)

	private void Awake() {
		anim = GetComponent<Animator>(); // Initialize the animator component
	}

	// Called when the player enters the trigger area.
	// If resetAftertime is enabled, toggles traps and schedules reset after resetTimer seconds.
	// Otherwise, toggles traps based on the toggleable property.
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.TryGetComponent<Player>(out var player)) {
			StopAllCoroutines();
			if (resetAftertime) {
				if (!pressed) {
					foreach (var trap in firetrap) {
						trap.Toggle();
					}
					pressed = true;
					anim.SetBool("pressed", true);
					AudioManager.Instance.PlaySFX("SFX_Press"); // Play button press sound
					StartCoroutine(ResetAfterTime()); // Custom coroutine to reset after a duration
				}
			}
			else {
				if (!pressed) {
					if (toggleable) {
						foreach (var trap in firetrap) {
							trap.Toggle();
						}
						AudioManager.Instance.PlaySFX("SFX_Press"); // Play button press sound
						pressed = !pressed;
						anim.SetBool("pressed", pressed);
					}
					else {
						foreach (var trap in firetrap) {
							trap.Toggle();
						}
						AudioManager.Instance.PlaySFX("SFX_Press"); // Play button press sound
						pressed = true;
						anim.SetBool("pressed", true);
					}
				}
			}
		}
	}

	// Called when the player exits the trigger area.
	// For non-time-based toggleable buttons, starts the reset coroutine.
	private void OnTriggerExit2D(Collider2D other) {
		if (other.TryGetComponent<Player>(out var player)) {
			if (!resetAftertime && toggleable) {
				StartCoroutine(ResetButton()); // Custom coroutine to reset state after delay
			}
		}
	}

	// Coroutine: Resets button state after a short delay (resetDelay)
	private IEnumerator ResetButton() {
		yield return new WaitForSeconds(resetDelay);
		pressed = false;
		anim.SetBool("pressed", false);
	}

	// Coroutine: For time-based resets, waits for resetTimer seconds, toggles traps, then resets the button.
	private IEnumerator ResetAfterTime() {
		yield return new WaitForSeconds(resetTimer);
		foreach (var trap in firetrap) {
			trap.Toggle();
		}
		pressed = false;
		anim.SetBool("pressed", false);
	}
}
