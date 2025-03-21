using UnityEngine;

public class Enemy_Mushroom : Enemy_Base {

    protected override void Update() {
        base.Update();

        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        HandleCollision();

        if (isGrounded) {
            HandleMovement();
        }

        if (isWallDetected || !isGroundinFrontDetected) {
            if (!isGrounded) {
                return;
            }
            Flip();
            idleTimer = idleDuration;
        }
    }
}