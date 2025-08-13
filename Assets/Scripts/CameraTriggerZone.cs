using UnityEngine;

public class CameraTriggerZone : MonoBehaviour
{
    // Delegate and static event to notify the camera to stop following.
    // Defines a delegate for the camera stop follow event.
    public delegate void CameraStopFollow();
    // Static event that other scripts can subscribe to.
    public static event CameraStopFollow OnCameraStopFollow;

    private void OnTriggerEnter(Collider other)
    {
        // Checks if the collider that entered the trigger is the "Ball".
        if (other.CompareTag("Ball"))
        {
            // Invokes the event to notify the camera to stop following.
            OnCameraStopFollow?.Invoke();
        }
    }
}