using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

public class Trap_MovingPlatform : MonoBehaviour {
	#region Variables

	[Header("Components")]
	private Rigidbody2D rb; // Rigidbody2D for the platform
	private BoxCollider2D col; // BoxCollider2D for the platform (optional, can be used for collision detection)
	private Animator anim; // Animator for the platform (optional, can be used for visual effects)

	[Header("Platform Settings")]
	[SerializeField] private bool debugMode = false; // Enable debug mode to see debug messages in the console
	[SerializeField] private bool isActive = true; // Is the platform active?
	[SerializeField] private bool grey = false; // Is the platform brown? (used for visual effects)
	[SerializeField] private float speed = 1f; // Speed of the platform
	[SerializeField] private float waitTime = 1f; // Time to wait at each waypoint


	private List<Transform> waypoints; // List of waypoints for the platform to move between
	private Vector2 targetPosition; // Target position for the platform to move towards
	private int currentWaypointIndex = 0; // Index of the current waypoint
	private bool isWaiting = false; // Is the platform waiting at a waypoint?

	// Keep track of attached player and previous position for properly moving the player
	private Transform attachedPlayer = null;
	private Vector2 previousPosition;
	private Vector2 positionDelta; // Store the position delta to apply to the player

	#endregion

	#region Unity Methods

	private void Awake() {
		// Get components if they are not assigned in the inspector
		if (rb == null) rb = GetComponent<Rigidbody2D>();
		if (col == null) col = GetComponent<BoxCollider2D>();
		if (anim == null) anim = GetComponent<Animator>();

		// Configure rigidbody for proper platform movement
		rb.interpolation = RigidbodyInterpolation2D.Interpolate;
		rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		rb.bodyType = RigidbodyType2D.Kinematic; // Set the body type to Kinematic for platform movement

		anim.SetLayerWeight(1, grey ? 1 : 0); // Set the layer weight for the animator based on the brown variable

		waypoints = new(); // Initialize the waypoints list
		Transform parentObject = transform.parent; // Get the parent object of the platform

		foreach (Transform child in parentObject) {
			if (child.name.Contains("Waypoint")) { // Check if the child has the "Waypoint" tag
				waypoints.Add(child); // Add the waypoint to the list
			}
		}
	}

	private void Start() {
		if (debugMode) {
			if (waypoints.Count == 0) {
				Debug.LogWarning("No waypoints found for the moving platform. Please add waypoints as children of the platform.");
			}
			else {
				Debug.Log("Waypoints found: " + string.Join(", ", waypoints.Select(w => w.name)));
			}
		}

		// Store initial position to calculate delta movement
		previousPosition = rb.position;
	}

	private void Update() {
		if (isActive && !isWaiting) {

			// Move the platform towards the target position
			MovePlatform();

			anim.SetBool("isActive", isActive); // Set the animator parameter based on the platform's velocity
		}
	}

	private void FixedUpdate() {
		// Calculate position delta in FixedUpdate to sync with physics
		positionDelta = rb.position - previousPosition;
		previousPosition = rb.position;
	}

	private void LateUpdate() {
		// If we have an attached player, update their position based on platform movement
		if (attachedPlayer != null && positionDelta.magnitude > 0) {
			Rigidbody2D playerRb = attachedPlayer.GetComponent<Rigidbody2D>();
			Player playerScript = attachedPlayer.GetComponent<Player>();

			if (playerRb != null) {
				// Only move the player with the platform if they're grounded
				// This prevents affecting the player while they're jumping or falling
				bool isPlayerGrounded = playerScript != null && IsPlayerGrounded(playerScript);

				if (isPlayerGrounded) {
					// Apply the platform's movement to the player
					playerRb.position += positionDelta;

					if (debugMode) {
						Debug.Log($"Moving player with platform. Delta: {positionDelta}");
					}
				}
			}
		}
	}

	#endregion

	#region Custom Methods

	private bool IsPlayerGrounded(Player player) {
		// Use the player's own isGrounded state if available
		// We can access this directly since it's needed for our platform mechanics
		return !player.isAirborne;
	}

	private void MovePlatform() {
		if (waypoints.Count == 0) return; // no waypoints to move towards

		if (waypoints[currentWaypointIndex] == null) return; // skip if the waypoint is null

		// move towards the first waypoint, then the second, and so on
		targetPosition = waypoints[currentWaypointIndex].position;
		rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, speed * Time.deltaTime));

		if (Vector2.Distance(rb.position, targetPosition) < 0.1f) {
			// Store the current waypoint index for debugging
			int reachedWaypointIndex = currentWaypointIndex;

			// Update index for next waypoint
			currentWaypointIndex++;

			// Check if we've reached the end of the waypoints
			if (currentWaypointIndex >= waypoints.Count) {
				waypoints.Reverse(); // reverse the direction of the platform
				currentWaypointIndex = 0; // reset the index to the first waypoint
			}

			isWaiting = true; // set waiting to true when the platform reaches a waypoint
			StartCoroutine(WaitAtWaypoint(reachedWaypointIndex)); // pass the reached waypoint index to the coroutine
		}
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		// Check if the collision is with the player
		if (collision.gameObject.CompareTag("Player")) {
			// Make sure player is on top of the platform (not hitting from sides/bottom)
			foreach (ContactPoint2D contact in collision.contacts) {
				if (contact.normal.y < -0.5f) {
					// Player is standing on the platform
					attachedPlayer = collision.transform;
					// debug message for player on platform
					if (debugMode) Debug.Log("Player is on the platform: " + collision.gameObject.name);
					break;
				}
			}
		}
	}

	private void OnCollisionExit2D(Collision2D collision) {
		// Check if the collision is with the player
		if (collision.gameObject.CompareTag("Player")) {
			// debug message for player leaving platform
			if (debugMode) Debug.Log("Player left the platform: " + collision.gameObject.name);
			if (attachedPlayer == collision.transform) {
				attachedPlayer = null;
			}
		}
	}

	private IEnumerator WaitAtWaypoint(int waypointIndex) {
		if (debugMode && waypointIndex >= 0 && waypointIndex < waypoints.Count) {
			Debug.Log("Waiting at waypoint: " + waypoints[waypointIndex].name);
		}
		yield return new WaitForSeconds(waitTime); // wait for the specified time
		isWaiting = false; // set waiting to false when the wait is over
	}

	#endregion
}
