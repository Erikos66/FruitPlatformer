using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    [Header("Fruit Manager")]
    public int fruitsCollected;
    public bool randomFruitsAllowed;
    public int totalFruits;
    public Fruit[] allFruits;


    [Header("Player")]
    public Player player;
    public GameObject playerPrefab;
    public GameObject currentSpawnPoint;
    public GameObject startPoint;
    public float RespawnDelay = 1f;
    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        if (!currentSpawnPoint) {
            currentSpawnPoint = startPoint;
            Debug.LogWarning("StartPoint not set! Using default StartPoint.");
        }

    }

    public void Score() {
        Debug.Log("Score!");
    }

    public void AddFruit() {
        fruitsCollected++;
        Debug.Log("Fruit collected! Total: " + fruitsCollected);
    }

    public bool AllowedRandomFuits() => randomFruitsAllowed;

    private IEnumerator RespawnPlayerCoroutine() {
        Transform playerCurrentSpawnPoint = currentSpawnPoint.transform;

        yield return new WaitForSeconds(RespawnDelay);

        if (player) {
            player.Die();
        }

        if (currentSpawnPoint.TryGetComponent<StartPoint>(out var startPoint)) {
            startPoint.AnimateFlag();
            playerCurrentSpawnPoint = startPoint.respawnPoint;
        }

        GameObject newPlayer = Instantiate(playerPrefab, playerCurrentSpawnPoint.position, playerCurrentSpawnPoint.rotation);

        player = newPlayer.GetComponent<Player>();

    }

    private void Start() {
        CollectFruitInfo();
        Debug.Log("Total fruits: " + totalFruits);
    }

    private void CollectFruitInfo() {
        allFruits = new Fruit[0];
        allFruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);
        totalFruits = allFruits.Length;
    }

    public void RespawnPlayer() => StartCoroutine(RespawnPlayerCoroutine());

    public void LoadCredits() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("The_End");
    }
}
