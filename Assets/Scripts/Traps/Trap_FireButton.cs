using System.Collections;
using UnityEngine;

public class Trap_FireButton : MonoBehaviour {
	#region Variables
	private Animator _anim; // Reference to the animator component

	[Header("Fire Traps")]
	[SerializeField] private Trap_Fire[] _firetrap; // Array of fire traps

	private bool _pressed = false; // Is button pressed

	[Header("Button Properties")]
	[SerializeField] private bool _toggleable = false; // Is button toggleable
	[SerializeField] private bool _resetAfterTime = false; // Should reset after time
	[SerializeField] private float _resetDelay = 0.5f; // Delay before reset
	[SerializeField] private float _resetTimer = 3f; // Timer for reset
	#endregion

	#region Unity Methods
	private void Awake() {
		_anim = GetComponent<Animator>();
	}
	#endregion

	#region Unity Events
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.TryGetComponent<Player>(out var player)) {
			StopAllCoroutines();
			if (_resetAfterTime) {
				if (!_pressed) {
					foreach (var trap in _firetrap)
						trap.Toggle();
					_pressed = true;
					_anim.SetBool("pressed", true);
					AudioManager.Instance.PlaySFX("SFX_Press");
					StartCoroutine(ResetAfterTime());
				}
			}
			else {
				if (!_pressed) {
					if (_toggleable) {
						foreach (var trap in _firetrap)
							trap.Toggle();
						AudioManager.Instance.PlaySFX("SFX_Press");
						_pressed = !_pressed;
						_anim.SetBool("pressed", _pressed);
					}
					else {
						foreach (var trap in _firetrap)
							trap.Toggle();
						AudioManager.Instance.PlaySFX("SFX_Press");
						_pressed = true;
						_anim.SetBool("pressed", true);
					}
				}
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.TryGetComponent<Player>(out var player)) {
			if (!_resetAfterTime && _toggleable)
				StartCoroutine(ResetButton());
		}
	}
	#endregion

	#region Coroutines
	private IEnumerator ResetButton() {
		yield return new WaitForSeconds(_resetDelay);
		_pressed = false;
		_anim.SetBool("pressed", false);
	}

	private IEnumerator ResetAfterTime() {
		yield return new WaitForSeconds(_resetTimer);
		foreach (var trap in _firetrap)
			trap.Toggle();
		_pressed = false;
		_anim.SetBool("pressed", false);
	}
	#endregion
}
