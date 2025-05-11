using System.Collections;
using UnityEngine;

public class BreakableBox : MonoBehaviour, IDamageable {
	[Header("Box Properties")]
	[SerializeField] private bool debugMode = false;
	[SerializeField] private int hitsToBreak = 1;
	private int currentHits = 0;
	private bool isBreaking = false;

	[Header("Break Effect")]
	[SerializeField] private Sprite[] brokenPieceSprites;
	[SerializeField] private float explosionForce = 5f;
	[SerializeField] private float torqueForce = 2f;
	[SerializeField] private float pieceLifetime = 2f;

	[Header("Components")]
	private Animator anim;
	private BoxCollider2D col;
	private SpriteRenderer sr;

	private void Awake() {
		anim = GetComponent<Animator>();
		col = GetComponent<BoxCollider2D>();
		sr = GetComponent<SpriteRenderer>();

		if (anim == null) {
			Debug.LogError("Animator component not found on " + gameObject.name);
		}

		if (col == null) {
			Debug.LogError("BoxCollider2D component not found on " + gameObject.name);
		}

		if (sr == null) {
			Debug.LogError("SpriteRenderer component not found on " + gameObject.name);
		}
	}

	public void Die() {

		if (isBreaking) return;

		currentHits++;

		if (debugMode) {
			Debug.Log($"{gameObject.name}: Taking damage. Hit {currentHits}/{hitsToBreak}");
		}

		// Play hit animation if it exists
		if (anim != null) {
			anim.SetTrigger("hit");
		}

		// Play hit sound if AudioManager exists
		if (AudioManager.Instance != null) {
			AudioManager.Instance.PlaySFX("SFX_BoxHit");
		}

		if (currentHits >= hitsToBreak) {
			isBreaking = true;

			if (debugMode) {
				Debug.Log($"{gameObject.name}: Breaking box and spawning pieces");
			}

			// Play break sound if AudioManager exists
			if (AudioManager.Instance != null) {
				AudioManager.Instance.PlaySFX("SFX_BoxBreak");
			}

			// Play break animation if it exists
			if (anim != null) {
				anim.SetTrigger("hit");  // Changed from "hit" to "break" for proper animation trigger
			}

			// Disable collider immediately
			col.enabled = false;

			// Create broken pieces
			SpawnBrokenPieces();

			// Hide the original sprite
			sr.enabled = false;

			// Destroy the object after a delay
			Destroy(gameObject, pieceLifetime);

		}


	}

	private void SpawnBrokenPieces() {
		if (brokenPieceSprites == null || brokenPieceSprites.Length == 0) {
			Debug.LogWarning("No broken piece sprites assigned to " + gameObject.name);
			return;
		}

		if (debugMode) {
			Debug.Log($"{gameObject.name}: Spawning {brokenPieceSprites.Length} broken pieces");
		}

		// Vector2 directions to launch pieces in 4 directions
		Vector2[] launchDirections = new Vector2[]
		{
			new Vector2(-1, 1),  // Top-left
            new Vector2(1, 1),   // Top-right
            new Vector2(-1, -1), // Bottom-left
            new Vector2(1, -1)   // Bottom-right
        };            // Spawn each piece with physics
		for (int i = 0; i < brokenPieceSprites.Length && i < launchDirections.Length; i++) {
			// Create a new GameObject for the piece
			GameObject piece = new GameObject("BrokenPiece_" + i);
			piece.transform.position = transform.position;
			piece.layer = LayerMask.NameToLayer("Disabled");

			if (debugMode) {
				Debug.Log($"{gameObject.name}: Creating piece {i} with sprite {brokenPieceSprites[i].name} and direction {launchDirections[i]}");
			}

			// Add SpriteRenderer and set the sprite
			SpriteRenderer renderer = piece.AddComponent<SpriteRenderer>();
			renderer.sprite = brokenPieceSprites[i];
			renderer.sortingOrder = sr.sortingOrder;

			// Add physics components
			Rigidbody2D pieceRB = piece.AddComponent<Rigidbody2D>();
			pieceRB.gravityScale = 1f; // Set gravity scale for pieces

			// Apply forces
			Vector2 direction = launchDirections[i].normalized;
			pieceRB.AddForce(direction * explosionForce, ForceMode2D.Impulse);

			// Add rotation
			float randomTorque = Random.Range(-torqueForce, torqueForce);
			pieceRB.AddTorque(randomTorque, ForceMode2D.Impulse);

			// Destroy after lifetime
			Destroy(piece, pieceLifetime);
		}
	}
}
