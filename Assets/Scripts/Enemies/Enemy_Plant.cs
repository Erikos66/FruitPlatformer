using UnityEngine;

public class Enemy_Plant : Enemy_Base {

    protected override void Update() {
        base.Update();

        if (DetectedPlayer()) {
            Attack();
        }
    }

    private void Attack() {
        anim.SetTrigger("onAttack");
    }
}
