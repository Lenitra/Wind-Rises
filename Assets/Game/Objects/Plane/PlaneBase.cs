using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public abstract class PlaneBase : MonoBehaviour
{
    [Header("Vitesse")]
    [SerializeField] protected float acceleration = 3f;
    [SerializeField] protected float deceleration = 7.5f;
    [SerializeField] protected float maxSpeed = 30f;
    [SerializeField] protected float minSpeed = 5f;
    [SerializeField] protected float gravityInfluence = 10f;
    [SerializeField] protected float maxAltitude = 100000f;
    [SerializeField] protected float altitudeSlowdownRate = 1f;
    [SerializeField] protected float altitudeDecelerationFactor = 0.1f;
    [SerializeField] protected float altitudePitchDownFactor = 5f;
    [SerializeField] protected float altitudeRollStabilizationFactor = 2f;

    [Header("Contrôles")]
    [SerializeField] protected float pitchSpeed = 80f;
    [SerializeField] protected float rollSpeed = 120f;
    [SerializeField] protected float controlInertia = 5f;
    [SerializeField] protected float rollStabilizationForce = 0.5f;
    [SerializeField] protected float rollToPitchInfluence = 7.5f;
    [SerializeField] protected float rollToYawInfluence = 10f;
    [SerializeField] protected float stallSpeed = 15f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI infoText;

    protected Rigidbody rb;
    protected float currentSpeed;
    protected Gamepad gamepad;
    protected bool canAccelerate = true;

    protected float throttleInput;
    protected float pitchInput;
    protected float rollInput;
    protected float smoothPitch;
    protected float smoothRoll;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 2f;
        currentSpeed = minSpeed;
        gamepad = Gamepad.current;
    }

    protected virtual void FixedUpdate()
    {
        if (rb == null) return;

        HandleInput();
        HandleGravityByIncline();
        ApplyMovement();
        UpdateUI();
    }

    protected abstract void HandleInput();

    protected void HandleGravityByIncline()
    {
        float incline = Vector3.Dot(transform.forward, Vector3.up);
        float gravityEffect = -incline * gravityInfluence * Time.fixedDeltaTime;
        currentSpeed += gravityEffect;
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed * 1.5f);
    }

    protected void ApplyMovement()
    {
        bool aboveMaxAltitude = transform.position.y > maxAltitude;

        if (aboveMaxAltitude)
        {
            canAccelerate = false;
            float incline = Vector3.Dot(transform.forward, Vector3.up);

            if (incline > 0f)
            {
                float altitudeDifference = transform.position.y - maxAltitude;
                float slowdownForce = altitudeDifference * altitudeSlowdownRate * altitudeDecelerationFactor * Time.fixedDeltaTime;
                currentSpeed -= slowdownForce;
                currentSpeed = Mathf.Max(currentSpeed, minSpeed);
            }
        }
        else
        {
            canAccelerate = true;
        }

        float finalPitch = pitchInput * pitchSpeed * Time.fixedDeltaTime;

        if (currentSpeed < stallSpeed)
            finalPitch += gravityInfluence * Time.fixedDeltaTime;

        if (aboveMaxAltitude)
        {
            float altitudeDifference = transform.position.y - maxAltitude;
            float pitchDownForce = altitudeDifference * altitudePitchDownFactor * Time.fixedDeltaTime;
            finalPitch += pitchDownForce;
        }

        float currentRoll = transform.eulerAngles.z;
        if (currentRoll > 180f) currentRoll -= 360f;

        float rollCorrection = 0f;
        if (Mathf.Abs(rollInput) < 0.1f)
        {
            float rollStabilization = rollStabilizationForce;

            if (aboveMaxAltitude)
            {
                float altitudeDifference = transform.position.y - maxAltitude;
                rollStabilization += altitudeDifference * altitudeRollStabilizationFactor * Time.fixedDeltaTime;
            }

            rollCorrection = -currentRoll * rollStabilization * Time.fixedDeltaTime;
        }

        float rollRatio = Mathf.Clamp01(Mathf.Abs(currentRoll) / 90f);
        float rollInfluenceFactor = rollRatio * rollRatio;
        float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);
        float speedFactor = 1f + (speedRatio * 2f);
        float rollInfluence = rollInfluenceFactor * rollToPitchInfluence * speedFactor * Time.fixedDeltaTime;
        finalPitch -= rollInfluence;

        float finalRoll = (rollInput * rollSpeed * Time.fixedDeltaTime) + rollCorrection;

        Quaternion localRotation = Quaternion.Euler(finalPitch, 0f, finalRoll);
        rb.MoveRotation(rb.rotation * localRotation);

        float yawFromRoll = -(currentRoll / 90f) * rollToYawInfluence * speedFactor * Time.fixedDeltaTime;
        Quaternion worldYawRotation = Quaternion.AngleAxis(yawFromRoll, Vector3.up);
        rb.MoveRotation(worldYawRotation * rb.rotation);

        rb.linearVelocity = transform.forward * currentSpeed;
    }

    protected void UpdateUI()
    {
        if (infoText != null)
            infoText.text = $"Alt: {Mathf.RoundToInt(transform.position.y)}m\n{Mathf.RoundToInt(currentSpeed * 6.66f)}km/h";
    }
}