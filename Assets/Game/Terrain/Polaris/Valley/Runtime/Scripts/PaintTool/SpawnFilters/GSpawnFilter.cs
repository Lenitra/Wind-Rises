#if GRIFFIN
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// Base class for all spawn filters used by <see cref="GFoliagePainter"/> and <see cref="GObjectPainter"/>.
    /// This is a pre-process unit that will perform modification on to-be-spawned instances position, rotation and scale, or prevent it from being spawned.
    /// Only add these filters to GFoliagePainter or GObjectPainter object.
    /// </summary>
    [System.Serializable]
    [AddComponentMenu("")]
    public abstract class GSpawnFilter : MonoBehaviour
    {
        private static List<Type> allFilters;
        /// <summary>
        /// A collection of all available spawn filter types. This was initialize at startup.
        /// </summary>
        public static List<Type> AllFilters 
        {
            get
            {
                if (allFilters == null)
                    allFilters = new List<Type>();
                return allFilters;
            }
            internal set
            {
                allFilters = value;
            }
        }

        [SerializeField]
        private bool ignore;
        /// <summary>
        /// Ignore this filter from running against an instance.
        /// </summary>
        public bool Ignore
        {
            get
            {
                return ignore;
            }
            set
            {
                ignore = value;
            }
        }

        /// <summary>
        /// Perform modification on instance's position, rotation, scale, etc.
        /// </summary>
        /// <param name="args">Information about the to-be-spawned instance. Its initial values should be filled by the painter.</param>
        public abstract void Apply(ref GSpawnFilterArgs args);
    }
}
#endif
