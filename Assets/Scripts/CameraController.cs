using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform targetToFollow;     // Object to follow
    [SerializeField] private Transform lookAtTarget;       // Object to look at (to orient the camera)
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float distance = 3.5f;
    public bool isTrigger = false;

    void OnEnable()
    {
        // Subscribe to events to control camera behavior
        CameraTriggerZone.OnCameraStopFollow += OnCameraStopFollow;
        BallCollisionHandler.OnShotEnded += OnShotEnded;
    }

    void OnDisable()
    {
        // Unsubscribe from events to prevent memory leaks
        CameraTriggerZone.OnCameraStopFollow -= OnCameraStopFollow;
        BallCollisionHandler.OnShotEnded -= OnShotEnded;
    }
    
    void LateUpdate()
    {
        if (!isTrigger)
        {
            // Calculate the camera's new position
            Vector3 direction = (lookAtTarget.position - targetToFollow.position).normalized;
            Vector3 offset = new Vector3(direction.x, 0, direction.z) - direction * distance;

            Vector3 pos = targetToFollow.position + offset;
            pos.y = targetToFollow.position.y;
            // Smoothly move the camera to the new position
            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * smoothSpeed);
        }

        // Always look at the target
        transform.LookAt(lookAtTarget);
    }

    private void OnCameraStopFollow()
    {
        isTrigger = true;
    }
    
    private void OnShotEnded()
    {
        isTrigger = false;
    }
}