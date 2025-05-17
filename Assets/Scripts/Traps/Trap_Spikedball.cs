using UnityEngine;

public class Trap_Spikedball : MonoBehaviour {
	#region Variables
	[SerializeField] private Rigidbody2D _spikeRb; // Rigidbody for spiked ball
	[SerializeField] private float _pushForce; // Initial push force
	#endregion

	#region Unity Methods
	private void Awake() {
		_spikeRb = GetComponent<Rigidbody2D>();
	}

	private void Start() {
		Vector2 pushVector = new(_pushForce, 0);
		_spikeRb.AddForce(pushVector, ForceMode2D.Impulse);
	}
	#endregion
}
