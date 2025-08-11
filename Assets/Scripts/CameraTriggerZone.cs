using UnityEngine;

public class CameraTriggerZone : MonoBehaviour
{
    // Delegate and static event to notify the camera to stop following.
    public delegate void CameraStopFollow();
    public static event CameraStopFollow OnCameraStopFollow;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            // When the ball enters the trigger, notify the camera.
            OnCameraStopFollow?.Invoke();
        }
    }
}