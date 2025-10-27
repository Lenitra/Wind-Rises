#if GRIFFIN && !GRIFFIN_EXCLUDE_HIGHLAND
using UnityEngine;

namespace Pinwheel.Griffin.SplineTool
{
    /// <summary>
    /// Base class for all spline modifiers, components to perform actual operation on terrains using spline shape.
    /// Modifiers can be added to the same game object or child object of the <see cref="GSplineCreator"/>.
    /// </summary>
    [System.Serializable]
    public abstract class GSplineModifier : MonoBehaviour
    {
        [SerializeField]
        protected GSplineCreator splineCreator;
        /// <summary>
        /// The <see cref="GSplineCreator"/> component attached to this game object or its parent.
        /// </summary>
        public GSplineCreator SplineCreator
        {
            get
            {
                if (splineCreator == null)
                {
                    splineCreator = GetComponentInParent<GSplineCreator>();
                }
                return splineCreator;
            }
            set
            {
                splineCreator = value;
            }
        }

        [SerializeField]
        protected int curviness = 5;
        /// <summary>
        /// Number of 'quads' for each spline segment. Higher value means more accurate spline shape.
        /// </summary>
        public int Curviness
        {
            get
            {
                return curviness;
            }
            set
            {
                curviness = Mathf.Max(2, value);
            }
        }

        [SerializeField]
        protected float width = 5;
        /// <summary>
        /// Width of the inner part of the spline, this region has alpha of 1.
        /// </summary>
        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                width = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        protected float falloffWidth = 5;
        /// <summary>
        /// Width of the outter part of the spline, this region has alpha fades from 1 to 0.
        /// </summary>
        public float FalloffWidth
        {
            get
            {
                return falloffWidth;
            }
            set
            {
                falloffWidth = Mathf.Max(0, value);
            }
        }

        /// <summary>
        /// Perform the action on terrains.
        /// </summary>
        public abstract void Apply();
    }
}
#endif
