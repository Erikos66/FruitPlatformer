using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Trap_MovingPlatform : MonoBehaviour {
	#region Variables
	[Header("Components")]
	private Rigidbody2D _rb; // Rigidbody reference
	private BoxCollider2D _col; // Collider reference
	private Animator _anim; // Animator reference

	[Header("Platform Settings")]
	[SerializeField] private bool _debugMode = false; // Debug mode
	[SerializeField] private bool _isActive = true; // Is platform active
	[SerializeField] private bool _grey = false; // Grey color toggle
	[SerializeField] private float _speed = 1f; // Platform speed
	[SerializeField] private float _waitTime = 1f; // Wait time at waypoint

	private List<Transform> _waypoints; // List of waypoints
	private Vector2 _targetPosition; // Current target position
	private int _currentWaypointIndex = 0; // Current waypoint index
	private bool _isWaiting = false; // Is platform waiting
	private Transform _attachedPlayer; // Player attached to platform
	private Vector2 _previousPosition; // Previous platform position
	private Vector2 _platformDelta; // Delta movement
	#endregion

	#region Unity Methods
	private void Awake() {
		_rb = GetComponent<Rigidbody2D>();
		_col = GetComponent<BoxCollider2D>();
		_anim = GetComponent<Animator>();

		_rb.interpolation = RigidbodyInterpolation2D.Interpolate;
		_rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		_rb.bodyType = RigidbodyType2D.Kinematic;

		_anim.SetLayerWeight(1, _grey ? 1 : 0);

		_waypoints = new();
		Transform parentObject = transform.parent;
		foreach (Transform child in parentObject) {
			if (child.name.Contains("Waypoint"))
				_waypoints.Add(child);
		}
		_previousPosition = _rb.position;
	}

	private void Start() {
		if (_debugMode) {
			if (_waypoints.Count == 0)
				Debug.LogWarning("No waypoints found for the moving platform. Please add waypoints as children of the platform.");
			else
				Debug.Log("Waypoints found: " + string.Join(", ", _waypoints.Select(w => w.name)));
		}
	}

	private void Update() {
		if (_isActive && !_isWaiting) {
			MovePlatform();
			_anim.SetBool("isActive", _isActive);
		}
	}

	private void LateUpdate() {
		_platformDelta = _rb.position - _previousPosition;
		if (_attachedPlayer != null) {
			Rigidbody2D playerRb = _attachedPlayer.GetComponent<Rigidbody2D>();
			if (playerRb != null)
				playerRb.position += _platformDelta;
		}
		_previousPosition = _rb.position;
	}
	#endregion

	#region Private Methods
	private void MovePlatform() {
		if (_waypoints.Count == 0 || _waypoints[_currentWaypointIndex] == null) return;
		_targetPosition = _waypoints[_currentWaypointIndex].position;
		_rb.MovePosition(Vector2.MoveTowards(_rb.position, _targetPosition, _speed * Time.deltaTime));
		if (Vector2.Distance(_rb.position, _targetPosition) < 0.1f) {
			int reachedWaypointIndex = _currentWaypointIndex;
			_currentWaypointIndex++;
			if (_currentWaypointIndex >= _waypoints.Count) {
				_waypoints.Reverse();
				_currentWaypointIndex = 0;
			}
			_isWaiting = true;
			StartCoroutine(WaitAtWaypoint(reachedWaypointIndex));
		}
	}
	#endregion

	#region Unity Events
	private void OnCollisionEnter2D(Collision2D collision) {
		if (collision.collider.CompareTag("Player"))
			_attachedPlayer = collision.collider.transform;
	}

	private void OnCollisionExit2D(Collision2D collision) {
		if (collision.collider.CompareTag("Player"))
			_attachedPlayer = null;
	}
	#endregion

	#region Coroutines
	private IEnumerator WaitAtWaypoint(int waypointIndex) {
		if (_debugMode && waypointIndex >= 0 && waypointIndex < _waypoints.Count)
			Debug.Log("Waiting at waypoint: " + _waypoints[waypointIndex].name);
		yield return new WaitForSeconds(_waitTime);
		_isWaiting = false;
	}
	#endregion
}
