#if GRIFFIN && !GRIFFIN_EXCLUDE_HIGHLAND
using System;
using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine.Rendering;


namespace Pinwheel.Griffin.SplineTool
{
    /// <summary>
    /// A component for creating splines in the scene.
    /// Splines can have branches and loops. 
    /// You can also add <see cref="GSplineModifier"/> to this game object to perform some actions such as making ramps, rivers, paint paths, spawn and remove foliage, etc. based on the spline's shape.
    /// </summary>
    [System.Serializable]
    [ExecuteInEditMode]
    public partial class GSplineCreator : MonoBehaviour
    {
        /// <summary>
        /// This layer was used internally by this component for spline mask rendering. 
        /// </summary>
        public const string SPLINE_LAYER = "POLARIS SPLINE";

        public delegate void SplineChangedHandler(GSplineCreator sender);
        [ExcludeFromDoc]
        public static event SplineChangedHandler Editor_SplineChanged;
        /// <summary>
        /// An event signalling that the spline was changed (editing anchor, tangents, etc.).
        /// To raise this event, call <see cref="GSplineCreator.MarkSplineChanged(GSplineCreator)"/>.
        /// </summary>
        public static event SplineChangedHandler SplineChanged;

        [SerializeField]
        private int groupId;
        /// <summary>
        /// The terrain group that will be affected by this spline and its modifiers.
        /// </summary>
        public int GroupId
        {
            get
            {
                return groupId;
            }
            set
            {
                groupId = value;
            }
        }

        [SerializeField]
        private bool enableTerrainMask;
        /// <summary>
        /// Use terrain mask texture (R) to lock a region from being edited.
        /// </summary>
        public bool EnableTerrainMask
        {
            get
            {
                return enableTerrainMask;
            }
            set
            {
                enableTerrainMask = value;
            }
        }

        [SerializeField]
        private Vector3 positionOffset;
        /// <summary>
        /// Offset the new anchor position from mouse raycast hit point when adding to this spline. 
        /// Mostly used by the editor.
        /// When adding new anchors via script, this takes no effect.
        /// </summary>
        public Vector3 PositionOffset
        {
            get
            {
                return positionOffset;
            }
            set
            {
                positionOffset = value;
            }
        }

        [SerializeField]
        private Quaternion initialRotation;
        /// <summary>
        /// Initial rotation for new anchors when adding to this spline if Auto Tangent is off.
        /// Mostly used by the editor.
        /// When adding new anchors via script, this takes no effect.
        /// </summary>
        public Quaternion InitialRotation
        {
            get
            {
                return initialRotation;
            }
            set
            {
                initialRotation = value;
            }
        }

        [SerializeField]
        private Vector3 initialScale;
        /// <summary>
        /// Initial scale for new anchors when adding to this spline.
        /// Mostly used by the editor.
        /// When adding new anchors via script, this takes no effect.
        /// </summary>
        public Vector3 InitialScale
        {
            get
            {
                return initialScale;
            }
            set
            {
                initialScale = value;
            }
        }

        [Obsolete]
        [SerializeField]
        private int smoothness;
        [Obsolete]
        [ExcludeFromDoc]
        public int Smoothness
        {
            get
            {
                return smoothness;
            }
            set
            {
                smoothness = Mathf.Max(2, value);
            }
        }

        [Obsolete]
        [SerializeField]
        private float width;
        [Obsolete]
        [ExcludeFromDoc]
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

        [Obsolete]
        [SerializeField]
        private float falloffWidth;
        [Obsolete]
        [ExcludeFromDoc]
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

        [SerializeField]
        private GSpline spline;
        /// <summary>
        /// The spline created by this component, containing all anchor points, tangents and segments.
        /// </summary>
        public GSpline Spline
        {
            get
            {
                if (spline == null)
                {
                    spline = new GSpline();
                }
                return spline;
            }
            protected set
            {
                spline = value;
            }
        }

        private void Reset()
        {
            PositionOffset = Vector3.zero;
            InitialRotation = Quaternion.identity;
            InitialScale = Vector3.one;
        }

        private void OnEnable()
        {
            //UpdateMeshes();
        }

        private void OnDisable()
        {
            CleanUp();
        }

        public void CleanUp()
        {
            Spline.Dispose();
        }

        /// <summary>
        /// Notify subscribers that a spline has been edited.
        /// Other scripts should subscribe to the <see cref="SplineChanged"/> event.
        /// </summary>
        /// <param name="sender">The spline that has been edited.</param>
        public static void MarkSplineChanged(GSplineCreator sender)
        {
            if (Editor_SplineChanged != null)
            {
                Editor_SplineChanged.Invoke(sender);
            }
            if (SplineChanged != null)
            {
                SplineChanged.Invoke(sender);
            }
        }

        /// <summary>
        /// Generate the spline mesh containing only vertices (triangle list, 3 consecutive vertices for 1 triangle) and alpha channel for each vertices.
        /// Acquire the exact vertex count for pre-allocated array with <see cref="GetVerticesCount(int, float, float)"/>.
        /// </summary>
        /// <param name="vertices">Pre-allocated array storing all vertices.</param>
        /// <param name="alphas">Pre-allocated array storing all alpha values.</param>
        /// <param name="curviness">Number of 'quads' for each spline segment. Higher value means more accurate spline shape.</param>
        /// <param name="width">Width of the inner part of the spline, this region has alpha of 1.</param>
        /// <param name="falloffWidth">Width of the outter part of the spline, this region has alpha fades from 1 to 0.</param>
        public void GenerateWorldVerticesAndAlphas(Vector3[] vertices, float[] alphas, int curviness, float width, float falloffWidth)
        {
            int vIndex = 0;
            int aIndex = 0;
            List<GSplineSegment> segments = Spline.Segments;
            float tStep = 1f / (curviness - 1);
            float eps = tStep * 0.5f;

            //generate vertices in local space first
            for (int sIndex = 0; sIndex < segments.Count; ++sIndex)
            {
                for (int tIndex = 0; tIndex < curviness - 1; ++tIndex)
                {
                    float t0 = tIndex * tStep;
                    float t1 = (tIndex + 1) * tStep;

                    Vector3 c0 = spline.EvaluatePosition(sIndex, t0);
                    Vector3 c1 = spline.EvaluatePosition(sIndex, t1);

                    Vector3 normal0, normal1;
                    if (t0 == 0)
                    {
                        Matrix4x4 matrix0 = Spline.TRS(sIndex, t0);
                        normal0 = transform.TransformVector(matrix0.MultiplyVector(Vector3.left));
                    }
                    else
                    {
                        float dt00 = t0 - eps;
                        float dt01 = t0 + eps;
                        dt00 += sIndex;
                        dt01 += sIndex;
                        Vector3 tangent0 = (spline.EvaluatePosition(dt01) - spline.EvaluatePosition(dt00)).normalized;
                        Vector3 up0 = spline.EvaluateUpVector(sIndex, t0);
                        normal0 = Vector3.Cross(tangent0, up0);
                    }


                    if (t1 == 1)
                    {
                        Matrix4x4 matrix1 = Spline.TRS(sIndex, t1);
                        normal1 = transform.TransformVector(matrix1.MultiplyVector(Vector3.left));
                    }
                    else
                    {
                        float dt10 = t1 - eps;
                        float dt11 = t1 + eps;
                        dt10 += sIndex;
                        dt11 += sIndex;
                        Vector3 tangent1 = (spline.EvaluatePosition(dt11) - spline.EvaluatePosition(dt10)).normalized;
                        Vector3 up1 = spline.EvaluateUpVector(sIndex, t1);
                        normal1 = Vector3.Cross(tangent1, up1);
                    }

                    Vector3 bl, tl, tr, br;
                    float halfWidth = width * 0.5f;

                    if (falloffWidth > 0)
                    {
                        //Left falloff
                        bl = c0 - normal0 * (halfWidth + falloffWidth);
                        tl = c1 - normal1 * (halfWidth + falloffWidth);
                        tr = c1 - normal1 * halfWidth;
                        br = c0 - normal0 * halfWidth;

                        vertices[vIndex++] = bl;
                        vertices[vIndex++] = tl;
                        vertices[vIndex++] = tr;
                        alphas[aIndex++] = 0;
                        alphas[aIndex++] = 0;
                        alphas[aIndex++] = 1;

                        vertices[vIndex++] = bl;
                        vertices[vIndex++] = tr;
                        vertices[vIndex++] = br;
                        alphas[aIndex++] = 0;
                        alphas[aIndex++] = 1;
                        alphas[aIndex++] = 1;
                    }

                    if (width > 0)
                    {
                        //Center
                        bl = c0 - normal0 * halfWidth;
                        tl = c1 - normal1 * halfWidth;
                        tr = c1 + normal1 * halfWidth;
                        br = c0 + normal0 * halfWidth;

                        vertices[vIndex++] = bl;
                        vertices[vIndex++] = tl;
                        vertices[vIndex++] = tr;
                        alphas[aIndex++] = 1;
                        alphas[aIndex++] = 1;
                        alphas[aIndex++] = 1;

                        vertices[vIndex++] = bl;
                        vertices[vIndex++] = tr;
                        vertices[vIndex++] = br;
                        alphas[aIndex++] = 1;
                        alphas[aIndex++] = 1;
                        alphas[aIndex++] = 1;
                    }

                    if (falloffWidth > 0)
                    {
                        //Right falloff
                        bl = c0 + normal0 * halfWidth;
                        tl = c1 + normal1 * halfWidth;
                        tr = c1 + normal1 * (halfWidth + falloffWidth);
                        br = c0 + normal0 * (halfWidth + falloffWidth);

                        vertices[vIndex++] = bl;
                        vertices[vIndex++] = tl;
                        vertices[vIndex++] = tr;
                        alphas[aIndex++] = 1;
                        alphas[aIndex++] = 1;
                        alphas[aIndex++] = 0;

                        vertices[vIndex++] = bl;
                        vertices[vIndex++] = tr;
                        vertices[vIndex++] = br;
                        alphas[aIndex++] = 1;
                        alphas[aIndex++] = 0;
                        alphas[aIndex++] = 0;
                    }
                }
            }

            //transform to world space
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = transform.TransformPoint(vertices[i]);
            }
        }

        /// <summary>
        /// Retrieve the exact number of vertex for the spline mesh.
        /// </summary>
        /// <param name="curviness">Number of 'quads' for each spline segment. Higher value means more accurate spline shape.</param>
        /// <param name="width">Width of the inner part of the spline, this region has alpha of 1.</param>
        /// <param name="falloffWidth">Width of the outter part of the spline, this region has alpha fades from 1 to 0.</param>
        /// <returns>Number of vertex.</returns>
        public int GetVerticesCount(int curviness, float width, float falloffWidth)
        {
            int fragmentCountPerSegment = curviness - 1;
            int segmentCount = Spline.Segments.Count;

            int vertexCountPerFragment = 0;
            if (width > 0)
            {
                vertexCountPerFragment += 6;
            }
            if (falloffWidth > 0)
            {
                vertexCountPerFragment += 12;
            }

            return segmentCount * fragmentCountPerSegment * vertexCountPerFragment;
        }
    }
}
#endif
