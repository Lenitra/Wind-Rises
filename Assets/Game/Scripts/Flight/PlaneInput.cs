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
        [SerializeField] private bool invertYAxis = false;
        [SerializeField] private float deadzone = 0.1f;

        // Sorties (lues par FlightController)
        public Vector2 MoveInput { get; private set; }  // x = roulis, y = tangage
        public float Throttle { get; private set; }     // -1 à 1
        public float Yaw { get; private set; }          // -1 à 1

        void Update()
        {
            ReadInput();
        }

        void ReadInput()
        {
            // Pitch (tangage) et Roll (roulis) - WASD ou Flèches
            float pitch = GetVerticalAxis();
            float roll = GetHorizontalAxis();

            if (invertYAxis)
                pitch = -pitch;

            MoveInput = new Vector2(roll, pitch);

            // Throttle (gaz) - Espace / Shift OU gâchettes R2/L2
            Throttle = GetThrottleAxis();

            // Yaw (lacet) - Q / E
            Yaw = GetYawAxis();
        }

        float GetHorizontalAxis()
        {
            float value = 0f;

            // Clavier (inversé pour correspondre aux attentes)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    value += 1f;  // A = roulis vers la gauche (positif)
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    value -= 1f;  // D = roulis vers la droite (négatif)
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
    }
}
