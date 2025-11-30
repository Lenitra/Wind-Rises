using UnityEngine;
using WindRises.Flight;

namespace WindRises.Camera
{
    /// <summary>
    /// Caméra de suivi fluide pour avion
    /// Suit directement le transform de l'avion sans EventBus pour éviter les saccades
    /// Utilise SmoothDamp pour un mouvement stable et prévisible
    /// Gère spécialement le rollback avec interpolation fluide
    /// </summary>
    public class FollowCam : MonoBehaviour
    {
        [Header("Cible")]
        [Tooltip("Transform de l'avion à suivre")]
        public Transform target;

        [Header("Position")]
        [Tooltip("Distance derrière l'avion (en mètres)")]
        public float followDistance = 12f;

        [Tooltip("Hauteur au-dessus de l'avion (en mètres)")]
        public float followHeight = 3f;

        [Header("Lissage")]
        [Tooltip("Temps pour atteindre la position cible (0 = instantané, 0.1 = fluide)")]
        [Range(0f, 0.5f)]
        public float positionSmoothTime = 0.05f;

        [Tooltip("Temps pour atteindre la rotation cible (0 = instantané, 0.1 = fluide)")]
        [Range(0f, 0.5f)]
        public float rotationSmoothTime = 0.05f;

        [Header("Rollback")]
        [Tooltip("Temps de lissage de la caméra pendant un rollback (plus élevé = plus fluide)")]
        [Range(0f, 1f)]
        public float rollbackSmoothTime = 0.3f;

        [Tooltip("Distance de recul de la caméra pendant le rollback")]
        public float rollbackExtraDistance = 5f;

        [Header("Inclinaison")]
        [Tooltip("Intensité maximale de l'inclinaison de la caméra (0 = horizon fixe, 1 = suit complètement à 90°)")]
        [Range(0f, 1f)]
        public float bankingIntensity = 0.5f;

        // Variables internes pour SmoothDamp
        private Vector3 _positionVelocity;

        // Référence au FlightRecorder pour détecter le rollback
        private FlightRecorder _flightRecorder;

        // Mode de suivi actuel
        private bool _isInRollbackMode = false;

        private void Awake()
        {
            // Récupérer le FlightRecorder si disponible
            if (target != null)
            {
                _flightRecorder = target.GetComponent<FlightRecorder>();
            }
        }

        /// <summary>
        /// LateUpdate garantit que la caméra se met à jour après la physique de l'avion
        /// Combiné avec Rigidbody.interpolation sur l'avion, élimine toutes les saccades
        /// </summary>
        void LateUpdate()
        {
            if (target == null)
                return;

            // Vérifier si on est en mode rollback
            UpdateRollbackMode();

            UpdateCameraPosition();
            UpdateCameraRotation();
        }

        /// <summary>
        /// Vérifie et met à jour l'état du mode rollback
        /// </summary>
        void UpdateRollbackMode()
        {
            if (_flightRecorder != null)
            {
                _isInRollbackMode = _flightRecorder.IsPlayingRollback;
            }
            else
            {
                _isInRollbackMode = false;
            }
        }

        /// <summary>
        /// Met à jour la position de la caméra derrière et au-dessus de l'avion
        /// </summary>
        void UpdateCameraPosition()
        {
            // Calculer la distance effective (augmentée pendant le rollback)
            float effectiveDistance = followDistance;
            if (_isInRollbackMode)
            {
                effectiveDistance += rollbackExtraDistance;
            }

            // Calcul de la position cible dans l'espace local de l'avion
            Vector3 offset = -target.forward * effectiveDistance + target.up * followHeight;
            Vector3 targetPosition = target.position + offset;

            // Temps de lissage adapté au mode
            float effectiveSmoothTime = _isInRollbackMode ? rollbackSmoothTime : positionSmoothTime;

            // Application avec lissage (ou instantané si smoothTime = 0)
            if (effectiveSmoothTime > 0.001f)
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    targetPosition,
                    ref _positionVelocity,
                    effectiveSmoothTime
                );
            }
            else
            {
                transform.position = targetPosition;
            }
        }

        /// <summary>
        /// Met à jour la rotation de la caméra pour regarder l'avion
        /// Utilise un simple LookAt pendant le rollback pour plus de stabilité
        /// </summary>
        void UpdateCameraRotation()
        {
            // Direction vers l'avion
            Vector3 lookDirection = (target.position - transform.position).normalized;

            // Sécurité : éviter les calculs si trop proche
            if (lookDirection.sqrMagnitude < 0.001f)
                return;

            if (_isInRollbackMode)
            {
                // Mode rollback : rotation simple et stable vers l'avion
                UpdateRollbackRotation(lookDirection);
            }
            else
            {
                // Mode normal : rotation avec banking
                UpdateNormalRotation(lookDirection);
            }
        }

        /// <summary>
        /// Rotation pendant le rollback : simple LookAt lissé sans banking
        /// </summary>
        void UpdateRollbackRotation(Vector3 lookDirection)
        {
            // Toujours utiliser Vector3.up pour un horizon stable
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

            // Lerp fluide vers la rotation cible
            float t = 1f - Mathf.Exp(-Time.deltaTime / rollbackSmoothTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
        }

        /// <summary>
        /// Rotation en mode normal : avec banking et lissage standard
        /// </summary>
        void UpdateNormalRotation(Vector3 lookDirection)
        {
            // Calcul du vecteur "up" pour l'inclinaison dans les virages
            Vector3 upVector = CalculateBankedUpVector(lookDirection);

            // Rotation finale regardant l'avion avec le bon "up"
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, upVector);

            // Application avec lissage (ou instantané si smoothTime = 0)
            ApplyRotation(targetRotation);
        }

        /// <summary>
        /// Calcule le vecteur "up" de la caméra en fonction du roulis de l'avion
        /// Banking automatique : 100% à 90°, 0% à 0° et 180°
        /// </summary>
        Vector3 CalculateBankedUpVector(Vector3 lookDirection)
        {
            if (bankingIntensity > 0.01f)
            {
                // Angle de roulis de l'avion (-180 à 180)
                float rollAngle = GetRollAngle(target.rotation);

                // Calcul du facteur de banking basé sur l'angle
                // Maximum à ±90°, minimum à 0° et ±180°
                // Utilise sin pour avoir une courbe naturelle
                float absRoll = Mathf.Abs(rollAngle);
                float bankingFactor = Mathf.Sin(absRoll * Mathf.Deg2Rad);

                // Application de l'intensité et de l'angle
                float bankAngle = rollAngle * bankingFactor * bankingIntensity;

                // Rotation du vecteur up autour de la direction de regard
                return Quaternion.AngleAxis(bankAngle, lookDirection) * Vector3.up;
            }
            else
            {
                // Pas d'inclinaison : horizon fixe
                return Vector3.up;
            }
        }

        /// <summary>
        /// Applique la rotation cible avec lissage
        /// Utilise Slerp pour un lissage fluide qui préserve le banking
        /// </summary>
        void ApplyRotation(Quaternion targetRotation)
        {
            if (rotationSmoothTime > 0.001f)
            {
                // Lissage de la rotation complète avec Slerp
                // Préserve le banking calculé dans CalculateBankedUpVector
                float t = 1f - Mathf.Exp(-Time.deltaTime / rotationSmoothTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
            }
            else
            {
                // Rotation instantanée
                transform.rotation = targetRotation;
            }
        }

        /// <summary>
        /// Extrait l'angle de roulis (roll) d'une rotation
        /// Convertit de 0-360° à -180-180° pour faciliter les calculs
        /// </summary>
        float GetRollAngle(Quaternion rotation)
        {
            float roll = rotation.eulerAngles.z;

            // Conversion en plage -180 à 180
            if (roll > 180f)
                roll -= 360f;

            return roll;
        }

        // === DEBUG ===
        void OnDrawGizmos()
        {
            if (!Application.isPlaying || target == null)
                return;

            // Position de l'avion (rouge)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, 1f);

            // Ligne caméra -> avion (cyan en mode normal, magenta en rollback)
            Gizmos.color = _isInRollbackMode ? Color.magenta : Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);

            // Direction de regard de la caméra (jaune)
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * 8f);
        }

        void OnGUI()
        {
            if (target == null)
                return;

            GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 310, 150));
            GUI.Box(new Rect(0, 0, 310, 150), "");

            GUILayout.Label("=== CAMERA DEBUG ===");

            // Mode de la caméra
            if (_isInRollbackMode)
            {
                GUILayout.Label("MODE: ROLLBACK (stabilisé)");
            }
            else
            {
                GUILayout.Label("MODE: Normal");
            }

            GUILayout.Label($"Distance: {Vector3.Distance(transform.position, target.position):F1}m");
            GUILayout.Label($"Position Velocity: {_positionVelocity.magnitude:F2} m/s");

            Vector3 lookDir = (target.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, lookDir);
            GUILayout.Label($"Look Angle: {angle:F1}°");

            if (!_isInRollbackMode)
            {
                float rollAngle = GetRollAngle(target.rotation);
                float bankingFactor = Mathf.Sin(Mathf.Abs(rollAngle) * Mathf.Deg2Rad);
                float bankAngle = rollAngle * bankingFactor * bankingIntensity;
                GUILayout.Label($"Banking: {bankAngle:F1}° (roll: {rollAngle:F1}°)");
            }

            GUILayout.EndArea();
        }
    }
}
