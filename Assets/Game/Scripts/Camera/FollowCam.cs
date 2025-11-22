using UnityEngine;

public class FollowCam : MonoBehaviour
{
    [Header("Cible à suivre")]
    public Transform target; // L’avion ou l’objet à suivre

    [Header("Position de la caméra")]
    public Vector3 offset = new Vector3(0f, 5f, -10f); // Décalage par rapport à la cible

    [Header("Suivi fluide (position)")]
    [Range(0.01f, 1f)]
    public float positionSmoothSpeed = 0.125f; // Vitesse de suivi de la position

    [Header("Regard fluide (rotation)")]
    [Range(0.01f, 1f)]
    public float rotationSmoothSpeed = 0.1f; // Vitesse de rotation vers la cible
    public bool lookAtTarget = true; // Si vrai, la caméra regarde la cible avec un léger délai

    void FixedUpdate()
    {
        if (target == null)
            return;

        // --- Position ---
        Vector3 desiredPosition = target.TransformPoint(offset);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, positionSmoothSpeed);
        transform.position = smoothedPosition;

        // --- Rotation fluide ---
        if (lookAtTarget)
        {
            // Direction de la cible
            Vector3 direction = (target.position - transform.position).normalized;

            // Rotation désirée vers la cible
            Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);

            // Interpolation fluide entre la rotation actuelle et la rotation désirée
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed);
        }
    }
}
