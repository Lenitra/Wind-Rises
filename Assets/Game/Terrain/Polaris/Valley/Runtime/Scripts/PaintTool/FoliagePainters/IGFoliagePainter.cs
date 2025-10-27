#if GRIFFIN
using System.Collections.Generic;

namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// Implement this interface to create custom paint mode for <see cref="GFoliagePainter"/>.<br/>
    /// This should be implement on a non-Unity type (regular C# class, not Unity component or scriptable object) as the GFoliagePainter component will use the 'new' keyword to instantiate this.<br/>
    /// Class with this interface should not store any resource intensive data (store them somewhere else such as static fields or scriptable object).
    /// </summary>
    public interface IGFoliagePainter
    {
        /// <summary>
        /// A string to identify this painter action in History/Backup Tool
        /// </summary>
        string HistoryPrefix { get; }

        /// <summary>
        /// A short description of how to use this painter (mouse button, keyboard modifiers, etc.). This will be displayed in the <see cref="GFoliagePainter"/>'s inspector.
        /// </summary>
        string Instruction { get; }

        /// <summary>
        /// Collection of <see cref="GSpawnFilter"/> that is compatible with this painter. This is for informational purpose only, the editor will use this to display info to user. <br/>
        /// It's your responsibility to pass each spawned instance through the <see cref="GFoliagePainterArgs.Filters"/> list in the <see cref="Paint(GStylizedTerrain, GFoliagePainterArgs)"/> function.
        /// </summary>
        List<System.Type> SuitableFilterTypes { get; }

        /// <summary>
        /// Perform painting. This will be called within the <see cref="GFoliagePainter.Paint(GFoliagePainterArgs)"/> function.
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="args"></param>
        void Paint(GStylizedTerrain terrain, GFoliagePainterArgs args);

        /// <summary>
        /// Indicate the terrain resources that need to be recorded for undo
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        List<GTerrainResourceFlag> GetResourceFlagForHistory(GFoliagePainterArgs args);
    }
}
#endif
