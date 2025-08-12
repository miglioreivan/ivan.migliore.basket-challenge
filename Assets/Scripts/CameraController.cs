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
        CameraTriggerZone.OnCameraStopFollow += OnCameraStopFollow;
        BallCollisionHandler.OnShotEnded += OnShotEnded;
    }

    private void OnDisable()
    {
        CameraTriggerZone.OnCameraStopFollow -= OnCameraStopFollow;
        BallCollisionHandler.OnShotEnded -= OnShotEnded;
    }

    // UPDATE LOGIC
    // The logic is executed in `LateUpdate` to avoid jitter
    // when the camera follows a moving object
    private void LateUpdate()
    {
        // If we are not in trigger mode, we follow the target
        if (!IsTrigger)
        {
            UpdateCameraPosition();
        }

        // The camera always looks at the target
        LookAtTarget();
    }

    // PRIVATE SUPPORT METHODS
    private void UpdateCameraPosition()
    {
        // Calcola il vettore di direzione dal targetToFollow a lookAtTarget
        Vector3 direction = (targetToFollow.position - lookAtTarget.position).normalized;

        // Calcola la posizione desiderata della telecamera
        // L'offset è calcolato moltiplicando la direzione per la distanza desiderata
        Vector3 desiredPosition = targetToFollow.position + direction * distance;

        // Imposta la posizione y della telecamera per mantenerla stabile
        desiredPosition.y = targetToFollow.position.y;

        // Sposta la telecamera in modo fluido verso la posizione desiderata
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
    }

    private void LookAtTarget()
    {
        // The camera points towards the target
        transform.LookAt(lookAtTarget);
    }

    // EVENT HANDLING METHODS
    private void OnCameraStopFollow()
    {
        // When the event is called, the camera stops following
        IsTrigger = true;
    }

    private void OnShotEnded()
    {
        // When the event is called, the camera resumes following
        IsTrigger = false;
    }
}