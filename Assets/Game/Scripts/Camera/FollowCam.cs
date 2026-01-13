using UnityEngine;

namespace WindRises.Camera
{
    /// <summary>
    /// Caméra de suivi fluide pour avion
    /// La cible et la caméra sont indépendantes (pas de relation parent-enfant)
    /// La caméra suit la cible avec un léger lerp pour un mouvement fluide
    /// </summary>
    public class FollowCam : MonoBehaviour
    {
        [Header("Cible")]
        [Tooltip("Transform de l'avion à suivre")]
        public Transform target;

        [Header("Lissage")]
        [Tooltip("Temps de lissage pour la position (plus bas = plus réactif)")]
        [Range(0.01f, 0.5f)]
        public float positionSmoothTime = 0.08f;

        [Tooltip("Temps de lissage pour la rotation (plus bas = plus réactif)")]
        [Range(0.01f, 0.5f)]
        public float rotationSmoothTime = 0.08f;

        [Header("Offset")]
        [Tooltip("Offset local par rapport à la cible (x=droite, y=haut, z=avant)")]
        public Vector3 localOffset = new Vector3(0f, 2f, -10f);

        private Vector3 _positionVelocity;

        void LateUpdate()
        {
            if (target == null)
                return;

            UpdateCameraPosition();
            UpdateCameraRotation();
        }

        void UpdateCameraPosition()
        {
            // Direction horizontale de l'avion (ignore pitch et roll)
            Vector3 flatForward = target.forward;
            flatForward.y = 0f;
            flatForward.Normalize();

            // Si l'avion est en piqué/montée verticale, utiliser sa direction quand même
            if (flatForward.sqrMagnitude < 0.001f)
                flatForward = target.forward;

            Vector3 flatRight = Vector3.Cross(Vector3.up, flatForward).normalized;

            // Position cible : offset basé sur la direction horizontale uniquement
            Vector3 targetPosition = target.position
                + flatForward * localOffset.z
                + Vector3.up * localOffset.y
                + flatRight * localOffset.x;

            // Lerp fluide vers la position cible
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _positionVelocity,
                positionSmoothTime
            );
        }

        void UpdateCameraRotation()
        {
            // Rotation cible : regarder vers l'avion
            Vector3 lookDirection = target.position - transform.position;

            if (lookDirection.sqrMagnitude < 0.001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

            // Lerp fluide vers la rotation cible
            float t = 1f - Mathf.Exp(-Time.deltaTime / rotationSmoothTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
        }
    }
}
