using System.Collections;
using UnityEngine;

public class Fruit : MonoBehaviour {
    private GameManager gameManager;
    private Animator anim;


    private void Awake() {
        anim = GetComponentInChildren<Animator>();
    }

    private void Start() {
        gameManager = GameManager.instance;

        if (gameManager.AllowedRandomFuits()) {
            SetRandomFruit();
        }
    }

    private void SetRandomFruit() {
        int random = Random.Range(0, 9);
        anim.SetFloat("fruitType", random);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        Player player = other.GetComponent<Player>();
        if (player != null) {
            gameManager.AddFruit();
            anim.SetTrigger("Collected");
            StartCoroutine(DestroyFruit());
            Destroy(GetComponent<Collider2D>());
        }
    }

    private IEnumerator DestroyFruit() {
        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
    }
}
