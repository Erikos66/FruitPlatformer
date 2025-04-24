using UnityEngine;

public class LevelController : MonoBehaviour {
    private void Start() {
        // Find the start point in the current level
        StartPoint startPoint = Object.FindAnyObjectByType<StartPoint>();

        if (startPoint != null) {
            // Set the GameManager's current spawn point and start point
            GameManager.instance.currentSpawnPoint = startPoint.gameObject;
            GameManager.instance.startPoint = startPoint.gameObject;

            Debug.Log("LevelController: StartPoint set for this level");
        }
        else {
            Debug.LogError("LevelController: No StartPoint found in this level!");
        }
    }
}
