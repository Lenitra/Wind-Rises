using UnityEngine;

namespace WindRises.Flight.VFX
{
    /// <summary>
    /// Configuration des trails visuels des ailes de l'avion.
    /// ScriptableObject permettant de créer différents profils de trails (classique, neon, etc.)
    /// </summary>
    [CreateAssetMenu(fileName = "WingTrailData", menuName = "Wind Rises/Wing Trail Data")]
    public class WingTrailData : ScriptableObject
    {
        [Header("Activation")]
        [Tooltip("Vitesse minimale (m/s) pour activer les trails")]
        public float activationSpeedThreshold = 15f;

        [Tooltip("Vitesse (m/s) en dessous de laquelle désactiver les trails (hysteresis pour éviter le flickering)")]
        public float deactivationSpeedThreshold = 12f;

        [Header("Apparence - Largeur")]
        [Tooltip("Largeur de base des trails à basse vitesse (mètres)")]
        public float baseWidth = 0.5f;

        [Tooltip("Largeur maximale des trails à haute vitesse (mètres)")]
        public float maxWidth = 1.2f;

        [Tooltip("Courbe de falloff de la largeur sur la durée de vie du trail (0=début, 1=fin)")]
        public AnimationCurve widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("Apparence - Durée")]
        [Tooltip("Durée de vie du trail à basse vitesse (secondes)")]
        public float baseTime = 1.0f;

        [Tooltip("Durée de vie du trail à haute vitesse (secondes)")]
        public float maxTime = 2.5f;

        [Header("Apparence - Couleur")]
        [Tooltip("Gradient de couleur basé sur la vitesse (0=lent, 1=rapide)")]
        public Gradient speedColorGradient = CreateDefaultGradient();

        [Header("Performance")]
        [Tooltip("Distance minimale entre deux vertices du trail (plus élevé = moins de vertices)")]
        public float minVertexDistance = 0.1f;

        private static Gradient CreateDefaultGradient()
        {
            Gradient gradient = new Gradient();

            // Cyan à basse vitesse, jaune moyen, rouge à haute vitesse
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(new Color(0f, 0.78f, 1f), 0f);    // Cyan
            colorKeys[1] = new GradientColorKey(new Color(1f, 0.78f, 0f), 0.5f);  // Jaune
            colorKeys[2] = new GradientColorKey(new Color(1f, 0.2f, 0.4f), 1f);   // Rouge/Rose

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);

            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }
    }
}
