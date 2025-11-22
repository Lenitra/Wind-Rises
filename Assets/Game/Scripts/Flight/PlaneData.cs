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

        [Tooltip("Accélération (unités/s)")]
        public float acceleration = 3f;

        [Tooltip("Décélération (unités/s)")]
        public float deceleration = 7.5f;

        [Tooltip("Vitesse de décrochage (pique du nez si en dessous)")]
        public float stallSpeed = 15f;

        [Header("Contrôles")]
        [Tooltip("Vitesse de rotation en tangage (pitch) - degrés/s")]
        public float pitchSpeed = 80f;

        [Tooltip("Vitesse de rotation en roulis (roll) - degrés/s")]
        public float rollSpeed = 120f;

        [Tooltip("Inertie des contrôles (plus haut = plus réactif)")]
        public float controlInertia = 5f;

        [Header("Stabilisation")]
        [Tooltip("Force de stabilisation automatique du roulis")]
        public float rollStabilizationForce = 0.5f;

        [Tooltip("Influence du roulis sur le tangage (virage coordonné)")]
        public float rollToPitchInfluence = 7.5f;

        [Tooltip("Influence du roulis sur le lacet (virage coordonné)")]
        public float rollToYawInfluence = 10f;

        [Header("Physique")]
        [Tooltip("Influence de la gravité basée sur l'inclinaison")]
        public float gravityInfluence = 10f;

        [Tooltip("Altitude maximale (limite haute)")]
        public float maxAltitude = 100000f;

        [Header("Gestion Altitude Max")]
        [Tooltip("Taux de ralentissement à haute altitude")]
        public float altitudeSlowdownRate = 1f;

        [Tooltip("Facteur de décélération à haute altitude")]
        public float altitudeDecelerationFactor = 0.1f;

        [Tooltip("Force de piqué automatique à haute altitude")]
        public float altitudePitchDownFactor = 5f;

        [Tooltip("Facteur de stabilisation du roulis à haute altitude")]
        public float altitudeRollStabilizationFactor = 2f;
    }
}
