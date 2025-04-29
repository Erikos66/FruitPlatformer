using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour {

    // Singleton instance
    public static CameraManager Instance { get; private set; }


    private CinemachineCamera CineCamera;
    private CinemachineImpulseSource ImpulseSource;

    [SerializeField] private Vector2 impantForce = new(0.75f, 0.75f);

    private void Awake() {
        // Find the virtual camera in the scene if it exists
        CineCamera = FindFirstObjectByType<CinemachineCamera>();
        ImpulseSource = FindFirstObjectByType<CinemachineImpulseSource>();

        // Singleton setup
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
        }
        else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Set the camera's target to follow a specific transform
    /// </summary>
    public void SetTargetToFollow(Transform target) {
        if (CineCamera == null) {
            // Try to find the virtual camera again
            CineCamera = FindFirstObjectByType<CinemachineCamera>();

            if (CineCamera == null) {
                Debug.LogError("No Cinemachine Camera found in the scene!");
                return;
            }
        }

        CineCamera.Follow = target;
    }

    internal void ShakeCamera() {
        if (ImpulseSource == null) {
            // Try to find the impulse source again
            ImpulseSource = FindFirstObjectByType<CinemachineImpulseSource>();

            if (ImpulseSource == null) {
                Debug.LogError("No Cinemachine Impulse Source found in the scene!");
                return;
            }
        }

        ImpulseSource.GenerateImpulse(impantForce);
    }
}
