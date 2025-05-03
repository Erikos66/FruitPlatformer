using UnityEngine;

public class DeadZone : MonoBehaviour {
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.TryGetComponent<Player>(out var player)) {
			player.Die();
		}
	}
}
