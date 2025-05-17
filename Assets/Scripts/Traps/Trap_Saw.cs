using UnityEngine;
using System.Collections;

public class SawTrap : MonoBehaviour {
	#region Variables
	[SerializeField] private float _moveSpeed = 3f; // Saw movement speed
	[SerializeField] private Transform[] _waypoints; // Waypoints for saw

	private int _currentWaypointIndex = 0; // Current waypoint index
	private Animator _anim; // Animator reference
	private bool _isWaiting = false; // Is saw waiting
	private float _waitTime = 1f; // Wait time at waypoint
	#endregion

	#region Unity Methods
	private void Awake() {
		_anim = GetComponent<Animator>();
	}

	private void Start() {
		transform.position = _waypoints[_currentWaypointIndex].position;
		EnableSaw();
	}

	private void Update() {
		MoveTrap();
	}
	#endregion

	#region Private Methods
	private void MoveTrap() {
		if (transform.position == _waypoints[_currentWaypointIndex].position && !_isWaiting) {
			StartCoroutine(WaitAtWaypoint());
		}
		if (!_isWaiting) {
			transform.position = Vector2.MoveTowards(
				transform.position,
				_waypoints[_currentWaypointIndex].position,
				_moveSpeed * Time.deltaTime
			);
		}
	}
	#endregion

	#region Coroutines
	private IEnumerator WaitAtWaypoint() {
		_isWaiting = true;
		yield return new WaitForSeconds(_waitTime);
		_currentWaypointIndex++;
		if (_currentWaypointIndex >= _waypoints.Length) {
			System.Array.Reverse(_waypoints);
			_currentWaypointIndex = 0;
		}
		_isWaiting = false;
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Enables the saw trap.
	/// </summary>
	public void EnableSaw() {
		_anim.SetBool("isSawActive", true);
	}

	/// <summary>
	/// Disables the saw trap.
	/// </summary>
	public void DisableSaw() {
		_anim.SetBool("isSawActive", false);
	}
	#endregion
}
