using System;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    public Player player;

    public int fruitsCollected;

    [SerializeField] public bool randomFruitsAllowed;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
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
}
