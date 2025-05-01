using UnityEngine;

public class CheckPoint : MonoBehaviour {
	private Animator anim;
	public Transform respawnPoint;
	private bool canBeActivated = true;
	private bool isActivated = false;

	private void Awake() {
		anim = GetComponent<Animator>();
	}

	private void ActiveCheckPoint() {
		if (isActivated)
			return;
		// Play the checkpoint sound
		AudioManager.Instance.PlaySFX("SFX_FlagUp");
		isActivated = true;
		canBeActivated = false;

		// Update the spawn point in PlayerManager directly
		PlayerManager.Instance.SetSpawnPoint(respawnPoint);
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		Player player = collision.GetComponent<Player>();
		if (player == null)
			return;
		if (canBeActivated) {
			ActiveCheckPoint();
			anim.SetBool("isActive", true);
		}
	}
}
