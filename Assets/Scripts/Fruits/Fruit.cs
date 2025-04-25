using System.Collections;
using UnityEngine;

public class Fruit : MonoBehaviour {
    [SerializeField] private string fruitID; // Unique ID for this fruit
    private GameManager gameManager;
    private Animator anim;
    private bool hasBeenCollectedBefore = false;

    private void Awake() {
        anim = GetComponentInChildren<Animator>();

        // If no ID has been set in the inspector, generate one based on position
        if (string.IsNullOrEmpty(fruitID)) {
            fruitID = $"{gameObject.scene.name}_fruit_{transform.position.x}_{transform.position.y}";
        }
    }

    private void Start() {
        gameManager = GameManager.instance;

        if (gameManager.AllowedRandomFuits()) {
            SetRandomFruit();
        }

        // Check if this fruit has been collected before
        CheckIfAlreadyCollected();
    }

    private void CheckIfAlreadyCollected() {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string key = $"Fruit_{currentScene}_{fruitID}";

        // If this fruit was collected in a previous session, disable it
        if (PlayerPrefs.GetInt(key, 0) == 1) {
            hasBeenCollectedBefore = true;
            gameObject.SetActive(false);
        }
    }

    private void SetRandomFruit() {
        int random = Random.Range(0, 9);
        anim.SetFloat("fruitType", random);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        Player player = other.GetComponent<Player>();
        if (player != null && !hasBeenCollectedBefore) {
            gameManager.AddFruit();
            anim.SetTrigger("Collected");
            StartCoroutine(DestroyFruit());
            Destroy(GetComponent<Collider2D>());

            // Mark this specific fruit as collected
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string key = $"Fruit_{currentScene}_{fruitID}";
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }
    }

    private IEnumerator DestroyFruit() {
        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
    }
}
