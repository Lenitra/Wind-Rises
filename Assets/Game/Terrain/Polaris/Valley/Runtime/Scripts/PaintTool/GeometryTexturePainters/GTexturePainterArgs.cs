#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;

namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// Struct containing all neccessary arguments for texture painting.
    /// Most of the fields will be filled by <see cref="GTerrainTexturePainter"/> component, you only need to set HitPoint, MouseEventType and ActionType depending on your game mechanic.
    /// </summary>
    public struct GTexturePainterArgs
    {
        /// <summary>
        /// Position of the brush, typically comes from raycast.
        /// </summary>
        public Vector3 HitPoint { get; set; }
        /// <summary>
        /// Mouse event. Some paint modes behave differently on mouse down/up, e.g: change surface height on mouse down and drag, snap foliage on mouse up.
        /// </summary>
        public GPainterMouseEventType MouseEventType { get; set; }
        /// <summary>
        /// Action type. In editor some paint modes behave differently depending on keyboard modifier (Shift, Ctrl). At runtime you decide the action type based on player interaction.
        /// </summary>
        public GPainterActionType ActionType { get; set; }

        public Vector3[] WorldPointCorners { get; set; }
        public Texture BrushMask { get; set; }
        public float Radius { get; set; }
        public float Rotation { get; set; }
        public float Opacity { get; set; }
        public Color Color { get; set; }
        public int SplatIndex { get; set; }
        public Vector3 SamplePoint { get; set; }
        public string CustomArgs { get; set; }
        public bool ForceUpdateGeometry { get; set; }
        public bool EnableTerrainMask { get; set; }
        public GConditionalPaintingConfigs ConditionalPaintingConfigs { get; set; }
    }
}
#endif
