using UnityEngine;
using UnityEngine.InputSystem;

namespace WindRises.Flight
{
    /// <summary>
    /// Gère les entrées clavier et manette pour l'avion
    /// Utilise le nouveau Input System de Unity
    /// </summary>
    public class PlaneInput : MonoBehaviour
    {
        [Header("Input Settings")]
        [Tooltip("Configuration de l'avion (vitesses, inertie, etc.)")]
        [SerializeField] private PlaneData planeData;

        [SerializeField] private bool invertYAxis = false;
        [SerializeField] private float deadzone = 0.1f;

        // Sorties (lues par FlightController)
        public Vector2 MoveInput { get; private set; }  // x = roulis, y = tangage
        public float Throttle { get; private set; }     // -1 à 1
        public float Yaw { get; private set; }          // -1 à 1
        public bool RollbackRequested { get; private set; }  // True pendant 1 frame quand R est pressé

        // Valeurs lissées progressivement (pour inertie)
        private Vector2 currentMoveInput;
        private float currentThrottle;

        void Awake()
        {
            if (planeData == null)
            {
                Debug.LogWarning("PlaneInput: PlaneData not assigned! Input inertia will be disabled. Please assign PlaneData in the Inspector.");
            }
        }

        void Update()
        {
            ReadInput();
        }

        void ReadInput()
        {
            // Lire les inputs cibles (instantanés)
            float targetPitch = GetVerticalAxis();
            float targetRoll = GetHorizontalAxis();

            if (invertYAxis)
                targetPitch = -targetPitch;

            Vector2 targetMoveInput = new Vector2(targetRoll, targetPitch);
            float targetThrottle = GetThrottleAxis();

            // Appliquer l'inertie (lissage progressif)
            if (planeData != null && planeData.inputSmoothTime > 0f)
            {
                float smoothSpeed = 1f / planeData.inputSmoothTime;
                float t = Mathf.Clamp01(smoothSpeed * Time.deltaTime);

                currentMoveInput = Vector2.Lerp(currentMoveInput, targetMoveInput, t);
                currentThrottle = Mathf.Lerp(currentThrottle, targetThrottle, t);
            }
            else
            {
                // Pas d'inertie (comportement original)
                currentMoveInput = targetMoveInput;
                currentThrottle = targetThrottle;
            }

            // Assigner aux propriétés publiques
            MoveInput = currentMoveInput;
            Throttle = currentThrottle;

            // Yaw et Rollback restent instantanés (pas d'inertie nécessaire)
            Yaw = GetYawAxis();
            RollbackRequested = GetRollbackInput();
        }

        float GetHorizontalAxis()
        {
            float value = 0f;

            // Clavier (inversé pour correspondre aux attentes)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    value -= 1f;  // A = roulis vers la gauche (positif)
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    value += 1f;  // D = roulis vers la droite (négatif)
            }

            // Gamepad
            if (Gamepad.current != null)
            {
                float gamepadValue = Gamepad.current.leftStick.x.ReadValue();
                if (Mathf.Abs(gamepadValue) > deadzone)
                    value = gamepadValue;
            }

            return Mathf.Abs(value) > deadzone ? value : 0f;
        }

        float GetVerticalAxis()
        {
            float value = 0f;

            // Clavier
            if (Keyboard.current != null)
            {
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                    value -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                    value += 1f;
            }

            // Gamepad (stick gauche Y)
            if (Gamepad.current != null)
            {
                float gamepadValue = Gamepad.current.leftStick.y.ReadValue();
                if (Mathf.Abs(gamepadValue) > deadzone)
                    value = gamepadValue;
            }

            return Mathf.Abs(value) > deadzone ? value : 0f;
        }

        float GetThrottleAxis()
        {
            float value = 0f;

            // Clavier - Espace (accélération) / Shift (freinage)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.isPressed)
                    value += 1f;
                if (Keyboard.current.leftShiftKey.isPressed)
                    value -= 1f;
            }

            // Gamepad - R2 (accélération) / L2 (freinage)
            if (Gamepad.current != null)
            {
                float rightTrigger = Gamepad.current.rightTrigger.ReadValue();
                float leftTrigger = Gamepad.current.leftTrigger.ReadValue();

                // R2 = accélération (+), L2 = freinage (-)
                value = rightTrigger - leftTrigger;
            }

            return Mathf.Abs(value) > deadzone ? value : 0f;
        }

        float GetYawAxis()
        {
            float value = 0f;

            // Clavier uniquement - Q / E pour yaw manuel
            if (Keyboard.current != null)
            {
                if (Keyboard.current.qKey.isPressed)
                    value -= 1f;
                if (Keyboard.current.eKey.isPressed)
                    value += 1f;
            }

            return Mathf.Abs(value) > deadzone ? value : 0f;
        }

        bool GetRollbackInput()
        {
            // Clavier - R pour rollback
            if (Keyboard.current != null)
            {
                if (Keyboard.current.rKey.wasPressedThisFrame)
                    return true;
            }

            // Gamepad - Bouton Y (Triangle sur PlayStation) pour rollback
            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonNorth.wasPressedThisFrame)
                    return true;
            }

            return false;
        }
    }
}
