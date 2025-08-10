using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform targetToFollow;     // Oggetto da seguire
    [SerializeField] private Transform lookAtTarget;       // Oggetto da guardare (per orientare la camera)
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float distance = 3.5f;
    public bool isTrigger = false;

    void LateUpdate()
    {
        if (!isTrigger)
        {
            Vector3 direction = (lookAtTarget.position - targetToFollow.position).normalized;
            Vector3 offset = new Vector3(direction.x, 0, direction.z) - direction * distance;

            Vector3 pos = targetToFollow.position + offset;
            pos.y = targetToFollow.position.y;
            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * smoothSpeed);
            // transform.LookAt(lookAtTarget);
        }

        transform.LookAt(lookAtTarget);

    }

}