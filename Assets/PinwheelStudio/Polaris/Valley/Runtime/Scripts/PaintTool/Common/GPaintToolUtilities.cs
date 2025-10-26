#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Collections;
using Unity.Jobs;

namespace Pinwheel.Griffin.PaintTool
{
    public static class GPaintToolUtilities
    {
        /// <summary>
        /// Populate the list with all custom spawn filter.
        /// The list should be populated with builtin filter that supported by the painter first.
        /// Use this function to build a list of supported spawn filter for a painter, then populate a selection popup from that.
        /// </summary>
        /// <param name="filters">A list of spawn filter types.</param>
        public static void AddCustomSpawnFilter(List<Type> filters)
        {
            for (int i = 0; i < GSpawnFilter.AllFilters.Count; ++i)
            {
                Type t = GSpawnFilter.AllFilters[i];
                if (!IsBuiltinFilter(t))
                    filters.Add(t);
            }
        }

        /// <summary>
        /// Check if a spawn filter is provided builtin.
        /// </summary>
        /// <param name="t">Type of the spawn filter. Use typeof() or GetType() for this.</param>
        /// <returns>True if a spawn filter type is provided builtin.</returns>
        private static bool IsBuiltinFilter(Type t)
        {
            return t == typeof(GAlignToSurfaceFilter) ||
                    t == typeof(GHeightConstraintFilter) ||
                    t == typeof(GRotationRandomizeFilter) ||
                    t == typeof(GScaleClampFilter) ||
                    t == typeof(GScaleRandomizeFilter) ||
                    t == typeof(GSlopeConstraintFilter);
        }

        /// <summary>
        /// Check if a brush stroke would overlap any terrains within a terrain group, for terrain culling, e.g: skip painting on a particular terrain if the brush cannot touch it.
        /// </summary>
        /// <param name="groupId">Terrain group ID.</param>
        /// <param name="position">Position of the stroke, in world space.</param>
        /// <param name="radius">Radius of the stroke, in world space. In other word, it's half the width of your brush quad.</param>
        /// <param name="rotation">Rotation of the stroke, in world space.</param>
        /// <returns>A list of overlap test results.</returns>
        public static List<GOverlapTestResult> OverlapTest(int groupId, Vector3 position, float radius, float rotation)
        {
            Vector3[] corners = GCommon.GetBrushQuadCorners(position, radius, rotation);
            return GCommon.OverlapTest(groupId, corners);
        }

        /// <summary>
        /// Transform the 4 corners of brush stroke quad in world space to terrain's normalized space.
        /// </summary>
        /// <param name="t">The terrain object.</param>
        /// <param name="worldCorners">The quad 4 corners, in world space.</param>
        /// <returns>The quad 4 corners, in terrain's normalized space.</returns>
        public static Vector2[] WorldToUvCorners(GStylizedTerrain t, Vector3[] worldCorners)
        {
            Vector2[] uvCorners = new Vector2[worldCorners.Length];
            for (int i = 0; i < uvCorners.Length; ++i)
            {
                uvCorners[i] = t.WorldPointToUV(worldCorners[i]);
            }
            return uvCorners;
        }
    }
}
#endif
