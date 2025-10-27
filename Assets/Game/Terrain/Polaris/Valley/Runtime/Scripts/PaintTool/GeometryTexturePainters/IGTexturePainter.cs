#if GRIFFIN
using System.Collections.Generic;

namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// Implement this interface to create custom paint mode for <see cref="GTerrainTexturePainter"/>.<br/>
    /// This should be implement on a non-Unity type (regular C# class, not Unity component or scriptable object) as the GTerrainTexturePainter component will use the 'new' keyword to instantiate this.<br/>
    /// Class with this interface should not store any resource intensive data (store them somewhere else such as static fields or scriptable object).
    /// </summary>
    /// <seealso cref="IGTexturePainterWithCustomParams"/>
    /// <seealso cref="IGTexturePainterWithLivePreview"/>
    /// <seealso cref="IConditionalPainter"/>
    public interface IGTexturePainter
    {
        /// <summary>
        /// A string to identify this painter action in History/Backup Tool
        /// </summary>
        string HistoryPrefix { get; }
        
        /// <summary>
        /// A short description of how to use this painter (mouse button, keyboard modifiers, etc.). This will be displayed in the <see cref="GTerrainTexturePainter"/>'s inspector.
        /// </summary>
        string Instruction { get; }

        /// <summary>
        /// This will be called within the <see cref="GTerrainTexturePainter.Paint(GTexturePainterArgs)"/> function. Painting process splitted into 2 phase:<br/>
        /// - First it iterates all overlapped terrains and calls <see cref="BeginPainting(GStylizedTerrain, GTexturePainterArgs)"/> on them.<br/>
        /// - After finishing all <see cref="BeginPainting(GStylizedTerrain, GTexturePainterArgs)"/> calls, it iterates all overlapped terrains again and calls <see cref="EndPainting(GStylizedTerrain, GTexturePainterArgs)"/>.<br/>
        /// 
        /// <para/>Use this function to do task such as draw on textures, mark regions dirty, force maximum LODs on mouse down, etc. 
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="args"></param>
        void BeginPainting(GStylizedTerrain terrain, GTexturePainterArgs args);
        
        /// <summary>
        /// This will be called within the <see cref="GTerrainTexturePainter.Paint(GTexturePainterArgs)"/> function. Painting process splitted into 2 phase:<br/>
        /// - First it iterates all overlapped terrains and calls <see cref="BeginPainting(GStylizedTerrain, GTexturePainterArgs)"/> on them.<br/>
        /// - After finishing all <see cref="BeginPainting(GStylizedTerrain, GTexturePainterArgs)"/> calls, it iterates all overlapped terrains again and calls <see cref="EndPainting(GStylizedTerrain, GTexturePainterArgs)"/>.<br/>
        /// 
        /// <para/>Use this function to do task that is expensive or need to read from adjacent terrains such as regenerate surface mesh, snap foliage, resume normal LODs on mouse up, etc. 
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="args"></param>
        void EndPainting(GStylizedTerrain terrain, GTexturePainterArgs args);

        /// <summary>
        /// Indicate the terrain resources that need to be recorded for undo
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        List<GTerrainResourceFlag> GetResourceFlagForHistory(GTexturePainterArgs args);
    }
}
#endif
