using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
	// Singleton instance
	public static PlayerManager Instance { get; private set; }

	[Header("Player Settings")]
	[SerializeField] private GameObject playerPrefab;
	[SerializeField] private Transform spawnPoint;

	private GameObject currentPlayer;
	private bool firstSpawn = true;

	private void Awake() {
		// Singleton setup
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
		}
		else if (Instance != this) {
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Set the current spawn point for the player
	/// </summary>
	public void SetSpawnPoint(Transform newSpawnPoint) {
		if (newSpawnPoint != null) {
			spawnPoint = newSpawnPoint;
			Debug.Log("Spawn point updated to: " + newSpawnPoint.position);
		}
	}

	/// <summary>
	/// Spawn or respawn the player at the current spawn point
	/// </summary>
	public void RespawnPlayer() {
		if (currentPlayer != null) {
			Destroy(currentPlayer);
		}

		if (spawnPoint == null) {
			// Try to find a spawn point in the scene
			spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint")?.transform;
			if (spawnPoint == null) {
				Debug.LogError("No spawn point found in the scene!");
				return;
			}
		}

		// Instantiate player prefab at spawn point
		currentPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);

		// Apply the selected skin using SkinManager
		if (SkinManager.Instance != null) {
			SkinManager.Instance.ApplySelectedSkin(currentPlayer);
		}

		// Set up camera to follow player
		CameraManager.Instance.SetTargetToFollow(currentPlayer.transform);

		//Play the respawn sound
		AudioManager.Instance.PlayRandomSFX("SFX_Respawn");

		// Start the timer on first spawn only
		if (firstSpawn) {
			TimerManager.Instance.StartLevelTimer();
			firstSpawn = false;
		}

		// Add a small delay before enabling player control
		StartCoroutine(EnablePlayerControlAfterDelay(0.5f));
	}

	private IEnumerator EnablePlayerControlAfterDelay(float delay) {
		yield return new WaitForSeconds(delay);
		Player playerComponent = currentPlayer.GetComponent<Player>();
		if (playerComponent != null) {
			playerComponent.EnableControl();
		}
	}

	/// <summary>
	/// Handle player death and respawn
	/// </summary>
	public void PlayerDied() {
		if (currentPlayer != null) {
			// Create death VFX
			Player playerComponent = currentPlayer.GetComponent<Player>();
			if (playerComponent != null && playerComponent.playerDeath_VFX != null) {
				Instantiate(playerComponent.playerDeath_VFX, currentPlayer.transform.position, Quaternion.identity);
			}

			currentPlayer = null;

			// Respawn player after a delay
			StartCoroutine(DelayedRespawn(1f));
		}
	}

	private IEnumerator DelayedRespawn(float delay) {
		yield return new WaitForSeconds(delay);
		RespawnPlayer();
	}

	/// <summary>
	/// Get the current player GameObject
	/// </summary>
	public GameObject GetCurrentPlayer() {
		return currentPlayer;
	}

	/// <summary>
	/// Reset the first spawn flag (called when loading a new level)
	/// </summary>
	public void ResetFirstSpawnFlag() {
		firstSpawn = true;
	}
}
