using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneControler : PlaneBase
{
    protected override void Awake()
    {
        base.Awake();

        // get the playerprefs to know which controller to use
        string inputMethod = PlayerPrefs.GetString("InputMethod", "Gamepad"); // Valeur par défaut: "Gamepad"
        Debug.Log($"InputMethod in PlayerPrefs: '{inputMethod}'");

        if (inputMethod != "Gamepad")
        {
            Debug.LogWarning($"PlaneControler disabled because InputMethod is '{inputMethod}' instead of 'Gamepad'");
            this.enabled = false;
            return;
        }
    }

    // ------------------------------------------------------------
    // Lecture des entrées manette
    // ------------------------------------------------------------
    protected override void HandleInput()
    {
        if (gamepad == null) return;

        // Trigger droit = accélérer, gauche = freiner/ralentir
        float accelTrigger = gamepad.rightTrigger.ReadValue();
        float brakeTrigger = gamepad.leftTrigger.ReadValue();

        // Stick gauche = pitch / roll
        Vector2 stick = gamepad.leftStick.ReadValue();
        float targetPitch = stick.y;
        float targetRoll = -stick.x;

        // Appliquer l'inertie aux contrôles (Lerp progressif)
        smoothPitch = Mathf.Lerp(smoothPitch, targetPitch, Time.fixedDeltaTime * controlInertia);
        smoothRoll = Mathf.Lerp(smoothRoll, targetRoll, Time.fixedDeltaTime * controlInertia);

        pitchInput = smoothPitch;
        rollInput = smoothRoll;

        // Gestion de la vitesse avec triggers séparés
        // Accélération : seulement si on ne dépasse pas la vitesse max OU si on est en dessous
        if (accelTrigger > 0f)
        {
            // Ne prendre en compte l'accélération que si on est en dessous de la vitesse max
            if (currentSpeed < maxSpeed && canAccelerate)
            {
                currentSpeed += accelTrigger * acceleration * Time.fixedDeltaTime;
            }
            // Si on dépasse maxSpeed, on ignore simplement l'input d'accélération
            // (la gravité peut toujours augmenter la vitesse)
        }

        // Décélération : toujours appliquée avec la gâchette gauche
        if (brakeTrigger > 0f)
        {
            currentSpeed -= brakeTrigger * deceleration * Time.fixedDeltaTime;
        }

        // Note: Pas de clamp ici pour permettre à la gravité de dépasser MaxSpeed
        // Le clamp global est géré dans HandleGravityByIncline() avec maxSpeed * 1.5f
    }
}