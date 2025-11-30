using System;
using System.Collections.Generic;
using UnityEngine;

namespace WindRises.Flight
{
    /// <summary>
    /// Système d'enregistrement des positions et vélocités de l'avion.
    /// Permet de stocker l'historique de vol pour des fonctionnalités comme
    /// le replay, le rollback en cas de crash, ou l'analyse de trajectoire.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class FlightRecorder : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Intervalle de temps en secondes entre deux enregistrements")]
        [SerializeField] private float recordInterval = 0.5f;

        [Tooltip("Nombre maximum d'états à conserver (0 = illimité)")]
        [SerializeField] private int maxRecordedStates = 120; // 60 secondes à 0.5s d'intervalle

        [Header("Animation")]
        [Tooltip("Vitesse de lecture du rollback (1 = temps réel, 2 = 2x plus rapide)")]
        [SerializeField] private float rollbackPlaybackSpeed = 3f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        // Composants
        private Rigidbody rb;

        // Historique des états
        private List<PlaneState> recordedStates;
        private float lastRecordTime;

        // Animation de rollback
        private bool isPlayingRollback = false;
        private int rollbackTargetIndex = -1;
        private int rollbackCurrentIndex = -1;
        private float rollbackLerpProgress = 0f;

        // Propriétés publiques
        public int RecordedStateCount => recordedStates.Count;
        public bool IsRecording { get; private set; }
        public bool IsPlayingRollback => isPlayingRollback;
        public IReadOnlyList<PlaneState> RecordedStates => recordedStates.AsReadOnly();

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            recordedStates = new List<PlaneState>();
            IsRecording = true;
            lastRecordTime = Time.time;
        }

        private void Start()
        {
            // Enregistrer l'état initial
            RecordCurrentState();
        }

        private void FixedUpdate()
        {
            // Si un rollback est en cours, ne pas enregistrer et animer le retour
            if (isPlayingRollback)
            {
                AnimateRollback();
                return;
            }

            if (!IsRecording)
                return;

            // Vérifier si l'intervalle d'enregistrement est écoulé
            if (Time.time - lastRecordTime >= recordInterval)
            {
                RecordCurrentState();
                lastRecordTime = Time.time;
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo)
                return;

            GUILayout.BeginArea(new Rect(10, 200, 310, 110));
            GUI.Box(new Rect(0, 0, 310, 110), "");

            GUILayout.Label("=== FLIGHT RECORDER DEBUG ===");
            GUILayout.Label($"Recording: {(IsRecording ? "ON" : "OFF")}");
            GUILayout.Label($"Recorded States: {recordedStates.Count}");

            if (recordedStates.Count > 0)
            {
                float duration = recordedStates.Count * recordInterval;
                GUILayout.Label($"Duration: {duration:F1}s");
            }

            if (isPlayingRollback)
            {
                float progress = 1f - ((float)(rollbackCurrentIndex - rollbackTargetIndex) / (recordedStates.Count - 1 - rollbackTargetIndex));
                GUILayout.Label($"ROLLBACK: {progress:P0}");
            }

            GUILayout.EndArea();
        }

        /// <summary>
        /// Enregistre l'état actuel de l'avion (position, rotation, vélocité)
        /// </summary>
        private void RecordCurrentState()
        {
            PlaneState state = new PlaneState
            {
                timestamp = Time.time,
                position = transform.position,
                rotation = transform.rotation,
                velocity = rb.linearVelocity,
                angularVelocity = rb.angularVelocity
            };

            recordedStates.Add(state);

            // Limiter la taille de l'historique si nécessaire
            if (maxRecordedStates > 0 && recordedStates.Count > maxRecordedStates)
            {
                recordedStates.RemoveAt(0);
            }
        }

        /// <summary>
        /// Démarre l'enregistrement
        /// </summary>
        public void StartRecording()
        {
            IsRecording = true;
            lastRecordTime = Time.time;
        }

        /// <summary>
        /// Arrête l'enregistrement
        /// </summary>
        public void StopRecording()
        {
            IsRecording = false;
        }

        /// <summary>
        /// Efface tout l'historique enregistré
        /// </summary>
        public void ClearRecording()
        {
            recordedStates.Clear();
            lastRecordTime = Time.time;
        }

        /// <summary>
        /// Récupère l'état enregistré à un index donné
        /// </summary>
        public PlaneState GetStateAtIndex(int index)
        {
            if (index < 0 || index >= recordedStates.Count)
            {
                Debug.LogWarning($"Index {index} hors limites. Nombre d'états: {recordedStates.Count}");
                return null;
            }

            return recordedStates[index];
        }

        /// <summary>
        /// Récupère l'état le plus proche d'un timestamp donné
        /// </summary>
        public PlaneState GetStateAtTime(float timestamp)
        {
            if (recordedStates.Count == 0)
                return null;

            PlaneState closestState = recordedStates[0];
            float closestDiff = Mathf.Abs(timestamp - closestState.timestamp);

            foreach (PlaneState state in recordedStates)
            {
                float diff = Mathf.Abs(timestamp - state.timestamp);
                if (diff < closestDiff)
                {
                    closestDiff = diff;
                    closestState = state;
                }
            }

            return closestState;
        }

        /// <summary>
        /// Restaure l'avion à un état précédent
        /// </summary>
        public void RestoreState(PlaneState state)
        {
            if (state == null)
            {
                Debug.LogWarning("Impossible de restaurer un état null");
                return;
            }

            transform.position = state.position;
            transform.rotation = state.rotation;
            rb.linearVelocity = state.velocity;
            rb.angularVelocity = state.angularVelocity;
        }

        /// <summary>
        /// Revient en arrière d'un nombre de frames spécifié
        /// </summary>
        public void RollbackFrames(int frameCount)
        {
            if (recordedStates.Count == 0)
            {
                Debug.LogWarning("Aucun état enregistré pour effectuer un rollback");
                return;
            }

            int targetIndex = Mathf.Max(0, recordedStates.Count - 1 - frameCount);
            PlaneState targetState = recordedStates[targetIndex];
            RestoreState(targetState);

            // Supprimer les états après le rollback
            if (targetIndex < recordedStates.Count - 1)
            {
                recordedStates.RemoveRange(targetIndex + 1, recordedStates.Count - targetIndex - 1);
            }
        }

        /// <summary>
        /// Revient en arrière d'un nombre de secondes spécifié avec animation
        /// </summary>
        public void RollbackSeconds(float seconds)
        {
            if (recordedStates.Count == 0)
            {
                Debug.LogWarning("Aucun état enregistré pour effectuer un rollback");
                return;
            }

            if (isPlayingRollback)
            {
                Debug.LogWarning("Un rollback est déjà en cours");
                return;
            }

            float targetTime = Time.time - seconds;
            PlaneState targetState = GetStateAtTime(targetTime);

            if (targetState != null)
            {
                int targetIndex = recordedStates.IndexOf(targetState);
                if (targetIndex >= 0)
                {
                    // Démarrer l'animation de rollback
                    StartAnimatedRollback(targetIndex);
                }
            }
        }

        /// <summary>
        /// Démarre une animation de rollback vers un index cible
        /// </summary>
        private void StartAnimatedRollback(int targetIndex)
        {
            rollbackTargetIndex = targetIndex;
            rollbackCurrentIndex = recordedStates.Count - 1;
            rollbackLerpProgress = 0f;
            isPlayingRollback = true;

            // Arrêter l'enregistrement pendant le rollback
            StopRecording();
        }

        /// <summary>
        /// Anime le rollback frame par frame avec interpolation
        /// </summary>
        private void AnimateRollback()
        {
            if (rollbackCurrentIndex <= rollbackTargetIndex)
            {
                // Rollback terminé
                FinishRollback();
                return;
            }

            // Calculer la vitesse de progression basée sur l'intervalle d'enregistrement
            float progressSpeed = (rollbackPlaybackSpeed / recordInterval) * Time.fixedDeltaTime;
            rollbackLerpProgress += progressSpeed;

            // Si on a dépassé 1, passer au segment suivant
            while (rollbackLerpProgress >= 1f && rollbackCurrentIndex > rollbackTargetIndex)
            {
                rollbackLerpProgress -= 1f;
                rollbackCurrentIndex--;
            }

            // Clamp pour éviter les dépassements
            rollbackCurrentIndex = Mathf.Max(rollbackCurrentIndex, rollbackTargetIndex);
            rollbackLerpProgress = Mathf.Clamp01(rollbackLerpProgress);

            // Interpoler entre l'état actuel et le suivant
            int fromIndex = Mathf.Min(rollbackCurrentIndex + 1, recordedStates.Count - 1);
            int toIndex = rollbackCurrentIndex;

            if (fromIndex < recordedStates.Count && toIndex >= 0)
            {
                PlaneState fromState = recordedStates[fromIndex];
                PlaneState toState = recordedStates[toIndex];

                // Interpoler position et rotation
                transform.position = Vector3.Lerp(fromState.position, toState.position, rollbackLerpProgress);
                transform.rotation = Quaternion.Slerp(fromState.rotation, toState.rotation, rollbackLerpProgress);

                // Interpoler les vélocités
                rb.linearVelocity = Vector3.Lerp(fromState.velocity, toState.velocity, rollbackLerpProgress);
                rb.angularVelocity = Vector3.Lerp(fromState.angularVelocity, toState.angularVelocity, rollbackLerpProgress);
            }
        }

        /// <summary>
        /// Termine l'animation de rollback
        /// </summary>
        private void FinishRollback()
        {
            // Restaurer l'état final
            if (rollbackTargetIndex >= 0 && rollbackTargetIndex < recordedStates.Count)
            {
                PlaneState finalState = recordedStates[rollbackTargetIndex];
                transform.position = finalState.position;
                transform.rotation = finalState.rotation;
                rb.linearVelocity = finalState.velocity;
                rb.angularVelocity = finalState.angularVelocity;

                // Supprimer les états après le point de rollback
                if (rollbackTargetIndex < recordedStates.Count - 1)
                {
                    recordedStates.RemoveRange(rollbackTargetIndex + 1, recordedStates.Count - rollbackTargetIndex - 1);
                }
            }

            // Réinitialiser l'état du rollback
            isPlayingRollback = false;
            rollbackTargetIndex = -1;
            rollbackCurrentIndex = -1;
            rollbackLerpProgress = 0f;

            // Reprendre l'enregistrement
            StartRecording();
            lastRecordTime = Time.time;

            Debug.Log("Rollback animé terminé");
        }
    }

    /// <summary>
    /// Représente l'état complet de l'avion à un instant donné
    /// </summary>
    [Serializable]
    public class PlaneState
    {
        [Tooltip("Timestamp de cet état")]
        public float timestamp;

        [Tooltip("Position de l'avion")]
        public Vector3 position;

        [Tooltip("Rotation de l'avion")]
        public Quaternion rotation;

        [Tooltip("Vélocité linéaire (direction et vitesse de déplacement)")]
        public Vector3 velocity;

        [Tooltip("Vélocité angulaire (rotation)")]
        public Vector3 angularVelocity;

        public override string ToString()
        {
            return $"PlaneState[t={timestamp:F2}s, pos={position}, vel={velocity.magnitude:F1}m/s]";
        }
    }
}
