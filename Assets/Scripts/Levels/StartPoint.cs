using System.Collections;
using UnityEngine;

public class StartPoint : MonoBehaviour {

	#region Variables

	[Header("Component References")]
	private Animator _anim;                      // Reference to the animator component

	[Header("Spawn Properties")]
	[SerializeField] public Transform respawnPoint;  // Point where player will spawn

	#endregion

	#region Unity Methods

	private void Awake() {
		_anim = GetComponentInChildren<Animator>();
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.TryGetComponent<Player>(out var player)) {
			AnimateFlag();
			// Timer is now started when player spawns, not here
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Triggers the flag waving animation
	/// </summary>
	public void AnimateFlag() {
		_anim.SetTrigger("waveflag");
	}

	#endregion
}
