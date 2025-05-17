using System.Collections;
using UnityEngine;

public class Trap_Arrow : MonoBehaviour {
	#region Variables
	private Animator _anim; // Reference to the animator component
	private Collider2D _col; // Reference to the collider component
	[SerializeField] private float _pushForce = 30f; // Force to push the player
	[SerializeField] private float _disableDelay = 0.3f; // Delay before disabling
	[SerializeField] private float _rotationSpeed = 100f; // Arrow rotation speed
	[SerializeField] private bool _rotateRight; // Should rotate right
	[SerializeField] private float _rechargeTime = 2f; // Time to recharge

	private int _direction = 1; // Rotation direction
	private Vector3 _activeScale = new(1f, 1f, 1f); // Scale when active
	private Vector3 _inactiveScale = new(0.5f, 0.5f, 0.5f); // Scale when inactive
	private Vector3 _targetScale; // Target scale for lerp
	#endregion

	#region Unity Methods
	private void Awake() {
		_anim = GetComponent<Animator>();
		_col = GetComponent<Collider2D>();
	}

	private void Start() {
		_targetScale = _activeScale;
		EnableArrow();
	}

	private void Update() {
		HandleRotation();
		HandleScale();
	}
	#endregion

	#region Private Methods
	private void HandleScale() {
		transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, 0.01f);
	}

	private void HandleRotation() {
		_direction = _rotateRight ? -1 : 1;
		transform.Rotate(0, 0, _rotationSpeed * _direction * Time.deltaTime);
	}
	#endregion

	#region Unity Events
	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.TryGetComponent<Player>(out var player)) {
			_anim.SetTrigger("spring");
			player.PushPlayer(transform.up * _pushForce, _disableDelay);
			AudioManager.Instance.PlaySFX("SFX_Boost");
		}
	}
	#endregion

	#region Coroutines
	private IEnumerator ArrowRecharge(float time) {
		yield return new WaitForSeconds(time);
		_anim.SetTrigger("reload");
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Enables the arrow trap.
	/// </summary>
	public void EnableArrow() {
		_targetScale = _activeScale;
		_col.enabled = true;
	}

	/// <summary>
	/// Disables the arrow trap and starts recharge coroutine.
	/// </summary>
	public void DisableArrow() {
		_targetScale = _inactiveScale;
		_col.enabled = false;
		StartCoroutine(ArrowRecharge(_rechargeTime));
	}
	#endregion
}

