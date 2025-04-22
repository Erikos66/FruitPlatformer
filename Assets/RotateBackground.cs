using UnityEngine;

public class RotateBackground : MonoBehaviour {

    public enum BackgroundType {
        Static,
        Rotating
    }

    public enum BackgroundColor {
        Blue,
        Brown,
        Grey,
        Green,
        Pink,
        Purple,
        Yellow
    }

    [SerializeField] private Vector2 rotationSpeed; // Speed of rotation
    [SerializeField] private BackgroundType backgroundType; // Type of background
    [SerializeField] private BackgroundColor backgroundColor; // Color of background
    [SerializeField] private Texture2D[] backgroundTextures; // Array of textures for the background

    private MeshRenderer mr;

    void Awake() {
        // Get the material of the background object
        mr = GetComponent<MeshRenderer>();

        // Set the background color based on the selected enum value
    }

    void Start() {
        UpdateBackgroundTexture();
    }

    void Update() {
        if (backgroundType == BackgroundType.Static) {
            return;
        }
        // Rotate the background based on the rotation speed
        float rotationX = rotationSpeed.x * Time.deltaTime;
        float rotationY = rotationSpeed.y * Time.deltaTime;
        mr.material.mainTextureOffset += new Vector2(rotationX, rotationY);
    }

    private void UpdateBackgroundTexture() {
        mr.material.mainTexture = backgroundTextures[(int)backgroundColor];
    }
}
