using UnityEngine;

public class Enemy_Mushroom : Base_Enemy_Class {

	#region Variables
	// Mushroom has no specific variables beyond what it inherits from the base class
	#endregion

	#region Unity Methods

	/// <summary>
	/// Updates the mushroom enemy's state and behavior
	/// </summary>
	protected override void Update() {
		base.Update();

		if (_isDead) {
			return;
		}

		_anim.SetFloat("xVelocity", Mathf.Abs(_rb.linearVelocity.x));

		HandleCollision();
		if (_isGrounded) {
			HandleMovement();

			if (_isWallDetected || !_isGroundinFrontDetected) {
				Flip();
				_idleTimer = _idleDuration;
			}
		}
	}

	#endregion
}
