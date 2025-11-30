using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WindRises.Flight;

namespace WindRises.Environment
{
    /// <summary>
    /// Gestionnaire de collisions pour le terrain et tous ses enfants.
    /// Détecte automatiquement les collisions avec l'avion sur l'objet parent et tous ses enfants récursivement.
    /// Déclenche un rollback temporisé pour permettre au joueur de récupérer après un crash.
    /// </summary>
    public class TerrainCollisions : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Tag de l'objet avion à détecter")]
        [SerializeField] private string planeTag = "Player";

        [Tooltip("Durée de la pause avant le rollback (en secondes)")]
        [SerializeField] private float pauseDuration = 1f;

        [Tooltip("Durée du rollback (en secondes)")]
        [SerializeField] private float rollbackDuration = 5f;

        [Header("Options")]
        [Tooltip("Ralentir le temps pendant la pause (effet slow-motion)")]
        [SerializeField] private bool useSlowMotion = true;

        [Tooltip("Vitesse du temps pendant la pause (0.1 = 10% de la vitesse normale)")]
        [SerializeField] private float slowMotionTimeScale = 0.2f;

        [Tooltip("Appliquer automatiquement aux enfants au démarrage")]
        [SerializeField] private bool autoSetupChildren = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        // État partagé entre tous les colliders du terrain
        private static bool isProcessingCrash = false;
        private static Coroutine currentCrashCoroutine = null;
        private static TerrainCollisions mainInstance = null;

        // Liste des colliders gérés
        private List<TerrainCollisionTrigger> childTriggers = new List<TerrainCollisionTrigger>();

        private void Awake()
        {
            // Définir cette instance comme instance principale si aucune n'existe
            if (mainInstance == null)
            {
                mainInstance = this;
            }
        }

        private void Start()
        {
            if (autoSetupChildren)
            {
                SetupCollisionHandlers();
            }
        }

        /// <summary>
        /// Configure les handlers de collision sur cet objet et tous ses enfants récursivement
        /// </summary>
        public void SetupCollisionHandlers()
        {
            // Nettoyer les anciens triggers
            foreach (var trigger in childTriggers)
            {
                if (trigger != null)
                {
                    Destroy(trigger);
                }
            }
            childTriggers.Clear();

            // Ajouter un handler sur cet objet s'il a un collider
            AddCollisionHandler(gameObject);

            // Ajouter récursivement sur tous les enfants
            AddCollisionHandlersRecursive(transform);

            if (showDebugInfo)
            {
                Debug.Log($"TerrainCollisions: {childTriggers.Count} colliders configurés (objet + enfants récursifs)");
            }
        }

        /// <summary>
        /// Ajoute récursivement des handlers de collision à tous les enfants
        /// </summary>
        private void AddCollisionHandlersRecursive(Transform parent)
        {
            foreach (Transform child in parent)
            {
                // Ajouter un handler si l'enfant a un collider
                AddCollisionHandler(child.gameObject);

                // Continuer récursivement
                AddCollisionHandlersRecursive(child);
            }
        }

        /// <summary>
        /// Ajoute un handler de collision à un GameObject spécifique
        /// </summary>
        private void AddCollisionHandler(GameObject obj)
        {
            // Vérifier si l'objet a un collider
            Collider col = obj.GetComponent<Collider>();
            if (col == null)
                return;

            // Ne pas ajouter si c'est un trigger (géré différemment)
            if (col.isTrigger)
                return;

            // Vérifier s'il n'y a pas déjà un trigger sur cet objet
            TerrainCollisionTrigger existingTrigger = obj.GetComponent<TerrainCollisionTrigger>();
            if (existingTrigger != null)
            {
                existingTrigger.Initialize(this);
                childTriggers.Add(existingTrigger);
                return;
            }

            // Ajouter un nouveau trigger
            TerrainCollisionTrigger trigger = obj.AddComponent<TerrainCollisionTrigger>();
            trigger.Initialize(this);
            childTriggers.Add(trigger);
        }

        /// <summary>
        /// Appelé par les triggers enfants quand une collision est détectée
        /// </summary>
        public void OnPlaneCollision(Collision collision, GameObject collisionObject)
        {
            // Ignorer si un crash est déjà en cours de traitement
            if (isProcessingCrash)
                return;

            // Récupérer le FlightRecorder de l'avion
            FlightRecorder recorder = collision.gameObject.GetComponent<FlightRecorder>();

            if (recorder != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"CRASH détecté sur '{collisionObject.name}'! Point d'impact: {collision.contacts[0].point}");
                }

                // Démarrer la séquence de rollback (utiliser l'instance principale)
                if (mainInstance != null && mainInstance.gameObject.activeInHierarchy)
                {
                    if (currentCrashCoroutine != null)
                    {
                        mainInstance.StopCoroutine(currentCrashCoroutine);
                    }
                    currentCrashCoroutine = mainInstance.StartCoroutine(CrashRecoverySequence(recorder));
                }
            }
            else
            {
                Debug.LogWarning("FlightRecorder non trouvé sur l'avion. Impossible d'effectuer un rollback.");
            }
        }

        /// <summary>
        /// Séquence de récupération après un crash :
        /// 1. Pause avec slow-motion
        /// 2. Rollback automatique
        /// 3. Reprise normale
        /// </summary>
        private IEnumerator CrashRecoverySequence(FlightRecorder recorder)
        {
            isProcessingCrash = true;

            // 1. Activer le slow-motion si configuré
            float originalTimeScale = Time.timeScale;
            if (useSlowMotion)
            {
                Time.timeScale = slowMotionTimeScale;
                if (showDebugInfo)
                {
                    Debug.Log($"Slow-motion activé ({slowMotionTimeScale * 100}% vitesse)");
                }
            }

            // 2. Attendre la durée de la pause (en temps réel, pas affecté par timeScale)
            yield return new WaitForSecondsRealtime(pauseDuration);

            // 3. Restaurer le temps normal avant le rollback
            Time.timeScale = originalTimeScale;

            // 4. Déclencher le rollback
            if (showDebugInfo)
            {
                Debug.Log($"Déclenchement du rollback de {rollbackDuration}s");
            }

            recorder.RollbackSeconds(rollbackDuration);

            // 5. Attendre la fin du rollback pour permettre un nouveau crash
            yield return new WaitUntil(() => !recorder.IsPlayingRollback);

            // 6. Petite pause de sécurité pour éviter les re-détections immédiates
            yield return new WaitForSeconds(0.5f);

            // 7. Fin de la séquence
            isProcessingCrash = false;
            currentCrashCoroutine = null;

            if (showDebugInfo)
            {
                Debug.Log("Récupération après crash terminée");
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo || !isProcessingCrash)
                return;

            // Afficher un message visuel pendant le crash
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 50, 300, 100));
            GUI.Box(new Rect(0, 0, 300, 100), "");

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };

            GUILayout.FlexibleSpace();
            GUILayout.Label("CRASH!", style);
            GUILayout.Label("Récupération en cours...", GUI.skin.label);
            GUILayout.FlexibleSpace();

            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            // Nettoyer si c'est l'instance principale
            if (mainInstance == this)
            {
                mainInstance = null;

                // S'assurer que le timeScale est restauré
                if (isProcessingCrash)
                {
                    Time.timeScale = 1f;
                    isProcessingCrash = false;
                }
            }

            // Nettoyer les triggers
            foreach (var trigger in childTriggers)
            {
                if (trigger != null)
                {
                    Destroy(trigger);
                }
            }
            childTriggers.Clear();
        }

        private void OnApplicationQuit()
        {
            // S'assurer que le timeScale est restauré à la fermeture
            Time.timeScale = 1f;
            isProcessingCrash = false;
        }

#if UNITY_EDITOR
        [ContextMenu("Setup Collision Handlers")]
        private void SetupCollisionHandlersEditor()
        {
            SetupCollisionHandlers();
        }

        [ContextMenu("Count Colliders")]
        private void CountColliders()
        {
            int count = 0;
            if (GetComponent<Collider>() != null) count++;
            count += CountCollidersRecursive(transform);
            Debug.Log($"Nombre total de colliders trouvés: {count}");
        }

        private int CountCollidersRecursive(Transform parent)
        {
            int count = 0;
            foreach (Transform child in parent)
            {
                if (child.GetComponent<Collider>() != null)
                    count++;
                count += CountCollidersRecursive(child);
            }
            return count;
        }
#endif
    }

    /// <summary>
    /// Composant trigger ajouté automatiquement aux objets avec colliders.
    /// Redirige les collisions vers le gestionnaire principal TerrainCollisions.
    /// </summary>
    [AddComponentMenu("")] // Cache dans le menu Add Component
    public class TerrainCollisionTrigger : MonoBehaviour
    {
        private TerrainCollisions manager;
        private string planeTag = "Player";

        public void Initialize(TerrainCollisions manager)
        {
            this.manager = manager;
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Vérifier si c'est l'avion
            if (collision.gameObject.CompareTag(planeTag) && manager != null)
            {
                manager.OnPlaneCollision(collision, gameObject);
            }
        }
    }
}
