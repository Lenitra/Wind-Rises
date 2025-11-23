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
            HandleRotation();
            ApplyMovement();
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
                currentSpeed += acceleration * throttle * Time.fixedDeltaTime;
                currentSpeed = Mathf.Clamp(currentSpeed, planeData.minSpeed, planeData.maxSpeed);
            }
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
