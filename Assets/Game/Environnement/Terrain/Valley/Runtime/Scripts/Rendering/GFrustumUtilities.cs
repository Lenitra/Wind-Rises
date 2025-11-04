#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace Pinwheel.Griffin.Rendering
{
    /// <summary>
    /// Utility class for calculating camera's frustum planes.
    /// </summary>
    public static class GFrustumUtilities
    {
        private static Vector3[] corners = new Vector3[4];

        /// <summary>
        /// Calculate the 6 frustum planes corresponding to a camera view with overriden zFar
        /// </summary>
        /// <param name="cam">Target camera.</param>
        /// <param name="planes">Pre-allocated array storing 6 frustum planes.</param>
        /// <param name="zFar">Target zFar (e.g: max foliage render distance)</param>
        public static void Calculate(Camera cam, Plane[] planes, float zFar)
        {
            GeometryUtility.CalculateFrustumPlanes(cam, planes);
            cam.CalculateFrustumCorners(GCommon.UnitRect, zFar, Camera.MonoOrStereoscopicEye.Mono, corners);
            planes[5].Set3Points(
                cam.transform.TransformPoint(corners[0]),
                cam.transform.TransformPoint(corners[1]),
                cam.transform.TransformPoint(corners[2]));
        }
    }
}
#endif
