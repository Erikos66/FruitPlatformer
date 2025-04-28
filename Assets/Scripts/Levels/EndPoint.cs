using UnityEngine;

public class EndPoint : MonoBehaviour {

    private Animator anim;

    void Awake() {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Player player = collision.GetComponent<Player>();
        if (player) {
            // Play level finish sound
            AudioManager.Instance.PlaySFX("SFX_Finish");

            anim.SetTrigger("activate");
            Debug.Log("Level complete!");

            // Explicitly stop the timer when player reaches the end point
            GameManager.instance.timerManager.StopLevelTimer();

            // Mark the level as finished and handle progression
            GameManager.instance.levelManager.LevelFinished();
        }
    }
}
