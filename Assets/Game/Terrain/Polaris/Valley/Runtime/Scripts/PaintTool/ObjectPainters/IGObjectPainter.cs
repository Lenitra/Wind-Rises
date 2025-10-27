#if GRIFFIN
using System.Collections.Generic;

namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// Implement this interface to create custom paint mode for <see cref="GObjectPainter"/>.<br/>
    /// This should be implement on a non-Unity type (regular C# class, not Unity component or scriptable object) as the GObjectPainter component will use the 'new' keyword to instantiate this.<br/>
    /// Class with this interface should not store any resource intensive data (store them somewhere else such as static fields or scriptable object).
    /// </summary>
    public interface IGObjectPainter
    {
        /// <summary>
        /// A short description of how to use this painter (mouse button, keyboard modifiers, etc.). This will be displayed in the <see cref="GObjectPainter"/>'s inspector.
        /// </summary>
        string Instruction { get; }

        /// <summary>
        /// Collection of <see cref="GSpawnFilter"/> that is compatible with this painter. This is for informational purpose only, the editor will use this to display info to user. <br/>
        /// It's your responsibility to pass each spawned instance through the <see cref="GObjectPainterArgs.Filters"/> list in the <see cref="Paint(GStylizedTerrain, GObjectPainterArgs)"/> function.
        /// </summary>
        List<System.Type> SuitableFilterTypes { get; }

        /// <summary>
        /// Perform painting. This will be called within the <see cref="GObjectPainter.Paint(GObjectPainterArgs)"/> function.
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="args"></param>
        void Paint(GStylizedTerrain terrain, GObjectPainterArgs args);
    }
}
#endif
