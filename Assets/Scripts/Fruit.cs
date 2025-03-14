using UnityEngine;

public class Fruit : MonoBehaviour {
    private GameManager gameManager;
    private Animator anim;


    private void Awake() {
        anim = GetComponentInChildren<Animator>();
    }

    private void Start() {
        gameManager = GameManager.instance;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        Player player = other.GetComponent<Player>();
        if (player != null) {
            gameManager.AddFruit();
            Destroy(gameObject);
        }
    }
}
