using UnityEngine;

public class Enemy_Mushroom : Enemy_Base {

    protected override void Update() {
        base.Update();

        if (isDead) {
            return;
        }

        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));

        HandleCollision();
        if (isGrounded) {
            HandleMovement();

            if (isWallDetected || !isGroundinFrontDetected) {
                Flip();
                idleTimer = idleDuration;
            }
        }
    }
}