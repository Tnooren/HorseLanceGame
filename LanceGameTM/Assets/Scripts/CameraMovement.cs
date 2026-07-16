using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Position")]
    [SerializeField] private float followDistance = 5f;
    [SerializeField] private float cameraHeight = -10f;
    [SerializeField] private float followSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 3f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Follow with lag
        Vector3 desiredPosition =
            target.position
            - target.up * followDistance
            + Vector3.forward * cameraHeight;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            0.3f);
    }
}