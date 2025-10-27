#if GRIFFIN && !GRIFFIN_EXCLUDE_HIGHLAND
using UnityEngine;

namespace Pinwheel.Griffin.SplineTool
{
    /// <summary>
    /// Represent a spline's anchor point.
    /// </summary>
    [System.Serializable]
    public class GSplineAnchor
    {
        [SerializeField]
        private Vector3 position;
        /// <summary>
        /// Position in local space.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        [SerializeField]
        private Quaternion rotation;
        /// <summary>
        /// Rotation in local space.
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }

        [SerializeField]
        private Vector3 scale;
        /// <summary>
        /// Scale in local space.
        /// </summary>
        public Vector3 Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }

        [ExcludeFromDoc]
        public GSweepTestData SweepTestData
        {
            get
            {
                return new GSweepTestData()
                {
                    position = this.position,
                    scale = this.scale
                };
            }
        }

        /// <summary>
        /// Create new anchor point with provided position, default rotation and scale.
        /// </summary>
        /// <param name="pos">Position in local space.</param>
        public GSplineAnchor(Vector3 pos)
        {
            position = pos;
            rotation = Quaternion.identity;
            scale = Vector3.one;
        }

        [ExcludeFromDoc]
        public struct GSweepTestData
        {
            public Vector3 position;
            public Vector3 scale;
        }
    }
}
#endif
