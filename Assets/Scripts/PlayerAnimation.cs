using UnityEngine;

public class PlayerAnimation : MonoBehaviour {

    private Player player;

    private void Awake() {
        player = GetComponentInParent<Player>();
    }

    public void RespawnFinished() {
        player.EnableControl();
    }
}
