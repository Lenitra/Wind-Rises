#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// Struct containing all neccessary arguments for foliage painting.
    /// Most of the fields will be filled by <see cref="GFoliagePainter"/> component, you only need to set HitPoint, MouseEventType and ActionType depending on your game mechanic.
    /// </summary>
    public struct GFoliagePainterArgs
    {
        /// <summary>
        /// Position of the brush, typically comes from raycast.
        /// </summary>
        public Vector3 HitPoint { get; set; }
        /// <summary>
        /// Mouse event. Some paint modes behave differently on mouse down/up, e.g: snap foliage, clear dirty state, refresh renderer on mouse up.
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
        public List<int> TreeIndices { get; set; }
        public List<int> GrassIndices { get; set; }
        public string CustomArgs { get; internal set; }
        /// <summary>
        /// Filter components attached to the <see cref="GFoliagePainter"/>, each spawned instance will be passed through this filter list to do adjustments such as scaling, orientation fix, random rotation, etc.<br/>
        /// If you're making a custom painter, it's up to you to pick suitable filter types and ignore others. 
        /// </summary>
        /// <seealso cref="IGFoliagePainter.SuitableFilterTypes"/>
        public GSpawnFilter[] Filters { get; internal set; }
        public bool ShouldCommitNow { get; set; }
    }
}
#endif
