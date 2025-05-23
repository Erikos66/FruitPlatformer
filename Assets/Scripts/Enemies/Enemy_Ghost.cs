using System.Collections;
using UnityEngine;

public class Enemy_Ghost : Enemy_Flying_Base {

	// Gonna use regions to organize the code better, this is a personal preference.
	#region Variables

	[Header("Enemy Ghost Settings")]
	[SerializeField] private LayerMask groundLayer; // Layer for ground detection
	[SerializeField] private float moveSpeed = 2f; // Speed of the enemy
	[SerializeField] private float chargeSpeed = 5f; // Speed of the enemy when charging
	[SerializeField] private float waitDuration = 2f; // Duration to wait at each point
	[SerializeField] private float roamDistance = 5f; // Distance to roam from the starting position
	[SerializeField] private float damageRadius = 0.5f; // Radius for damage detection
	[SerializeField] private Vector2 knockbackDirection; // Direction of knockback effect
	[SerializeField] private float knockbackDuration = 1f; // Duration of the knockback effect
	[SerializeField] private bool debugMode = false; // Toggle for debug messages

	private enum State { Roaming, Chasing, Waiting } // Enum to represent the enemy's state
	private State currentState = State.Roaming; // Initial state of the enemy
	private State previousState; // Track previous state to detect state changes
	private Vector2 startingPosition; // Starting position of the enemy
	private Vector2[] roamPoints; // Array to store random roam points
	private int facingDirection = 1; // 1 for right, -1 for left
	private Vector2 targetPoint; // Target point for roaming
	private Vector2 playerPosition; // Position of the player
	private Coroutine waitCoroutine; // Coroutine for waiting state
	private bool isWaiting = false; // Flag to check if the enemy is waiting
	private bool isRoaming = false; // Flag to check if the enemy is roaming
	private bool isChasing = false; // Flag to check if the enemy is chasing
	private bool isPlayerDetected = false; // Flag to check if the player is detected
	private bool isReturningToStart = false; // Flag to check if the enemy is returning to the starting position
	private bool hasPlayedChargeSound = false; // Flag to track if charge sound has been played

	#endregion


	#region Unity Methods

	override protected void Awake() {
		base.Awake(); // Call the base class Awake method

		// Initialize the starting position of the enemy to its current position in the scene.
		// This will be used as the center point for generating random roam points.
		startingPosition = transform.position;
		targetPoint = startingPosition; // Set the target point to the starting position
		previousState = currentState; // Initialize previous state
	}

	private void Start() {

		// Generate random roam points within the specified distance from the starting position
		// and store them in the roamPoints array.
		roamPoints = new Vector2[5];
		for (int i = 0; i < roamPoints.Length; i++) {
			float x = UnityEngine.Random.Range(-roamDistance, roamDistance);
			float y = UnityEngine.Random.Range(-roamDistance, roamDistance);
			roamPoints[i] = new Vector2(startingPosition.x + x, startingPosition.y + y);
		}

	}

	private void Update() {
		if (isDead) return; // If the enemy is dead, exit the update method	

		// TODO: Make this not be as expensive, it's fine for now, but it could be better.
		anim.SetBool("isRoaming", isRoaming);

		// Debug the current state if debug mode is enabled, can safely remove this later.
		if (debugMode && Time.frameCount % 60 == 0) {
			Debug.Log($"Ghost State: {currentState}");
		}

		// Track state transitions
		bool stateChanged = previousState != currentState;
		if (stateChanged) {
			// Reset sound flag when transitioning out of chasing state
			if (previousState == State.Chasing) {
				hasPlayedChargeSound = false;
			}

			if (debugMode) {
				Debug.Log($"State changed from {previousState} to {currentState}");
			}
			previousState = currentState;
		}

		// handles the enemies states, roaming, chasing and waiting.
		// Currently also handling the flags for the states here, but could be moved to the states themselves.
		switch (currentState) {
			case State.Roaming:
				isRoaming = true;
				isChasing = false;
				isWaiting = false;
				Roam();
				break;
			case State.Chasing:
				isChasing = true;
				isRoaming = false;
				isWaiting = false;
				ChasePlayer();
				break;
			case State.Waiting:
				isWaiting = true;
				isRoaming = false;
				isChasing = false;
				Wait();
				break;
		}

		// Check for player detection and handle it accordingly.
		HandlePlayerDetection();
	}

	private void OnDrawGizmos() {

		// Draw a sphere in the scene view to visualize the detection radius for player detection
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, detectionRadius);

		// Draw a sphere for the damage radius
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, damageRadius);

	}

	#endregion

	#region States

	/// <summary>
	/// Handles the waiting state of the enemy.
	/// The enemy waits for a specified duration before changing to the roaming state.
	/// </summary>
	private void Wait() {
		if (!isWaiting || waitCoroutine == null) {
			playerPosition = Vector2.zero; // Reset the player position when waiting

			// Cancel any existing wait coroutine before starting a new one
			if (waitCoroutine != null) {
				StopCoroutine(waitCoroutine);
				waitCoroutine = null;
			}

			isWaiting = true;
			waitCoroutine = StartCoroutine(WaitCoroutine());

			if (debugMode) {
				Debug.Log("Started new wait coroutine");
			}
		}
	}

	/// <summary>
	/// Handles the chasing state of the enemy.
	/// The enemy chases the player if they are within the detection radius.
	/// If the player is hit, the enemy returns to the waiting state.
	/// </summary>
	private void ChasePlayer() {
		// Play charge sound only once when entering chase state
		if (!hasPlayedChargeSound) {
			PlayChargeSFX();
			hasPlayedChargeSound = true;
			if (debugMode) {
				Debug.Log("Played ghost charge sound");
			}
		}

		if (playerPosition != Vector2.zero && isPlayerDetected) {
			targetPoint = playerPosition; // Set the target point to the player's position
			HandleMovement(chargeSpeed); // Move faster towards the player
		}
		else if (!isPlayerDetected) {
			// If player is no longer detected, go back to waiting state
			currentState = State.Waiting;
		}
	}

	/// <summary>
	/// Handles the roaming state of the enemy.
	/// The enemy moves to random points within the specified distance from the starting position.
	/// After reaching a point, it waits for a specified duration before moving to the next point.
	/// </summary>
	private void Roam() {
		if (!isRoaming) {
			// Pick a new target point when entering roaming state
			targetPoint = PickNextRoamPoint();
			isRoaming = true;
		}

		HandleMovement(moveSpeed);

		if (Vector2.Distance(transform.position, targetPoint) < 0.1f) {
			targetPoint = PickNextRoamPoint(); // Reset target point when reached
			currentState = State.Waiting;
			isRoaming = false; // Reset roaming flag when we reach the target
			isReturningToStart = false; // Reset returning to start flag, it is important to reset the flag after attacking the player
		}
	}

	#endregion

	#region Private Methods

	private Vector2 PickNextRoamPoint() {
		// Pick a new random point from the roamPoints array
		Vector2 nextPoint = roamPoints[UnityEngine.Random.Range(0, roamPoints.Length)];
		while (nextPoint == targetPoint) {
			nextPoint = roamPoints[UnityEngine.Random.Range(0, roamPoints.Length)];
		}
		return nextPoint; // Return the new target point
	}

	private void HandlePlayerDetection() {
		// damage the player if they are within the damage radius and knock them back.
		Collider2D[] dcolliders = Physics2D.OverlapCircleAll(transform.position, damageRadius, playerLayer);
		if (currentState == State.Chasing) {
			foreach (Collider2D collider in dcolliders) {
				if (currentState == State.Chasing)
					collider.GetComponent<Player>().Knockback(knockbackDuration, knockbackDirection, transform.position);

				// After knocking back the player, return to starting position
				targetPoint = startingPosition;
				currentState = State.Roaming;
				isPlayerDetected = false;
				isReturningToStart = true; // Set the flag to indicate returning to start

				if (debugMode) {
					Debug.Log("Knocked back player, returning to starting position");
				}

				return;
			}
		}

		// check if the player is within the detection radius, if so, set the target point to the players position and change the state to chasing.
		isPlayerDetected = false;
		if (!isReturningToStart) {
			Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayer);
			foreach (Collider2D collider in colliders) {
				if (collider.CompareTag("Player")) {
					playerPosition = collider.transform.position;
					isPlayerDetected = true;
					currentState = State.Chasing;
					return;
				}
			}
			return;
		}


		// If no player is detected and we were previously chasing, go back to waiting
		if (!isPlayerDetected && isChasing) {
			playerPosition = Vector2.zero;
			currentState = State.Waiting;
		}
	}

	private IEnumerator WaitCoroutine() {
		if (debugMode) {
			Debug.Log($"Wait started, will wait for {waitDuration} seconds");
		}

		yield return new WaitForSeconds(waitDuration);

		if (debugMode) {
			Debug.Log("Wait completed, transitioning to Roaming state");
		}

		currentState = State.Roaming;
		isWaiting = false;
		waitCoroutine = null; // Reset the coroutine to null, clearing the reference when it's done
	}

	private void HandleMovement(float moveSpeed) {
		// Move the enemy towards the target point and flip its sprite based on the direction of movement.
		Vector2 direction = (targetPoint - (Vector2)transform.position).normalized;
		transform.position = Vector2.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);

		if (direction.x > 0) {
			facingDirection = 1; // facing right
		}
		else if (direction.x < 0) {
			facingDirection = -1; // facing left
		}
		// Flip the sprite based on the facing direction
		if (facingDirection == 1) {
			transform.localScale = new Vector3(-1, 1, 1); // facing right
		}
		else if (facingDirection == -1) {
			transform.localScale = new Vector3(1, 1, 1); // facing left
		}
	}

	private void PlayChargeSFX() {
		// play a sfx once the player is detected.
		if (AudioManager.Instance != null) {
			AudioManager.Instance.PlaySFX("SFX_GhostCharge");
		}
	}

	#endregion


	// These methods are used in the animator, modify at your own peril.
	#region Animation Methods

	private void MakeInvisible() {
		// Set the sprite renderer to be invisible and disable the collider
		spriteRenderer.enabled = false;
		col.enabled = false;
	}

	private void MakeVisible() {
		// Set the sprite renderer to be visible and enable the collider
		spriteRenderer.enabled = true;
		col.enabled = true;
	}
	#endregion

}
