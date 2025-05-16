using System.Collections;
using UnityEngine;

public class BreakableBox : MonoBehaviour, IDamageable {

	#region Variables

	[Header("Box Properties")]
	[SerializeField] private bool _debugMode = false;   // Enable debug logging
	[SerializeField] private int _hitsToBreak = 1;      // Number of hits required to break box
	private int _currentHits = 0;                       // Current hit count
	private bool _isBreaking = false;                   // Flag to prevent multiple break sequences

	[Header("Break Effect")]
	[SerializeField] private Sprite[] _brokenPieceSprites;  // Sprites for broken pieces
	[SerializeField] private float _explosionForce = 5f;    // Force to apply to broken pieces
	[SerializeField] private float _torqueForce = 2f;       // Rotation force for broken pieces
	[SerializeField] private float _pieceLifetime = 2f;     // How long broken pieces exist

	[Header("Components")]
	private Animator _anim;             // Animation controller
	private BoxCollider2D _col;         // Collider component
	private SpriteRenderer _sr;         // Sprite renderer component

	#endregion

	#region Unity Methods

	private void Awake() {
		_anim = GetComponent<Animator>();
		_col = GetComponent<BoxCollider2D>();
		_sr = GetComponent<SpriteRenderer>();

		if (_anim == null) {
			Debug.LogError("Animator component not found on " + gameObject.name);
		}

		if (_col == null) {
			Debug.LogError("BoxCollider2D component not found on " + gameObject.name);
		}

		if (_sr == null) {
			Debug.LogError("SpriteRenderer component not found on " + gameObject.name);
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Handles damage received and box destruction logic
	/// </summary>
	public void Die() {
		if (_isBreaking) return;

		_currentHits++;

		if (_debugMode) {
			Debug.Log($"{gameObject.name}: Taking damage. Hit {_currentHits}/{_hitsToBreak}");
		}

		// Play hit animation if it exists
		if (_anim != null) {
			_anim.SetTrigger("hit");
		}

		// Play hit sound if AudioManager exists
		if (AudioManager.Instance != null) {
			AudioManager.Instance.PlaySFX("SFX_BoxHit");
		}

		if (_currentHits >= _hitsToBreak) {
			_isBreaking = true;

			if (_debugMode) {
				Debug.Log($"{gameObject.name}: Breaking box and spawning pieces");
			}

			// Play break sound if AudioManager exists
			if (AudioManager.Instance != null) {
				AudioManager.Instance.PlaySFX("SFX_BoxBreak");
			}

			// Play break animation if it exists
			if (_anim != null) {
				_anim.SetTrigger("hit");  // Changed from "hit" to "break" for proper animation trigger
			}

			// Disable collider immediately
			_col.enabled = false;

			// Create broken pieces
			SpawnBrokenPieces();

			// Hide the original sprite
			_sr.enabled = false;

			// Destroy the object after a delay
			Destroy(gameObject, _pieceLifetime);
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Creates and launches broken pieces of the box
	/// </summary>
	private void SpawnBrokenPieces() {
		if (_brokenPieceSprites == null || _brokenPieceSprites.Length == 0) {
			Debug.LogWarning("No broken piece sprites assigned to " + gameObject.name);
			return;
		}

		if (_debugMode) {
			Debug.Log($"{gameObject.name}: Spawning {_brokenPieceSprites.Length} broken pieces");
		}

		// Vector2 directions to launch pieces in 4 directions
		Vector2[] launchDirections = new Vector2[]
		{
			new Vector2(-1, 1),   // Top-left
			new Vector2(1, 1),    // Top-right
			new Vector2(-1, -1),  // Bottom-left
			new Vector2(1, -1)    // Bottom-right
		};

		// Spawn each piece with physics
		for (int i = 0; i < _brokenPieceSprites.Length && i < launchDirections.Length; i++) {
			// Create a new GameObject for the piece
			GameObject piece = new GameObject("BrokenPiece_" + i);
			piece.transform.position = transform.position;
			piece.layer = LayerMask.NameToLayer("Disabled");

			if (_debugMode) {
				Debug.Log($"{gameObject.name}: Creating piece {i} with sprite {_brokenPieceSprites[i].name} and direction {launchDirections[i]}");
			}

			// Add SpriteRenderer and set the sprite
			SpriteRenderer renderer = piece.AddComponent<SpriteRenderer>();
			renderer.sprite = _brokenPieceSprites[i];
			renderer.sortingOrder = _sr.sortingOrder;

			// Add physics components
			Rigidbody2D pieceRB = piece.AddComponent<Rigidbody2D>();
			pieceRB.gravityScale = 1f; // Set gravity scale for pieces

			// Apply forces
			Vector2 direction = launchDirections[i].normalized;
			pieceRB.AddForce(direction * _explosionForce, ForceMode2D.Impulse);

			// Add rotation
			float randomTorque = Random.Range(-_torqueForce, _torqueForce);
			pieceRB.AddTorque(randomTorque, ForceMode2D.Impulse);

			// Destroy after lifetime
			Destroy(piece, _pieceLifetime);
		}
	}

	#endregion
}
