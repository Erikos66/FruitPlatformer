using UnityEngine;

public class Trap_Trampoline : MonoBehaviour {
	#region Variables
	private Animator _anim; // Animator reference
	[SerializeField] private float _pushForce = 50f; // Force to push player
	[SerializeField] private float _disableDelay = 0f; // Delay before disabling
	#endregion

	#region Unity Methods
	private void Awake() {
		_anim = GetComponent<Animator>();
	}
	#endregion

	#region Unity Events
	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.TryGetComponent<Player>(out var player)) {
			_anim.SetTrigger("spring");
			player.PushPlayer(transform.up * _pushForce, _disableDelay);
			AudioManager.Instance.PlaySFX("SFX_Piston");
		}
	}
	#endregion
}
