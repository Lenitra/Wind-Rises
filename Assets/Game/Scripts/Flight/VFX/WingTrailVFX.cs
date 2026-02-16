using UnityEngine;
using WindRises.Core;

namespace WindRises.Flight.VFX
{
    /// <summary>
    /// Gère les trails visuels aux extrémités des ailes de l'avion.
    /// S'abonne aux événements de position/vitesse via EventBus et met à jour dynamiquement
    /// les propriétés des TrailRenderers (couleur, largeur, durée) en fonction de la vitesse.
    /// </summary>
    public class WingTrailVFX : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Configuration des trails (thresholds, couleurs, largeurs, etc.)")]
        [SerializeField] private WingTrailData trailData;

        [Header("Trail Points")]
        [Tooltip("Transform du point de spawn du trail gauche (extrémité aile gauche)")]
        [SerializeField] private Transform leftWingPoint;

        [Tooltip("Transform du point de spawn du trail droit (extrémité aile droite)")]
        [SerializeField] private Transform rightWingPoint;

        [Header("Speed Reference (optional)")]
        [Tooltip("Vitesse minimale de l'avion (référence PlaneData)")]
        [SerializeField] private float minSpeed = 5f;

        [Tooltip("Vitesse maximale de l'avion (référence PlaneData)")]
        [SerializeField] private float maxSpeed = 30f;

        // Components
        private TrailRenderer leftTrail;
        private TrailRenderer rightTrail;

        // State
        private float currentSpeed;
        private bool isActive = false;

        private void Awake()
        {
            // Récupérer les TrailRenderer sur les wing points
            if (leftWingPoint != null)
                leftTrail = leftWingPoint.GetComponent<TrailRenderer>();

            if (rightWingPoint != null)
                rightTrail = rightWingPoint.GetComponent<TrailRenderer>();

            // Vérifications
            if (leftTrail == null || rightTrail == null)
            {
                Debug.LogError("WingTrailVFX: TrailRenderer components not found on wing points! Please add TrailRenderer to both LeftWingPoint and RightWingPoint.");
                enabled = false;
                return;
            }

            if (trailData == null)
            {
                Debug.LogError("WingTrailVFX: TrailData not assigned! Please assign a WingTrailData asset.");
                enabled = false;
                return;
            }

            // Initialiser les TrailRenderers
            InitializeTrailRenderers();
        }

        private void OnEnable()
        {
            // S'abonner aux événements de position/vitesse
            EventBus.Instance.Subscribe<PositionUpdated>(OnPositionUpdated);
        }

        private void OnDisable()
        {
            // Se désabonner des événements
            EventBus.Instance.Unsubscribe<PositionUpdated>(OnPositionUpdated);
        }

        private void Update()
        {
            // Mettre à jour l'état actif/inactif des trails
            UpdateTrailState();

            // Mettre à jour les propriétés dynamiques des trails
            UpdateTrailProperties();
        }

        /// <summary>
        /// Callback appelé quand l'avion publie une mise à jour de position/rotation
        /// </summary>
        private void OnPositionUpdated(PositionUpdated data)
        {
            // Calculer la vitesse depuis le vecteur mouvement (magnitude du vecteur vélocité)
            currentSpeed = data.mouvement.magnitude;
        }

        /// <summary>
        /// Configure les TrailRenderers avec les paramètres initiaux
        /// </summary>
        private void InitializeTrailRenderers()
        {
            ConfigureTrail(leftTrail);
            ConfigureTrail(rightTrail);
        }

        /// <summary>
        /// Configure un TrailRenderer avec les paramètres de base
        /// </summary>
        private void ConfigureTrail(TrailRenderer trail)
        {
            // Width curve (falloff de la largeur sur la durée de vie)
            trail.widthCurve = trailData.widthCurve;

            // Durée de vie initiale
            trail.time = trailData.baseTime;

            // Distance minimale entre vertices (performance)
            trail.minVertexDistance = trailData.minVertexDistance;

            // Désactiver l'émission au départ (activé dynamiquement selon vitesse)
            trail.emitting = false;

            // Alignement : face à la caméra pour meilleur rendu
            trail.alignment = LineAlignment.View;

            // Mode texture : étirer sur toute la longueur
            trail.textureMode = LineTextureMode.Stretch;
        }

        /// <summary>
        /// Gère l'activation/désactivation des trails selon la vitesse
        /// Utilise une hysteresis pour éviter le flickering
        /// </summary>
        private void UpdateTrailState()
        {
            bool shouldBeActive = currentSpeed >= trailData.activationSpeedThreshold;
            bool shouldDeactivate = currentSpeed < trailData.deactivationSpeedThreshold;

            // Activer si vitesse dépasse le seuil et pas déjà actif
            if (!isActive && shouldBeActive)
            {
                ActivateTrails();
                isActive = true;
            }
            // Désactiver si vitesse descend sous le seuil de désactivation et actif
            else if (isActive && shouldDeactivate)
            {
                DeactivateTrails();
                isActive = false;
            }
        }

        /// <summary>
        /// Active l'émission des trails
        /// </summary>
        private void ActivateTrails()
        {
            if (leftTrail != null)
                leftTrail.emitting = true;

            if (rightTrail != null)
                rightTrail.emitting = true;
        }

        /// <summary>
        /// Désactive l'émission des trails (ils vont naturellement fade out)
        /// </summary>
        private void DeactivateTrails()
        {
            if (leftTrail != null)
                leftTrail.emitting = false;

            if (rightTrail != null)
                rightTrail.emitting = false;
        }

        /// <summary>
        /// Met à jour dynamiquement les propriétés des trails selon la vitesse
        /// (largeur, durée de vie, couleur)
        /// </summary>
        private void UpdateTrailProperties()
        {
            // Seulement mettre à jour si les trails sont actifs
            if (!isActive)
                return;

            // Calculer le ratio de vitesse normalisé (0 = minSpeed, 1 = maxSpeed)
            float speedRatio = Mathf.Clamp01((currentSpeed - minSpeed) / (maxSpeed - minSpeed));

            // Interpoler la largeur selon la vitesse
            float width = Mathf.Lerp(trailData.baseWidth, trailData.maxWidth, speedRatio);

            // Interpoler la durée de vie selon la vitesse
            float time = Mathf.Lerp(trailData.baseTime, trailData.maxTime, speedRatio);

            // Évaluer la couleur depuis le gradient selon la vitesse
            Color color = trailData.speedColorGradient.Evaluate(speedRatio);

            // Appliquer sur les deux trails
            if (leftTrail != null)
            {
                leftTrail.startWidth = width;
                leftTrail.time = time;
                leftTrail.startColor = color;
            }

            if (rightTrail != null)
            {
                rightTrail.startWidth = width;
                rightTrail.time = time;
                rightTrail.startColor = color;
            }
        }
    }
}
