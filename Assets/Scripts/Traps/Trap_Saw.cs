using UnityEngine;
using System.Collections;

public class SawTrap : MonoBehaviour {
	[SerializeField] private float moveSpeed = 3f;
	[SerializeField] private Transform[] waypoints;

	private int currentWaypointIndex = 0;
	private Animator anim;
	private bool isWaiting = false;
	private float waitTime = 1f;

	private void Awake() {
		anim = GetComponent<Animator>();
	}

	private void Start() {
		transform.position = waypoints[currentWaypointIndex].position;
		EnableSaw();
	}

	private void Update() {
		MoveTrap();
	}

	private void MoveTrap() {
		if (transform.position == waypoints[currentWaypointIndex].position && !isWaiting) {
			StartCoroutine(WaitAtWaypoint());
		}
		if (!isWaiting) {
			transform.position = Vector2.MoveTowards(
				transform.position,
				waypoints[currentWaypointIndex].position,
				moveSpeed * Time.deltaTime
			);
		}
	}

	private IEnumerator WaitAtWaypoint() {
		isWaiting = true;
		yield return new WaitForSeconds(waitTime);
		currentWaypointIndex++;
		if (currentWaypointIndex >= waypoints.Length) {
			System.Array.Reverse(waypoints);
			currentWaypointIndex = 0;
		}
		isWaiting = false;
	}

	public void EnableSaw() {
		anim.SetBool("isSawActive", true);
	}

	public void DisableSaw() {
		anim.SetBool("isSawActive", false);
	}
}
