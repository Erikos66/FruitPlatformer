using UnityEngine;

public class EndPoint : MonoBehaviour {

    private Animator anim;

    void Awake() {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Player player = collision.GetComponent<Player>();
        if (player) {
            anim.SetTrigger("activate");
            Debug.Log("Level complete!");

            // Explicitly stop the timer when player reaches the end point
            GameManager.instance.StopLevelTimer();

            GameManager.instance.LevelFinished();
        }
    }
}
