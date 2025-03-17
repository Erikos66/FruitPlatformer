using UnityEngine;

public class CheckPoint : MonoBehaviour {
    private Animator anim;
    public Transform respawnPoint;
    private bool canBeActivated = true;
    private bool isActivated = false;

    private void Awake() {
        anim = GetComponent<Animator>();
    }

    private void ActiveCheckPoint() {
        if (isActivated)
            return;
        isActivated = true;
        canBeActivated = false;
        GameManager.instance.currentSpawnPoint = gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Player player = collision.GetComponent<Player>();
        if (player == null)
            return;
        if (canBeActivated) {
            ActiveCheckPoint();
            anim.SetBool("isActive", true);
        }
    }
}
