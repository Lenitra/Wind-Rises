#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// Struct containing all neccessary arguments for object painting.
    /// Most of the fields will be filled by <see cref="GObjectPainter"/> component, you only need to set HitPoint, MouseEventType and ActionType depending on your game mechanic.
    /// </summary>
    public struct GObjectPainterArgs
    {
        /// <summary>
        /// Position of the brush, typically comes from raycast.
        /// </summary>
        public Vector3 HitPoint { get; set; }
        /// <summary>
        /// Mouse event. Some paint modes behave differently on mouse down/up, e.g: snap object on mouse up.
        /// </summary>
        public GPainterMouseEventType MouseEventType { get; set; }
        /// <summary>
        /// Action type. In editor some paint modes behave differently depending on keyboard modifier (Shift, Ctrl). At runtime you decide the action type based on player interaction.
        /// </summary>
        public GPainterActionType ActionType { get; set; }  

        public Vector3[] WorldPointCorners { get; internal set; }
        public Texture2D Mask { get; set; }
        public bool EnableTerrainMask { get; set; }
        public float Radius { get; internal set; }
        public float Rotation { get; internal set; }
        public int Density { get; internal set; }
        public float EraseRatio { get; internal set; }
        public float ScaleStrength { get; internal set; }
        public List<GameObject> Prototypes { get; set; }
        public List<int> PrototypeIndices { get; set; }
        public GSpawnFilter[] Filters { get; set; }
        public string CustomArgs { get; internal set; }
    }
}
#endif
