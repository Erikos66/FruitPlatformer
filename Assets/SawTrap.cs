using UnityEngine;
using System.Collections;

public class SawTrap : MonoBehaviour {
    [SerializeField] private float moveSpeed = 3; // The speed at which the trap moves
    [SerializeField] private Transform[] waypoints; // The waypoints the trap moves between

    private int currentWaypointIndex = 0; // The index of the current waypoint
    private Animator anim;
    private bool isWaiting = false; // Added field
    private float waitTime = 1; // Added field

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

    /// <summary>
    /// Move the trap between the waypoints
    /// </summary>
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

    /// <summary>
    ///  Wait at the waypoint for 1 second
    /// </summary>
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

    /// <summary>
    /// Enable the saw animation
    /// </summary>
    public void EnableSaw() {
        anim.SetBool("isSawActive", true);
    }

    /// <summary>
    /// Disable the saw animation
    /// </summary>
    public void DisableSaw() {
        anim.SetBool("isSawActive", false);
    }
}
