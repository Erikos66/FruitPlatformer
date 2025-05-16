using UnityEngine;

public class EndPoint : MonoBehaviour {

	#region Variables

	[Header("Component References")]
	private Animator _anim;         // Reference to the animator component

	#endregion

	#region Unity Methods

	/// <summary>
	/// Initialize components
	/// </summary>
	private void Awake() {
		_anim = GetComponent<Animator>();
	}

	/// <summary>
	/// Handles level completion when player reaches endpoint
	/// </summary>
	private void OnTriggerEnter2D(Collider2D collision) {
		Player player = collision.GetComponent<Player>();
		if (player) {
			// Play level finish sound
			AudioManager.Instance.PlaySFX("SFX_Finish");

			_anim.SetTrigger("activate");
			Debug.Log("Level complete!");

			// Explicitly stop the timer when player reaches the end point
			TimerManager.Instance.StopLevelTimer();

			// Mark the level as finished and handle progression
			LevelManager.Instance.LevelFinished();

			player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
		}
	}

	#endregion
}
