using UnityEngine;

public class Trap_Fire : MonoBehaviour {
	#region Variables
	private Animator _anim; // Reference to the animator component
	public bool isOn = true; // Is the fire trap on
	#endregion

	#region Unity Methods
	private void Awake() {
		_anim = GetComponent<Animator>();
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Toggles the fire trap on or off.
	/// </summary>
	public virtual void Toggle() {
		isOn = !isOn;
		_anim.SetBool("active", isOn);
	}
	#endregion

	#region Unity Events
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.TryGetComponent<Player>(out var player)) {
			if (!isOn)
				player.Knockback(0.5f, new Vector2(15, 10), transform.position);
		}
	}
	#endregion
}
