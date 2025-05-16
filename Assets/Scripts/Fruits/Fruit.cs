using System.Collections;
using UnityEngine;

public class Fruit : MonoBehaviour {

	#region Variables

	[SerializeField] private string _fruitID;     // Unique ID for this fruit
	private Animator _anim;                       // Reference to the animator component
	private bool _hasBeenCollectedBefore = false; // Tracks if this fruit was already collected

	#endregion

	#region Unity Methods

	private void Awake() {
		_anim = GetComponentInChildren<Animator>();

		// If no ID has been set in the inspector, generate one based on position
		if (string.IsNullOrEmpty(_fruitID)) {
			_fruitID = $"{gameObject.scene.name}_fruit_{transform.position.x}_{transform.position.y}";
		}
	}

	private void Start() {
		// Check if GameManager instance exists before attempting to use it
		// The functionality for random fruits would now be in a different manager
		if (GameManager.Instance != null) {
			SetRandomFruit();
		}

		// Check if this fruit has been collected before
		CheckIfAlreadyCollected();
	}

	private void OnTriggerEnter2D(Collider2D other) {
		Player player = other.GetComponent<Player>();
		if (player != null && !_hasBeenCollectedBefore) {
			// Play random pickup sound
			AudioManager.Instance.PlayRandomSFX("SFX_PickUp");

			_anim.SetTrigger("Collected");
			StartCoroutine(DestroyFruit());
			Destroy(GetComponent<Collider2D>());

			// Use FruitManager to track collected fruit
			if (GameManager.Instance != null)
				FruitManager.Instance.CollectFruit();

			// Mark this specific fruit as collected
			string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
			string key = $"Fruit_{currentScene}_{_fruitID}";
			PlayerPrefs.SetInt(key, 1);
			PlayerPrefs.Save();
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Checks if this fruit has been collected in a previous session
	/// </summary>
	private void CheckIfAlreadyCollected() {
		string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		string key = $"Fruit_{currentScene}_{_fruitID}";

		// If this fruit was collected in a previous session, disable it
		if (PlayerPrefs.GetInt(key, 0) == 1) {
			_hasBeenCollectedBefore = true;
			gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Sets a random fruit type via the animator
	/// </summary>
	private void SetRandomFruit() {
		int random = Random.Range(0, 9);
		_anim.SetFloat("fruitType", random);
	}

	#endregion

	#region Coroutines

	/// <summary>
	/// Destroys the fruit object after animation completes
	/// </summary>
	private IEnumerator DestroyFruit() {
		yield return new WaitForSeconds(0.8f);
		Destroy(gameObject);
	}

	#endregion
}
