using UnityEngine;

public class CameraController : MonoBehaviour
{
    // SERIALIZED FIELDS
    // These are used to configure the script from the Unity editor
    [SerializeField] private Transform targetToFollow;
    [SerializeField] private Transform lookAtTarget;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float distance = 3.5f;

    // PROPERTIES
    // The `IsTrigger` property manages the camera's state
    // and provides a clear public interface
    public bool IsTrigger { get; private set; } = false;

    // CALLBACK METHODS
    // Used to subscribe and unsubscribe from events in a clean way
    private void OnEnable()
    {
        // Subscribes to the event that stops the camera from following the target.
        CameraTriggerZone.OnCameraStopFollow += OnCameraStopFollow;
        // Subscribes to the event that resumes the camera following the target.
        BallCollisionHandler.OnShotEnded += OnShotEnded;
    }

    private void OnDisable()
    {
        // Unsubscribes from the event to prevent memory leaks.
        CameraTriggerZone.OnCameraStopFollow -= OnCameraStopFollow;
        // Unsubscribes from the event to prevent memory leaks.
        BallCollisionHandler.OnShotEnded -= OnShotEnded;
    }

    // UPDATE LOGIC
    // The logic is executed in `LateUpdate` to avoid jitter
    // when the camera follows a moving object
    private void LateUpdate()
    {
        // If the camera is not in trigger mode, it updates its position to follow the target.
        if (!IsTrigger)
        {
            UpdateCameraPosition();
        }

        // The camera always looks at the target.
        LookAtTarget();
    }

    // PRIVATE SUPPORT METHODS
    private void UpdateCameraPosition()
    {
        // Calculates the direction vector from the lookAtTarget to the targetToFollow.
        Vector3 direction = (targetToFollow.position - lookAtTarget.position).normalized;

        // Calculates the desired camera position with an offset based on distance.
        Vector3 desiredPosition = targetToFollow.position + direction * distance;

        // Keeps the camera's y-position stable by setting it to the target's y-position.
        desiredPosition.y = targetToFollow.position.y;

        // Smoothly moves the camera towards the desired position.
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
    }

    private void LookAtTarget()
    {
        // Makes the camera's forward direction point towards the lookAtTarget.
        transform.LookAt(lookAtTarget);
    }

    // EVENT HANDLING METHODS
    private void OnCameraStopFollow()
    {
        // Sets the IsTrigger flag to true to stop the camera from following.
        IsTrigger = true;
    }

    private void OnShotEnded()
    {
        // Sets the IsTrigger flag to false to resume the camera following.
        IsTrigger = false;
    }
}