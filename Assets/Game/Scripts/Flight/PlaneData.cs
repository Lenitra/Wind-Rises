using UnityEngine;

namespace WindRises.Flight
{
    /// <summary>
    /// Configuration des paramètres de vol de l'avion
    /// ScriptableObject pour créer différents types d'avions
    /// </summary>
    [CreateAssetMenu(fileName = "PlaneData", menuName = "Wind Rises/Plane Data")]
    public class PlaneData : ScriptableObject
    {
        [Header("Vitesse")]
        [Tooltip("Vitesse minimale de l'avion")]
        public float minSpeed = 5f;

        [Tooltip("Vitesse maximale de l'avion")]
        public float maxSpeed = 30f;

        [Tooltip("Accélération")]
        public float acceleration = 3f;

        [Tooltip("Décélération")]
        public float deceleration = 7.5f;


        [Header("Contrôles")]
        [Tooltip("Vitesse de rotation en tangage (pitch) - degrés/s")]
        public float pitchSpeed = 80f;

        [Tooltip("Vitesse de rotation en roulis (roll) - degrés/s")]
        public float rollSpeed = 120f;

        [Header("Stabilisation")]
        [Tooltip("Force de stabilisation automatique du roulis")]
        public float rollStabilizationForce = 0.5f;

        [Tooltip("Force de virage basée sur le roulis (plus élevé = virages plus serrés)")]
        public float rollTurnInfluence = 30f;

        [Header("Maniabilité")]
        [Tooltip("Facteur de réduction de la maniabilité à vitesse max (0 = pas de réduction, 1 = réduction maximale)")]
        [Range(0f, 1f)]
        public float speedManeuverabilityFactor = 0.5f;

        [Header("Physique")]
        [Tooltip("Amortissement linéaire (frottement de l'air)")]
        public float linearDrag = 0.5f;

        [Tooltip("Influence de la gravité sur l'avion")]
        public float gravityInfluence = 10f;

        [Tooltip("Intensité des turbulences atmosphériques")]
        public float turbulenceIntensity = 0f;

        [Tooltip("Fréquence des turbulences (vitesse de changement)")]
        public float turbulenceFrequency = 1f;
    }
}
