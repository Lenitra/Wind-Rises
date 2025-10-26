#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Griffin
{
    /// <summary>
    /// Utility class for spawning game object on the terrain.
    /// </summary>
    public static class GSpawner
    {
        /// <summary>
        /// Get the root game object name for spawning a prefab into, use this to keep the hierarchy clean.
        /// </summary>
        /// <param name="prototype">The prefab.</param>
        /// <returns>Root game object name.</returns>
        public static string GetPrototypeRootName(GameObject prototype)
        {
            return string.Format("~Root_{0}", prototype.name);
        }

        /// <summary>
        /// Instantiate a cloned object of a prefab and place it under the terrain.
        /// This function also records undo in the editor.
        /// </summary>
        /// <param name="terrain">The terrain object.</param>
        /// <param name="prototype">The prefab.</param>
        /// <param name="worldPos">World position of spawned object.</param>
        /// <param name="parent">Parent transform of spawned object. If null, a default root object will be used.</param>
        /// <returns>The spawned object.</returns>
        public static GameObject Spawn(GStylizedTerrain terrain, GameObject prototype, Vector3 worldPos, Transform parent = null)
        {
            GameObject g = null;
#if UNITY_EDITOR
            bool isPrefab = false;
#if UNITY_2018_1 || UNITY_2018_2
            isPrefab = PrefabUtility.GetPrefabObject(prototype);
#else
            isPrefab = PrefabUtility.IsPartOfPrefabAsset(prototype);
#endif

            if (isPrefab)
            {
                g = PrefabUtility.InstantiatePrefab(prototype) as GameObject;
            }
            else
            {
                g = GameObject.Instantiate<GameObject>(prototype);
            }

            string undoName = string.Format("Spawn {0}", prototype.name);
            Undo.RegisterCreatedObjectUndo(g, undoName);
#else
            g = GameObject.Instantiate<GameObject>(prototype);
#endif
            if (parent == null)
            {
                parent = GetRoot(terrain, prototype).transform;
            }
            g.transform.parent = parent;

            g.transform.position = worldPos;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;

            return g;
        }

        /// <summary>
        /// Destroy instances of a prefab under a terrain, matching a condition.
        /// </summary>
        /// <param name="terrain">The terrain object.</param>
        /// <param name="prototype">The prefab.</param>
        /// <param name="condition">The condition to test against each instance.</param>
        public static void DestroyIf(GStylizedTerrain terrain, GameObject prototype, System.Predicate<GameObject> condition)
        {
            Transform parent = GetRoot(terrain, prototype).transform;
            GameObject[] children = GUtilities.GetChildrenGameObjects(parent);
            for (int i = 0; i < children.Length; ++i)
            {
                if (condition.Invoke(children[i]))
                {
#if UNITY_EDITOR
                    Undo.DestroyObjectImmediate(children[i]);
#else
                    GUtilities.DestroyGameobject(children[i]);
#endif
                }
            }
        }

        /// <summary>
        /// Destroy instances of all prefabs under a terrain, matching a condition.
        /// The condition will be tested against each instance under default root object, whose name starts with ~Root
        /// </summary>
        /// <param name="terrain">The terrain object.</param>
        /// <param name="condition">The condition to test against each instance.</param>
        public static void DestroyIf(GStylizedTerrain terrain, System.Predicate<GameObject> condition)
        {
            List<Transform> parents = new List<Transform>();
            foreach (Transform t in terrain.transform)
            {
                if (t.name.StartsWith("~Root"))
                {
                    parents.Add(t);
                }
            }

            for (int i = 0; i < parents.Count; ++i)
            {
                GameObject[] children = GUtilities.GetChildrenGameObjects(parents[i]);
                for (int j = 0; j < children.Length; ++j)
                {
                    if (condition.Invoke(children[j]))
                    {
#if UNITY_EDITOR
                        Undo.DestroyObjectImmediate(children[j]);
#else
                    GUtilities.DestroyGameobject(children[j]);
#endif
                    }
                }
            }
        }
        
        /// <summary>
        /// Get the root game object to spawn a prefab into. This will return a child game object of the terrain, all instances of the same prefab will be placed under this object.
        /// </summary>
        /// <param name="terrain">The terrain object.</param>
        /// <param name="prototype">The prefab.</param>
        /// <returns>The root game object corresponding target prefab.</returns>
        public static GameObject GetRoot(GStylizedTerrain terrain, GameObject prototype)
        {
            Transform parent = terrain.transform;
            string name = GetPrototypeRootName(prototype);

            Transform t = parent.Find(name);
            if (t == null)
            {
                GameObject g = new GameObject(name);
                g.transform.parent = parent;
                GUtilities.ResetTransform(g.transform, parent);
                t = g.transform;

                GObjectHelper oh = g.AddComponent<GObjectHelper>();
                oh.Terrain = terrain;
            }
            return t.gameObject;
        }
    }
}
#endif
