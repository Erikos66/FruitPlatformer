using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    public int fruitsCollected;

    [SerializeField] public bool randomFruitsAllowed;

    [Header("Player")]
    public Player player;
    [SerializeField] public GameObject playerPrefab;
    [SerializeField] public GameObject currentSpawnPoint;
    [SerializeField] public GameObject startPoint;
    [SerializeField] public float RespawnDelay = 1f;
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

    public void RespawnPlayer() => StartCoroutine(RespawnPlayerCoroutine());
}
