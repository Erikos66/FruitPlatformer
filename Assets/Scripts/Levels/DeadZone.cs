using UnityEngine;

public class DeadZone : MonoBehaviour {

	#region Unity Methods

	/// <summary>
	/// Detects when players enter the dead zone and eliminates them
	/// </summary>
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.TryGetComponent<Player>(out var player)) {
			player.Die();
		}
	}

	#endregion
}
