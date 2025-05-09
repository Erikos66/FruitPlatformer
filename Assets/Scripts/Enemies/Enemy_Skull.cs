using System.Collections.Generic;
using UnityEngine;

public class Enemy_Skull : Enemy_Flying_Base {
	private enum SkullState {
		Charge,
		Patrol
	}

	#region Variables

	[Header("Skull Specific")]
	[SerializeField] private LayerMask groundLayer; // Layer for ground detection
	[SerializeField] private bool debugMode = false; // Enable debug mode for testing
	[SerializeField] private float moveSpeed = 5f; // Speed of the skull
	[SerializeField] private float chargeSpeed = 10f; // Speed of the charge attack
	[SerializeField] private float patrolRadius = 5f; // Radius of the patrol area
	[SerializeField] private float wallDetectionDistance = 1f; // Distance for wall detection
	[SerializeField] private float damageRadius = 1f; // Radius for player damage
	[SerializeField] private Vector2 knockbackForce; // Force applied to the player when knocked back
	[SerializeField] private float chargeCooldown = 3f; // Cooldown time between charges in seconds

	private bool hasShield = true; // Indicates if the skull has a shield
	private SkullState currentState = SkullState.Patrol; // Current state of the skull
	private Vector2 patrolCenter; // Center position of the patrol area
	private Vector2 currentTarget; // Current target position for patrolling or charging
	private List<Vector2> patrolPoints; // Array of patrol points
	private int currentPatrolIndex = 0; // Index of the current patrol point
	private Vector2 chargeDirection; // Direction of the charge attack
	private float cooldownTimer = 0f; // Timer for the charge cooldown
	private bool canCharge = true; // Whether the skull can charge

	#endregion

	#region Unity Methods

	override protected void Awake() {

		base.Awake(); // Call the base class Awake method to initialize components

		col.enabled = false; // Disable the collider initially

		SetupPatrol(); // Setup patrol points and center position

	}

	private void Update() {
		if (isDead) return; // If the enemy is dead, skip the update

		// Handle the cooldown timer
		if (!canCharge) {
			cooldownTimer -= Time.deltaTime;
			if (cooldownTimer <= 0f) {
				canCharge = true;
			}
		}

		HitDetection(); // Check for wall collisions and change direction if needed

		HandleMovement(); // Handle the movement of the skull based on its state

		PlayerDetection(); // Check for player detection and change state if needed

		if (currentState == SkullState.Patrol) {
			Patrolling();
		}
		if (currentState == SkullState.Charge) {
			Charging();
		}
	}

	void OnDrawGizmos() {
		if (debugMode) {
			// Draw patrol area
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(patrolCenter, patrolRadius);

			// Draw wall detection radius around current position
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, wallDetectionDistance);

			// Draw player detection radius
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(transform.position, detectionRadius);

			// Draw damage radius
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(transform.position, damageRadius);

			// Draw patrol points
			if (patrolPoints != null) {
				Gizmos.color = Color.green;
				for (int i = 0; i < patrolPoints.Count; i++) {
					Gizmos.DrawSphere(patrolPoints[i], 0.2f);

					// Draw line to current target
					if (i == currentPatrolIndex && currentState == SkullState.Patrol) {
						Gizmos.color = Color.blue;
						Gizmos.DrawLine(transform.position, patrolPoints[i]);
						Gizmos.color = Color.green;
					}
				}
				// Draw charge direction when charging
				if (currentState == SkullState.Charge) {
					Gizmos.color = Color.red;
					Gizmos.DrawLine(transform.position, (Vector2)transform.position + chargeDirection * 3f);
				}
			}
		}
	}

	#endregion

	#region Custom Methods

	private void HandleMovement() {
		Vector2 direction;
		float currentSpeed;

		if (currentState == SkullState.Patrol) {
			// Move towards the current target position
			direction = (currentTarget - (Vector2)transform.position).normalized;
			currentSpeed = moveSpeed;
		}
		else { // Charge state
			   // Move in the charge direction
			direction = chargeDirection;
			currentSpeed = chargeSpeed;
		}

		// Apply velocity
		rb.linearVelocity = direction * currentSpeed;

		// Flip the sprite based on the movement direction (only for horizontal movement)
		if (direction.x < 0) {
			spriteRenderer.flipX = true;
		}
		else if (direction.x > 0) {
			spriteRenderer.flipX = false;
		}
	}

	private void SetupPatrol() {
		// the enemy will set its transform position as the center of the patrol
		patrolCenter = transform.position;

		// the enemy will create 5 patrol points from random points in the patrol radius that are not overlapping with the ground layer
		patrolPoints = new List<Vector2>();
		for (int i = 0; i < 5; i++) {
			Vector2 randomPoint = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * patrolRadius;

			// Check if the point is valid (not too close to walls/ground)
			if (IsValidPatrolPoint(randomPoint)) {
				patrolPoints.Add(randomPoint);
			}
			else {
				i--; // Try again
			}
		}

		// Set the first patrol point as the initial target
		if (patrolPoints.Count > 0) {
			currentTarget = patrolPoints[0];
		}
		else {
			// Fallback if we couldn't find any valid patrol points
			currentTarget = transform.position;
			Debug.LogWarning("Skull enemy couldn't find valid patrol points!");
		}
	}

	private void Patrolling() {
		// if the enemy is close to the target, switch to the next patrol point
		if (Vector2.Distance(transform.position, currentTarget) < 0.1f) {
			currentPatrolIndex++;
			if (currentPatrolIndex >= patrolPoints.Count) {
				currentPatrolIndex = 0;
			}
			currentTarget = patrolPoints[currentPatrolIndex];
		}

	}

	private void HitDetection() {
		// Create a circular raycast around the skull to detect ground collisions
		Vector2 currentPosition = transform.position;
		int rayCount = 8; // Number of raycasts in the circle
		float rayLength = wallDetectionDistance;

		Vector2 hitNormal = Vector2.zero;

		// Behavior differs based on state
		if (currentState == SkullState.Patrol) {
			// Cast rays in a circle around the skull
			for (int i = 0; i < rayCount; i++) {
				// Calculate the angle and direction for each ray
				float angle = i * (360f / rayCount);
				Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

				// Cast the ray and check for ground collision
				RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, rayLength, groundLayer);

				if (hit.collider != null) {
					hitNormal = hit.normal;

					if (debugMode) {
						Debug.DrawRay(currentPosition, direction * rayLength, Color.red, 0.1f);
					}

					// Check if we're currently moving toward a patrol point that's near a wall
					if (Vector2.Distance(currentTarget, hit.point) < wallDetectionDistance * 2) {
						ReplacePatrolPoint(currentPatrolIndex);
						break;
					}
				}
				else if (debugMode) {
					Debug.DrawRay(currentPosition, direction * rayLength, Color.green, 0.1f);
				}
			}
		}
		else if (currentState == SkullState.Charge) {
			// For charge state, we only check collision in the charge direction
			RaycastHit2D hit = Physics2D.Raycast(currentPosition, chargeDirection, rayLength, groundLayer);

			if (hit.collider != null) {
				if (debugMode) {
					Debug.DrawRay(currentPosition, chargeDirection * rayLength, Color.red, 0.1f);
				}

				// We hit a wall while charging
				HandleWallCollision();
			}
			else if (debugMode) {
				Debug.DrawRay(currentPosition, chargeDirection * rayLength, Color.green, 0.1f);
			}
		}
	}

	// Generate a new valid patrol point to replace an invalid one
	private void ReplacePatrolPoint(int index) {
		// Generate a new random point that doesn't collide with ground
		Vector2 newPoint;
		bool validPoint = false;
		int attempts = 0;
		const int maxAttempts = 10;

		do {
			// Generate a random point within the patrol radius
			newPoint = (Vector2)patrolCenter + UnityEngine.Random.insideUnitCircle * patrolRadius;

			// Check if the point is far enough from ground/walls
			validPoint = IsValidPatrolPoint(newPoint);
			attempts++;
		} while (!validPoint && attempts < maxAttempts);

		// If we found a valid point, update the patrol point
		if (validPoint) {
			patrolPoints[index] = newPoint;
			// If we were heading to this point, update the current target
			if (index == currentPatrolIndex) {
				currentTarget = newPoint;
			}
		}
		// If we couldn't find a valid point after several attempts, just move to the next point
		else if (patrolPoints.Count > 1) {
			currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
			currentTarget = patrolPoints[currentPatrolIndex];
		}
	}

	// Check if a potential patrol point is valid (not too close to ground/walls)
	private bool IsValidPatrolPoint(Vector2 point) {
		// Check with a slightly larger radius to ensure the skull won't get too close to walls
		float safetyBuffer = wallDetectionDistance * 1.5f;
		int rayCount = 8;

		for (int i = 0; i < rayCount; i++) {
			float angle = i * (360f / rayCount);
			Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

			RaycastHit2D hit = Physics2D.Raycast(point, direction, safetyBuffer, groundLayer);
			if (hit.collider != null) {
				return false; // Point is too close to a wall
			}
		}

		return true; // Point is valid
	}

	private void PlayerDetection() {
		// Skip detection if we're already charging or in cooldown
		if (currentState == SkullState.Charge || !canCharge) {
			return;
		}

		// Check for player within detection radius
		Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

		if (playerCollider != null) {
			// Player detected, start charge

			// play charge sfx
			AudioManager.Instance.PlaySFXOnce("SFX_SkullCharge");

			// Check if the player is within the damage radius
			StartCharge(playerCollider.transform.position);
		}
	}

	private void StartCharge(Vector2 playerPosition) {
		// Change state to charge
		currentState = SkullState.Charge;

		// Set charge direction (straight line towards player's current position)
		chargeDirection = (playerPosition - (Vector2)transform.position).normalized;

		// Store current target position for later use (when returning to patrol)
		currentTarget = playerPosition;
	}

	private void Charging() {

		// Check for collision with ground/walls
		Vector2 currentPosition = transform.position;
		RaycastHit2D groundHit = Physics2D.Raycast(currentPosition, chargeDirection, wallDetectionDistance, groundLayer);

		if (groundHit.collider != null) {
			// Hit a wall/ground
			HandleWallCollision();
			return;
		}

		// Check for player within damage radius
		Collider2D playerCollider = Physics2D.OverlapCircle(currentPosition, damageRadius, playerLayer);

		if (playerCollider != null) {
			// Hit the player
			HandlePlayerCollision(playerCollider.gameObject);
		}
	}

	private void HandleWallCollision() {
		// We hit a wall, lose shield or gain it
		hasShield = !hasShield;

		// Play wall hit sound effect
		AudioManager.Instance.PlaySFXOnce("SFX_SkullImpact");

		// Trigger shield break animation
		if (anim != null) {
			anim.SetTrigger("wallHit");
		}

		// enable/disable collider based on shield status
		col.enabled = (hasShield); 

		// Return to patrol state
		ReturnToPatrol();
	}

	private void HandlePlayerCollision(GameObject player) {
		// apply knockback to the player using the players knockback method
		if (player.TryGetComponent<Player>(out var playerScript)) {
			Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
			playerScript.Knockback(1f, knockbackForce, knockbackDirection);
		}

		// Return to patrol state
		ReturnToPatrol();
	}

	private void ReturnToPatrol() {
		// Change state back to patrol
		currentState = SkullState.Patrol;

		// Start cooldown
		canCharge = false;
		cooldownTimer = chargeCooldown;

		// Reset to closest patrol point
		FindClosestPatrolPoint();
	}

	private void FindClosestPatrolPoint() {
		// Find the closest patrol point to return to
		float closestDistance = float.MaxValue;
		int closestIndex = 0;

		for (int i = 0; i < patrolPoints.Count; i++) {
			float distance = Vector2.Distance(transform.position, patrolPoints[i]);
			if (distance < closestDistance) {
				closestDistance = distance;
				closestIndex = i;
			}
		}

		currentPatrolIndex = closestIndex;
		currentTarget = patrolPoints[currentPatrolIndex];
	}

	override public void Die() {
		if (hasShield) return; // If the skull has a shield, don't die
		base.Die(); // Call the base class Die method
	}
	#endregion

}
