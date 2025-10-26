#if GRIFFIN && !GRIFFIN_EXCLUDE_HIGHLAND
using UnityEngine;
using System;
using System.ComponentModel;

namespace Pinwheel.Griffin.SplineTool
{
    /// <summary>
    /// Represent a spline segment, a path connecting 2 anchor points.
    /// </summary>
    [System.Serializable]
    public class GSplineSegment : IDisposable
    {
        [SerializeField]
        private int startIndex;
        /// <summary>
        /// Index of the start anchor.
        /// </summary>
        /// <seealso cref="GSpline.Anchors"/>
        public int StartIndex
        {
            get
            {
                return startIndex;
            }
            set
            {
                startIndex = value;
            }
        }

        [SerializeField]
        private int endIndex;
        /// <summary>
        /// Index of the end anchor.
        /// </summary>
        /// <seealso cref="GSpline.Anchors"/>
        public int EndIndex
        {
            get
            {
                return endIndex;
            }
            set
            {
                endIndex = value;
            }
        }

        [SerializeField]
        private Vector3 startTangent;
        /// <summary>
        /// Position of the start anchor's tangent handle, in world space.
        /// </summary>
        public Vector3 StartTangent
        {
            get
            {
                return startTangent;
            }
            set
            {
                startTangent = value;
            }
        }

        [SerializeField]
        private Vector3 endTangent;
        /// <summary>
        /// Position of the end anchor's tangent handle, in world space.
        /// </summary>
        public Vector3 EndTangent
        {
            get
            {
                return endTangent;
            }
            set
            {
                endTangent = value;
            }
        }

        [ExcludeFromDoc]
        public GSweepTestData SweepTestData
        {
            get
            {
                return new GSweepTestData()
                {
                    startIndex = this.startIndex,
                    endIndex = this.endIndex,
                    startTangent = this.startTangent,
                    endTangent = this.endTangent
                };
            }
        }

        /// <summary>
        /// Flip the start and end anchor.
        /// </summary>
        public void FlipDirection()
        {
            int tmp = startIndex;
            startIndex = endIndex;
            endIndex = tmp;
        }

        [ExcludeFromDoc]
        public void Dispose()
        {
        }

        [ExcludeFromDoc]
        public struct GSweepTestData
        {
            public int startIndex;
            public int endIndex;
            public Vector3 startTangent;
            public Vector3 endTangent;
        }
    }
}
#endif
