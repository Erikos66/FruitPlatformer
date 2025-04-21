using UnityEngine;

public class Enemy_Plant : Enemy_Base {
    [SerializeField] private float attackCooldown = 2f;
    private float attackCooldownTimer = 0f;

    protected override void Update() {
        base.Update();

        // Decrease cooldown timer
        if (attackCooldownTimer > 0) {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (DetectedPlayer() && attackCooldownTimer <= 0) {
            Attack();
        }
    }

    private void Attack() {
        anim.SetTrigger("onAttack");
        attackCooldownTimer = attackCooldown;
    }
}
