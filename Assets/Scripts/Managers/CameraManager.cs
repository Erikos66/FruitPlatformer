using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour {
    private CinemachineCamera virtualCamera;

    private void Start() {
        // Find the virtual camera in the scene if it exists
        virtualCamera = FindFirstObjectByType<CinemachineCamera>();
    }

    /// <summary>
    /// Set the camera's target to follow a specific transform
    /// </summary>
    public void SetTargetToFollow(Transform target) {
        if (virtualCamera == null) {
            // Try to find the virtual camera again
            virtualCamera = FindFirstObjectByType<CinemachineCamera>();

            if (virtualCamera == null) {
                Debug.LogError("No Cinemachine Camera found in the scene!");
                return;
            }
        }

        virtualCamera.Follow = target;
    }

    /// <summary>
    /// Change camera settings for specific scenarios (e.g., boss fights, cutscenes)
    /// </summary>
    public void SetCameraSettings(float orthographicSize, float dampingX, float dampingY) {
        if (virtualCamera == null) return;

        virtualCamera.Lens.OrthographicSize = orthographicSize;

        var composer = virtualCamera.GetComponent<CinemachinePositionComposer>();
        if (composer != null) {
            composer.Damping.x = dampingX;
            composer.Damping.y = dampingY;
        }
    }

    /// <summary>
    /// Reset camera to default settings
    /// </summary>
    public void ResetCameraSettings() {
        if (virtualCamera == null) return;

        // Set default values - adjust these as needed
        virtualCamera.Lens.OrthographicSize = 5f;

        var composer = virtualCamera.GetComponent<CinemachinePositionComposer>();
        if (composer != null) {
            composer.Damping.x = 1f;
            composer.Damping.y = 1f;
        }
    }

    /// <summary>
    /// Shake the camera with specified intensity and duration
    /// </summary>
    public void ShakeCamera(float intensity, float duration) {
        StartCoroutine(ShakeCameraRoutine(intensity, duration));
    }

    private IEnumerator ShakeCameraRoutine(float intensity, float duration) {
        if (virtualCamera == null) yield break;

        // Get or add noise component
        var noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise == null) yield break;

        // Set noise parameters
        noise.AmplitudeGain = intensity;

        // Wait for specified duration
        yield return new WaitForSeconds(duration);

        // Reset noise
        noise.AmplitudeGain = 0f;
    }
}
