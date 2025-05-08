using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Trap_MovingPlatform : MonoBehaviour {
	[Header("Components")]
	private Rigidbody2D rb;
	private BoxCollider2D col;
	private Animator anim;

	[Header("Platform Settings")]
	[SerializeField] private bool debugMode = false;
	[SerializeField] private bool isActive = true;
	[SerializeField] private bool grey = false;
	[SerializeField] private float speed = 1f;
	[SerializeField] private float waitTime = 1f;

	private List<Transform> waypoints;
	private Vector2 targetPosition;
	private int currentWaypointIndex = 0;
	private bool isWaiting = false;
	private Transform attachedPlayer;
	private Vector2 previousPosition;
	private Vector2 platformDelta;

	private void Awake() {
		rb = GetComponent<Rigidbody2D>();
		col = GetComponent<BoxCollider2D>();
		anim = GetComponent<Animator>();

		rb.interpolation = RigidbodyInterpolation2D.Interpolate;
		rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		rb.bodyType = RigidbodyType2D.Kinematic;

		anim.SetLayerWeight(1, grey ? 1 : 0);

		waypoints = new();
		Transform parentObject = transform.parent;
		foreach (Transform child in parentObject) {
			if (child.name.Contains("Waypoint"))
				waypoints.Add(child);
		}
		previousPosition = rb.position;
	}

	private void Start() {
		if (debugMode) {
			if (waypoints.Count == 0)
				Debug.LogWarning("No waypoints found for the moving platform. Please add waypoints as children of the platform.");
			else
				Debug.Log("Waypoints found: " + string.Join(", ", waypoints.Select(w => w.name)));
		}
	}

	private void Update() {
		if (isActive && !isWaiting) {
			MovePlatform();
			anim.SetBool("isActive", isActive);
		}
	}

	private void LateUpdate() {
		platformDelta = rb.position - previousPosition;
		if (attachedPlayer != null) {
			Rigidbody2D playerRb = attachedPlayer.GetComponent<Rigidbody2D>();
			if (playerRb != null)
				playerRb.position += platformDelta;
		}
		previousPosition = rb.position;
	}

	private void MovePlatform() {
		if (waypoints.Count == 0 || waypoints[currentWaypointIndex] == null) return;
		targetPosition = waypoints[currentWaypointIndex].position;
		rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, speed * Time.deltaTime));
		if (Vector2.Distance(rb.position, targetPosition) < 0.1f) {
			int reachedWaypointIndex = currentWaypointIndex;
			currentWaypointIndex++;
			if (currentWaypointIndex >= waypoints.Count) {
				waypoints.Reverse();
				currentWaypointIndex = 0;
			}
			isWaiting = true;
			StartCoroutine(WaitAtWaypoint(reachedWaypointIndex));
		}
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		if (collision.collider.CompareTag("Player"))
			attachedPlayer = collision.collider.transform;
	}

	private void OnCollisionExit2D(Collision2D collision) {
		if (collision.collider.CompareTag("Player"))
			attachedPlayer = null;
	}

	private IEnumerator WaitAtWaypoint(int waypointIndex) {
		if (debugMode && waypointIndex >= 0 && waypointIndex < waypoints.Count)
			Debug.Log("Waiting at waypoint: " + waypoints[waypointIndex].name);
		yield return new WaitForSeconds(waitTime);
		isWaiting = false;
	}
}
