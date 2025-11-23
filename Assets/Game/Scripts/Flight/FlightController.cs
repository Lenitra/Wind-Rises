using UnityEngine;

namespace WindRises.Flight
{
    /// <summary>
    /// Contrôleur de vol arcade basé sur un Rigidbody.
    /// Gère l'accélération, le pitch et le roll de l'avion.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(PlaneInput))]
    public class FlightController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PlaneData planeData;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        // Components
        private Rigidbody rb;
        private PlaneInput input;

        // État du vol
        private float currentSpeed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            input = GetComponent<PlaneInput>();

            // Éliminer les saccades de caméra
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.useGravity = false;

            // Vitesse initiale
            currentSpeed = planeData.minSpeed;
        }

        private void FixedUpdate()
        {
            HandleSpeed();
            HandleGravity();
            HandleDrag();
            HandleRotation();
            ApplyMovement();
        }

        private void OnGUI()
        {
            if (!showDebugInfo)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 310, 150));
            GUI.Box(new Rect(0, 0, 310, 150), "");

            GUILayout.Label("=== FLIGHT DEBUG ===");

            // Vitesse
            GUILayout.Label($"Speed: {currentSpeed:F1} m/s ({currentSpeed * 3.6f:F0} km/h)");

            // Throttle
            GUILayout.Label($"Throttle: {input.Throttle:F2}");

            // Roll
            float currentRoll = transform.eulerAngles.z;
            if (currentRoll > 180f) currentRoll -= 360f;
            GUILayout.Label($"Roll: {currentRoll:F1}°");

            // Pitch
            float currentPitch = transform.eulerAngles.x;
            if (currentPitch > 180f) currentPitch -= 360f;
            GUILayout.Label($"Pitch: {currentPitch:F1}°");

            // Yaw
            float currentYaw = transform.eulerAngles.y;
            GUILayout.Label($"Yaw: {currentYaw:F1}°");

            // Altitude
            GUILayout.Label($"Altitude: {transform.position.y:F1} m");

            GUILayout.EndArea();
        }

        /// <summary>
        /// Gère l'accélération et la décélération
        /// </summary>
        private void HandleSpeed()
        {
            float throttle = input.Throttle;

            if (throttle != 0f)
            {
                float acceleration = throttle > 0 ? planeData.acceleration : planeData.deceleration;
                float speedDelta = acceleration * throttle * Time.fixedDeltaTime;

                // Limiter uniquement la contribution du throttle, pas la vitesse totale
                if (throttle > 0 && currentSpeed >= planeData.maxSpeed)
                {
                    // Si déjà au-dessus de maxSpeed (grâce à la gravité), ne pas accélérer davantage
                    speedDelta = 0f;
                }

                currentSpeed += speedDelta;
            }
        }

        /// <summary>
        /// Applique la gravité en fonction de l'inclinaison (pitch)
        /// </summary>
        private void HandleGravity()
        {
            // Calcul de l'inclinaison : dot product entre forward et up
            // > 0 = montée, < 0 = descente, 0 = horizontal
            float incline = Vector3.Dot(transform.forward, Vector3.up);

            // Effet de gravité : ralentit en montée, accélère en descente
            float gravityEffect = -incline * planeData.gravityInfluence * Time.fixedDeltaTime;
            currentSpeed += gravityEffect;

            // La gravité ne peut pas faire descendre sous minSpeed, mais peut dépasser maxSpeed
            currentSpeed = Mathf.Max(currentSpeed, planeData.minSpeed);
        }

        /// <summary>
        /// Applique le frottement de l'air (linear drag)
        /// </summary>
        private void HandleDrag()
        {
            // Ratio de vitesse par rapport à la vitesse max (clamped à 100%)
            float speedRatio = Mathf.Min(currentSpeed / planeData.maxSpeed, 1f);

            // Drag proportionnel à la vitesse
            float dragForce = speedRatio * planeData.linearDrag * Time.fixedDeltaTime;

            // Application du drag (ralentissement)
            currentSpeed -= dragForce;

            // Ne pas descendre sous minSpeed à cause du drag
            currentSpeed = Mathf.Max(currentSpeed, planeData.minSpeed);
        }

        /// <summary>
        /// Gère le pitch, roll et yaw automatique basé sur l'inclinaison
        /// </summary>
        private void HandleRotation()
        {
            Vector2 moveInput = input.MoveInput;

            // Pitch local
            float pitch = moveInput.y * planeData.pitchSpeed * Time.fixedDeltaTime;

            // Roll local avec stabilisation automatique
            float rollInput = -moveInput.x * planeData.rollSpeed * Time.fixedDeltaTime;

            // Stabilisation du roll (retour automatique à l'horizontal)
            float currentRoll = transform.eulerAngles.z;
            if (currentRoll > 180f) currentRoll -= 360f;

            float rollCorrection = 0f;
            if (Mathf.Abs(moveInput.x) < 0.1f)
            {
                rollCorrection = -currentRoll * planeData.rollStabilizationForce * Time.fixedDeltaTime;
            }

            float finalRoll = rollInput + rollCorrection;

            // Rotation locale (pitch + roll)
            Quaternion localRotation = Quaternion.Euler(pitch, 0f, finalRoll);
            rb.MoveRotation(rb.rotation * localRotation);

            // Yaw automatique dans le référentiel MONDE basé sur l'angle de roll
            // Facteur proportionnel : max à ±90°, min à 0° et 180°
            float rollFactor = Mathf.Sin(Mathf.Abs(currentRoll) * Mathf.Deg2Rad);
            float yawFromRoll = -(currentRoll / 90f) * rollFactor * planeData.rollTurnInfluence * Time.fixedDeltaTime;

            // Rotation monde (yaw autour de l'axe Y global)
            Quaternion worldYawRotation = Quaternion.AngleAxis(yawFromRoll, Vector3.up);
            rb.MoveRotation(worldYawRotation * rb.rotation);
        }

        /// <summary>
        /// Applique le mouvement forward
        /// </summary>
        private void ApplyMovement()
        {
            rb.linearVelocity = transform.forward * currentSpeed;
        }
    }
}
