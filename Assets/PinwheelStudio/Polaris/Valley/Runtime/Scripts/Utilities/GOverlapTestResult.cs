#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin
{
    /// <summary>
    /// Storing the result of overlap test, used by many terrain tools.
    /// </summary>
    public struct GOverlapTestResult
    {
        /// <summary>
        /// The terrain that has been tested against.
        /// </summary>
        public GStylizedTerrain Terrain;

        /// <summary>
        /// <see langword="true"/>if the terrain was overlapped.
        /// </summary>
        public bool IsOverlapped;

        /// <summary>
        /// Element at index i is true if a terrain chunk was overlapped, otherwise false.
        /// </summary>
        public bool[] IsChunkOverlapped;
    }
}
#endif
