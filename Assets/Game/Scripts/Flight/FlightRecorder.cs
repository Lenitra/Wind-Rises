using System;
using UnityEngine;

namespace WindRises.Flight
{
    /// <summary>
    /// Système d'enregistrement des positions et vélocités de l'avion.
    /// Utilise un buffer circulaire pour stocker l'historique de vol sans allocations GC.
    /// Permet le rollback en cas de crash ou sur demande du joueur.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class FlightRecorder : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Intervalle de temps en secondes entre deux enregistrements")]
        [SerializeField] private float recordInterval = 0.5f;

        [Tooltip("Nombre maximum d'états à conserver")]
        [SerializeField] private int maxRecordedStates = 120;

        [Header("Animation")]
        [Tooltip("Vitesse de lecture du rollback (1 = temps réel, 3 = 3x plus rapide)")]
        [SerializeField] private float rollbackPlaybackSpeed = 3f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        // Composants
        private Rigidbody rb;

        // Buffer circulaire (allocation unique, zéro GC en runtime)
        private PlaneState[] stateBuffer;
        private int bufferHead;   // Prochaine position d'écriture
        private int bufferCount;  // Nombre d'états actuellement stockés

        private float lastRecordTime;

        // Animation de rollback
        private bool isPlayingRollback;
        private int rollbackTargetIndex = -1;
        private int rollbackCurrentIndex = -1;
        private float rollbackLerpProgress;
        private bool wasKinematic;

        // Propriétés publiques
        public int RecordedStateCount => bufferCount;
        public bool IsRecording { get; private set; }
        public bool IsPlayingRollback => isPlayingRollback;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // Allocation unique du buffer circulaire
            int capacity = Mathf.Max(1, maxRecordedStates);
            stateBuffer = new PlaneState[capacity];
            bufferHead = 0;
            bufferCount = 0;

            IsRecording = true;
            lastRecordTime = Time.time;
        }

        private void Start()
        {
            RecordCurrentState();
        }

        private void FixedUpdate()
        {
            if (isPlayingRollback)
            {
                AnimateRollback();
                return;
            }

            if (!IsRecording)
                return;

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
            GUILayout.Label($"Recorded States: {bufferCount}");

            if (bufferCount > 0)
            {
                float duration = bufferCount * recordInterval;
                GUILayout.Label($"Duration: {duration:F1}s");
            }

            if (isPlayingRollback)
            {
                int remaining = rollbackCurrentIndex - rollbackTargetIndex;
                int total = bufferCount - 1 - rollbackTargetIndex;
                float progress = total > 0 ? 1f - (float)remaining / total : 1f;
                GUILayout.Label($"ROLLBACK: {progress:P0}");
            }

            GUILayout.EndArea();
        }

        #region Recording

        /// <summary>
        /// Enregistre l'état actuel dans le buffer circulaire. O(1), zéro allocation.
        /// </summary>
        private void RecordCurrentState()
        {
            stateBuffer[bufferHead] = new PlaneState
            {
                timestamp = Time.time,
                position = transform.position,
                rotation = transform.rotation,
                velocity = rb.linearVelocity,
                angularVelocity = rb.angularVelocity
            };

            bufferHead = (bufferHead + 1) % stateBuffer.Length;
            if (bufferCount < stateBuffer.Length)
                bufferCount++;
        }

        public void StartRecording()
        {
            IsRecording = true;
            lastRecordTime = Time.time;
        }

        public void StopRecording()
        {
            IsRecording = false;
        }

        public void ClearRecording()
        {
            bufferCount = 0;
            bufferHead = 0;
            lastRecordTime = Time.time;
        }

        #endregion

        #region State Access

        /// <summary>
        /// Accède à un état par index logique (0 = le plus ancien). O(1).
        /// </summary>
        public PlaneState? GetStateAtIndex(int index)
        {
            if (index < 0 || index >= bufferCount)
            {
                Debug.LogWarning($"Index {index} hors limites. Nombre d'états: {bufferCount}");
                return null;
            }

            return stateBuffer[LogicalToPhysical(index)];
        }

        /// <summary>
        /// Trouve l'index logique de l'état le plus proche d'un timestamp donné.
        /// Recherche binaire O(log n) car les timestamps sont monotoniquement croissants.
        /// Retourne -1 si le buffer est vide.
        /// </summary>
        public int FindClosestStateIndex(float timestamp)
        {
            if (bufferCount == 0)
                return -1;

            int lo = 0;
            int hi = bufferCount - 1;

            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                float midTime = stateBuffer[LogicalToPhysical(mid)].timestamp;

                if (midTime < timestamp)
                    lo = mid + 1;
                else
                    hi = mid;
            }

            // Vérifier si l'élément précédent est plus proche
            if (lo > 0)
            {
                float diffCurrent = Mathf.Abs(stateBuffer[LogicalToPhysical(lo)].timestamp - timestamp);
                float diffPrev = Mathf.Abs(stateBuffer[LogicalToPhysical(lo - 1)].timestamp - timestamp);
                if (diffPrev < diffCurrent)
                    return lo - 1;
            }

            return lo;
        }

        /// <summary>
        /// Convertit un index logique (0 = plus ancien) en index physique dans le buffer circulaire.
        /// </summary>
        private int LogicalToPhysical(int logicalIndex)
        {
            return (bufferHead - bufferCount + logicalIndex + stateBuffer.Length) % stateBuffer.Length;
        }

        #endregion

        #region Restore & Rollback

        /// <summary>
        /// Restaure l'avion à un état précédent
        /// </summary>
        public void RestoreState(PlaneState state)
        {
            transform.position = state.position;
            transform.rotation = state.rotation;
            rb.linearVelocity = state.velocity;
            rb.angularVelocity = state.angularVelocity;
        }

        /// <summary>
        /// Revient en arrière d'un nombre de frames spécifié (instantané, sans animation)
        /// </summary>
        public void RollbackFrames(int frameCount)
        {
            if (bufferCount == 0)
            {
                Debug.LogWarning("Aucun état enregistré pour effectuer un rollback");
                return;
            }

            int targetIndex = Mathf.Max(0, bufferCount - 1 - frameCount);
            RestoreState(stateBuffer[LogicalToPhysical(targetIndex)]);
            TruncateAfter(targetIndex);
        }

        /// <summary>
        /// Revient en arrière d'un nombre de secondes spécifié avec animation.
        /// Passe le Rigidbody en kinematic pour éviter les collisions fantômes pendant le retour.
        /// </summary>
        public void RollbackSeconds(float seconds)
        {
            if (bufferCount == 0)
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
            int targetIndex = FindClosestStateIndex(targetTime);

            if (targetIndex >= 0)
            {
                StartAnimatedRollback(targetIndex);
            }
        }

        /// <summary>
        /// Démarre une animation de rollback vers un index logique cible.
        /// Active isKinematic pour empêcher la physique d'interférer et éviter
        /// les re-collisions pendant que l'avion retrace sa trajectoire.
        /// </summary>
        private void StartAnimatedRollback(int targetIndex)
        {
            rollbackTargetIndex = targetIndex;
            rollbackCurrentIndex = bufferCount - 1;
            rollbackLerpProgress = 0f;
            isPlayingRollback = true;

            // Désactiver la physique pendant le rollback
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;

            StopRecording();
        }

        /// <summary>
        /// Anime le rollback frame par frame avec interpolation lissée.
        /// Utilise MovePosition/MoveRotation pour un mouvement fluide en mode kinematic.
        /// </summary>
        private void AnimateRollback()
        {
            if (rollbackCurrentIndex <= rollbackTargetIndex)
            {
                FinishRollback();
                return;
            }

            float progressSpeed = (rollbackPlaybackSpeed / recordInterval) * Time.fixedDeltaTime;
            rollbackLerpProgress += progressSpeed;

            while (rollbackLerpProgress >= 1f && rollbackCurrentIndex > rollbackTargetIndex)
            {
                rollbackLerpProgress -= 1f;
                rollbackCurrentIndex--;
            }

            rollbackCurrentIndex = Mathf.Max(rollbackCurrentIndex, rollbackTargetIndex);
            rollbackLerpProgress = Mathf.Clamp01(rollbackLerpProgress);

            int fromIndex = Mathf.Min(rollbackCurrentIndex + 1, bufferCount - 1);
            int toIndex = rollbackCurrentIndex;

            PlaneState fromState = stateBuffer[LogicalToPhysical(fromIndex)];
            PlaneState toState = stateBuffer[LogicalToPhysical(toIndex)];

            // MovePosition/MoveRotation pour un mouvement interpolé propre en kinematic
            Vector3 pos = Vector3.Lerp(fromState.position, toState.position, rollbackLerpProgress);
            Quaternion rot = Quaternion.Slerp(fromState.rotation, toState.rotation, rollbackLerpProgress);

            rb.MovePosition(pos);
            rb.MoveRotation(rot);
        }

        /// <summary>
        /// Termine l'animation de rollback, restaure l'état physique et reprend l'enregistrement.
        /// </summary>
        private void FinishRollback()
        {
            if (rollbackTargetIndex >= 0 && rollbackTargetIndex < bufferCount)
            {
                PlaneState finalState = stateBuffer[LogicalToPhysical(rollbackTargetIndex)];
                transform.position = finalState.position;
                transform.rotation = finalState.rotation;

                // Restaurer la physique AVANT d'appliquer les vélocités
                rb.isKinematic = wasKinematic;
                rb.linearVelocity = finalState.velocity;
                rb.angularVelocity = finalState.angularVelocity;

                TruncateAfter(rollbackTargetIndex);
            }
            else
            {
                rb.isKinematic = wasKinematic;
            }

            isPlayingRollback = false;
            rollbackTargetIndex = -1;
            rollbackCurrentIndex = -1;
            rollbackLerpProgress = 0f;

            StartRecording();
            lastRecordTime = Time.time;

            Debug.Log("Rollback animé terminé");
        }

        /// <summary>
        /// Tronque le buffer pour ne garder que les états jusqu'à l'index logique donné (inclus). O(1).
        /// </summary>
        private void TruncateAfter(int lastLogicalIndex)
        {
            if (lastLogicalIndex < bufferCount - 1)
            {
                bufferHead = (LogicalToPhysical(lastLogicalIndex) + 1) % stateBuffer.Length;
                bufferCount = lastLogicalIndex + 1;
            }
        }

        #endregion
    }

    /// <summary>
    /// État complet de l'avion à un instant donné.
    /// Struct pour éviter les allocations heap et la pression sur le GC.
    /// </summary>
    [Serializable]
    public struct PlaneState
    {
        public float timestamp;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 angularVelocity;

        public override string ToString()
        {
            return $"PlaneState[t={timestamp:F2}s, pos={position}, vel={velocity.magnitude:F1}m/s]";
        }
    }
}
