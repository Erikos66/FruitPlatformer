using UnityEngine;

public class Trap_Spikedball : MonoBehaviour {
    [SerializeField] private Rigidbody2D spikerb;
    [SerializeField] private float pushForce;

    private void Awake() {
        spikerb = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        Vector2 pushVector = new(pushForce, 0);
        spikerb.AddForce(pushVector, ForceMode2D.Impulse);
    }
}
