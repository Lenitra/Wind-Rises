#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// Information about the to-be-spawned foliage or object instance.
    /// </summary>
    public struct GSpawnFilterArgs
    {
        /// <summary>
        /// The terrain it will be spawned on.
        /// </summary>
        public GStylizedTerrain Terrain { get; set; }

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Color Color { get; set; }
        public Vector3 SurfaceNormal { get; set; }
        public Vector2 SurfaceTexcoord { get; set; }

        /// <summary>
        /// Set this to true to prevent the instance to be spawned.
        /// </summary>
        public bool ShouldExclude { get; set; }

        /// <summary>
        /// Create a default object.
        /// </summary>
        /// <returns></returns>
        public static GSpawnFilterArgs Create()
        {
            GSpawnFilterArgs args = new GSpawnFilterArgs();
            args.Position = Vector3.zero;
            args.Rotation = Quaternion.identity;
            args.Scale = Vector3.one;
            args.Color = Color.white;
            args.SurfaceNormal = Vector3.up;
            args.SurfaceTexcoord = Vector2.zero;
            args.ShouldExclude = false;
            return args;
        }
    }
}
#endif
