using UnityEngine;
using WindRises.Core;

namespace WindRises.Flight
{
    /// <summary>
    /// Contrôleur d'avion arcade style GTA V
    /// Physique simple, intuitive et responsive
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlaneInput))]
    public class FlightController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PlaneData planeData;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        // Composants
        private Rigidbody _rb;
        private PlaneInput _input;

        // État
        private float _currentSpeed;
        private float _targetSpeed;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _input = GetComponent<PlaneInput>();

            _rb.useGravity = false;
            _rb.linearDamping = 0f;
            _rb.angularDamping = 3f;

            _currentSpeed = planeData.minSpeed;
            _targetSpeed = planeData.minSpeed;
        }

        void FixedUpdate()
        {
            HandleSpeed();
            HandleRotation();
            ApplyMovement();
            PublishEvents();
        }

        /// <summary>
        /// Gestion de la vitesse (comme GTA V - simple et direct)
        /// </summary>
        void HandleSpeed()
        {
            // Throttle modifie la vitesse cible
            if (_input.Throttle > 0)
            {
                _targetSpeed += planeData.acceleration * Time.fixedDeltaTime;
            }
            else if (_input.Throttle < 0)
            {
                _targetSpeed -= planeData.deceleration * Time.fixedDeltaTime;
            }

            // Clamp la vitesse cible
            _targetSpeed = Mathf.Clamp(_targetSpeed, planeData.minSpeed, planeData.maxSpeed);

            // Interpoler vers la vitesse cible
            _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, Time.fixedDeltaTime * 2f);

            // Effet de gravité basé sur l'inclinaison (monte = perd de la vitesse)
            float pitchAngle = Vector3.Dot(transform.forward, Vector3.up);
            float gravityEffect = -pitchAngle * planeData.gravityInfluence * Time.fixedDeltaTime;
            _currentSpeed += gravityEffect;

            // Clamp final
            _currentSpeed = Mathf.Clamp(_currentSpeed, planeData.minSpeed * 0.5f, planeData.maxSpeed * 1.5f);
        }

        /// <summary>
        /// Gestion de la rotation (système arcade GTA V)
        /// </summary>
        void HandleRotation()
        {
            Vector2 moveInput = _input.MoveInput;

            // === PITCH (Tangage) - W/S ===
            float pitchSpeed = moveInput.y * planeData.pitchSpeed * Time.fixedDeltaTime;

            // === ROLL (Roulis) - A/D ===
            float rollSpeed = -moveInput.x * planeData.rollSpeed * Time.fixedDeltaTime;

            // === YAW (Lacet) - Automatique basé sur le roulis ===
            // C'est LA CLÉ du système GTA V !
            float currentRoll = transform.eulerAngles.z;
            if (currentRoll > 180f) currentRoll -= 360f;

            // Calculer le facteur de virage (max à ±90°, min à 0°/180°)
            float absRoll = Mathf.Abs(currentRoll);
            float turnFactor;

            if (absRoll <= 90f)
                turnFactor = absRoll / 90f;
            else
                turnFactor = (180f - absRoll) / 90f;

            // Courbe progressive
            turnFactor = Mathf.Sin(turnFactor * Mathf.PI * 0.5f);

            // Yaw automatique - tourne dans la direction du roulis
            // Roulis gauche (négatif dans currentRoll) = Yaw gauche (positif)
            // Roulis droite (positif dans currentRoll) = Yaw droite (négatif)
            float yawSpeed = -Mathf.Sign(currentRoll) * turnFactor * planeData.rollToYawInfluence * Time.fixedDeltaTime;

            // Yaw manuel avec Q/E (optionnel)
            if (Mathf.Abs(_input.Yaw) > 0.1f)
            {
                yawSpeed += _input.Yaw * planeData.pitchSpeed * 0.5f * Time.fixedDeltaTime;
            }

            // Facteur de vitesse (plus rapide = plus réactif)
            float speedFactor = Mathf.Clamp01(_currentSpeed / planeData.maxSpeed);
            speedFactor = 0.5f + speedFactor * 0.5f; // 0.5 à 1.0

            pitchSpeed *= speedFactor;
            rollSpeed *= speedFactor;
            yawSpeed *= speedFactor;

            // === STABILISATION DU ROULIS ===
            // L'avion retourne doucement à plat si aucun input
            float rollCorrection = 0f;
            if (Mathf.Abs(moveInput.x) < 0.1f)
            {
                rollCorrection = -currentRoll * planeData.rollStabilizationForce * Time.fixedDeltaTime;
            }

            // === DÉCROCHAGE ===
            // Si trop lent, l'avion pique du nez
            if (_currentSpeed < planeData.stallSpeed)
            {
                float stallEffect = (planeData.stallSpeed - _currentSpeed) / planeData.stallSpeed;
                pitchSpeed -= stallEffect * 30f * Time.fixedDeltaTime;
            }

            // === APPLICATION DES ROTATIONS ===
            // GTA V style: Yaw en mondial, Pitch+Roll en local

            // 1. Pitch et Roll (contrôles locaux)
            Quaternion localRotation = Quaternion.Euler(pitchSpeed, 0f, rollSpeed + rollCorrection);
            _rb.MoveRotation(_rb.rotation * localRotation);

            // 2. Yaw (rotation mondiale autour de Vector3.up)
            Quaternion yawRotation = Quaternion.AngleAxis(yawSpeed, Vector3.up);
            _rb.MoveRotation(yawRotation * _rb.rotation);
        }

        /// <summary>
        /// Applique le mouvement vers l'avant
        /// </summary>
        void ApplyMovement()
        {
            _rb.linearVelocity = transform.forward * _currentSpeed;
        }

        /// <summary>
        /// Publie les événements
        /// </summary>
        void PublishEvents()
        {
            EventBus.Instance.Publish(new SpeedChanged
            {
                Speed = _currentSpeed * 6.66f,
                MaxSpeed = planeData.maxSpeed * 6.66f
            });

            EventBus.Instance.Publish(new AltitudeChanged
            {
                Altitude = transform.position.y
            });

            EventBus.Instance.Publish(new PositionUpdated
            {
                Position = transform.position,
                Rotation = transform.rotation
            });
        }

        void OnDrawGizmos()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            // Direction
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 10f);

            // Vélocité
            if (_rb != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + _rb.linearVelocity);
            }
        }

        void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 400, 300));
            GUILayout.Label("=== FLIGHT DEBUG (GTA V Style) ===");
            GUILayout.Label($"Vitesse: {(_currentSpeed * 6.66f):F0} km/h (cible: {(_targetSpeed * 6.66f):F0})");
            GUILayout.Label($"Max: {(planeData.maxSpeed * 6.66f):F0} km/h");
            GUILayout.Label($"Altitude: {transform.position.y:F1} m");
            GUILayout.Label($"Throttle: {_input.Throttle:F2}");
            GUILayout.Label($"Input - Pitch: {_input.MoveInput.y:F2} | Roll: {_input.MoveInput.x:F2}");

            float currentRoll = transform.eulerAngles.z;
            if (currentRoll > 180f) currentRoll -= 360f;
            GUILayout.Label($"Roll Angle: {currentRoll:F1}°");

            float absRoll = Mathf.Abs(currentRoll);
            float turnFactor;
            if (absRoll <= 90f)
                turnFactor = absRoll / 90f;
            else
                turnFactor = (180f - absRoll) / 90f;
            turnFactor = Mathf.Sin(turnFactor * Mathf.PI * 0.5f);

            string turnDir = "DROIT";
            if (Mathf.Abs(currentRoll) > 5f)
                turnDir = currentRoll < 0 ? "GAUCHE (A)" : "DROITE (D)";

            GUILayout.Label($"Turn Factor: {turnFactor:F2}");
            GUILayout.Label($"Direction: {turnDir}");

            if (_currentSpeed < planeData.stallSpeed)
            {
                GUILayout.Label("!!! DÉCROCHAGE !!!");
            }

            GUILayout.EndArea();
        }
    }
}
